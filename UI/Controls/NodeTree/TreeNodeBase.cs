using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Glide;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Container = Blish_HUD.Controls.Container;

namespace BhModule.Community.Pathing.UI.Controls.TreeNodes
{
    public abstract class TreeNodeBase : Container
    {
        public bool DevMode = false;

        public TreeView.TreeView TreeView { get; protected set; }

        private AsyncTexture2D _textureArrow = AsyncTexture2D.FromAssetId(155909);

        protected int       NodeDepth         { get; set; }
        public    bool      Expanded          { get; set; }
        public    bool      Expandable        { get; set; } = true;
        public    bool      Clickable        { get; set; } = false;
        public    bool      ShowBackground    { get; set; } = true;
        public    Color     BackgroundOpaqueColor = Color.Black;
        public    float     BackgroundOpacity     = 0.3f;
        public    bool      MouseOverItemDetails { get; set; }
        public    int       PaddingLeft          { get; set; } = 14;
        public    int       PanelHeight          { get; set; } = 0;
        public    Rectangle PanelRectangle       => new Rectangle(2, 2, this.Width, this.PanelHeight - 4);
        public    float     ArrowRotation        { get; set; } = 0f;

        protected float ArrowOpacity = 1f;

        public bool Highlighted { get; set; }

        public Color  HighlightColor = Color.LightYellow;
        public string Name { get; set; }

        public event EventHandler<MouseEventArgs> OnPanelClick;

        public IList<TreeNodeBase> ChildBaseNodes { get; } = new List<TreeNodeBase>();

        private Tween SlideAnimation { get; set; }

        protected TreeNodeBase()
        {
            this.Visible         =  false; //Invisible until parent is set
            this.PropertyChanged += OnPropertyChanged;
        }

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
	        if (e.PropertyName.Equals(nameof(this.Parent), StringComparison.InvariantCultureIgnoreCase))
                OnParentChanged();
        }

        protected virtual void OnParentChanged()
        {
            if (this.Parent == null)
                return;

            switch (this.Parent)
            {
                case TreeView.TreeView treeView:
                    this.TreeView = treeView;
                    this.Visible  = true;
                    break;
                case TreeNodeBase parentNode:
                    this.TreeView  = parentNode.TreeView;
                    this.Visible   = parentNode.Expanded;
                    this.NodeDepth = parentNode.NodeDepth + 1;
                    break;
            }
        }

        protected override void OnChildAdded(ChildChangedEventArgs e)
        {
			if (!(e.ChangedChild is TreeNodeBase newChild)) return;

            this.TreeView.AddNode(newChild);
            this.ChildBaseNodes.Add(newChild);

			ReflowChildLayout(this.ChildBaseNodes);

            base.OnChildAdded(e);
        }

		protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            if (e.ChangedChild is TreeNodeBase removedChild) {
                if(this.TreeView != null)
                    this.TreeView.RemoveNode(removedChild);
                
                this.ChildBaseNodes.Remove(removedChild);
            }

            ReflowChildLayout(this.ChildBaseNodes);

            base.OnChildRemoved(e);
        }

        protected override void OnRightMouseButtonPressed(MouseEventArgs e) {
            base.OnRightMouseButtonPressed(e);

            //Hack to stop parent menus from being displayed
            if (!this.MouseOverItemDetails && this.Menu is { Visible: true })
                this.Menu.Hide();
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            this.MouseOverItemDetails = this.RelativeMousePosition.Y <= this.PanelHeight;

            if (this.MouseOverItemDetails && (this.ChildBaseNodes.Count > 0 && this.Expandable) || this.Clickable)
                this.EffectBehind?.Enable();
            else
                this.EffectBehind?.Disable();

            base.OnMouseMoved(e);
        }

        //Reposition the child container items        
        private int ReflowChildLayout(IEnumerable<TreeNodeBase> containerChildren) {
            var lastBottom = this.PanelHeight;

            foreach (var child in containerChildren)
            {
                child.Location = new Point(this.PaddingLeft, lastBottom);

                child.Width = this.Width - this.PaddingLeft;

                lastBottom = child.Bottom;
            }

            return lastBottom + 5;
        }

        public void ShowChildren()
        {
            foreach (var node in this.ChildBaseNodes)
                node.Show();
        }

        public void HideChildren()
        {
            foreach (var node in this.ChildBaseNodes)
                node.Hide();
        }

        public void Toggle()
        {
            if (this.Expanded)
                Collapse();
            else
                Expand();
        }

        public void Expand()
        {
            if (this.Expanded) return;

            //Have to set before recalculating below
            this.Expanded = true;

            ShowChildren();

            Animate(this.ContentRegion.Bottom);
            RecalculateLayout();
        }

        public void Collapse()
        {
            if (!this.Expanded) return;

            //Have to set before recalculating below
            this.Expanded = false;

            HideChildren();

            Animate(this.PanelHeight);
            RecalculateLayout();
        }

        private void Animate(int newHeight)
        {
            this.SlideAnimation?.CancelAndComplete();

            var rotation = this.Expanded ? MathHelper.PiOver2 : 0f;

            this.Height = newHeight;

            this.SlideAnimation = Animation.Tweener
                                           .Tween(this, new
                                            {
                                                ArrowRotation = rotation
                                            }, 0.3f)
                                           .Ease(Ease.QuadOut);
        }

        public override void RecalculateLayout()
        {
            if(this.EffectBehind != null)
            {
                this.EffectBehind.Size     = new Vector2(this.PanelRectangle.Width, this.PanelRectangle.Height);
                this.EffectBehind.Location = new Vector2(this.PanelRectangle.X,     this.PanelRectangle.Y);
            }

            UpdateContentRegion();

            RecalculateParentLayout();
        }

        public void RecalculateParentLayout()
        {
            //Only recalculate if the parent is expanded
            if (this.Parent is TreeNodeBase parentContainer && parentContainer.Expanded)
            {
                parentContainer.RecalculateLayout();
            }

            if (this.Parent is TreeView.TreeView list)
            {
                list.RecalculateLayout();
            }
        }

        public void UpdateContentRegion() {
            var nodes = this.ChildBaseNodes
                            .Where(n => n.Visible)
                            .ToList();

            int bottomChild = ReflowChildLayout(nodes);

            this.ContentRegion = nodes.Any()
                                     ? new Rectangle(0, 0, this.Width, bottomChild)
                                     : new Rectangle(0, 0, this.Width,      this.PanelHeight);

            this.Height = this.Expanded ? this.ContentRegion.Bottom : this.PanelHeight;
        }

        public void ClearChildNodes() {
            if (this.ChildBaseNodes.Count <= 0) return;

            var controlsQueue = new Queue<Control>(this.ChildBaseNodes);

            while (controlsQueue.Count > 0)
            {
                var control = controlsQueue.Dequeue();

                control.Parent = null;
                control.Dispose();
            }
        }

        protected override void OnClick(MouseEventArgs e) {
            if (!this.MouseOverItemDetails) return;

            if (e.EventType == MouseEventType.LeftMouseButtonReleased && 
                this.MouseOverItemDetails                             && 
                this.ChildBaseNodes.Count > 0 &&
                this.Expandable)
            {
                this.OnPanelClick?.Invoke(this, e);

                Toggle();

                Content.PlaySoundEffectByName($"tab-swap-{RandomUtil.GetRandom(1, 5)}");
            }

            base.OnClick(e);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.ShowBackground || this.Highlighted)
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, this.PanelRectangle,  BackgroundOpaqueColor * BackgroundOpacity);
                
            if(this.Highlighted)
                DrawFrame(spriteBatch);

            if (this.Expandable)
                DrawArrow(spriteBatch);

            base.PaintBeforeChildren(spriteBatch, bounds);
        }

        private void DrawArrow(SpriteBatch spriteBatch)
        {
            if (!this.ChildBaseNodes.Any())
                return;

            spriteBatch.DrawOnCtrl(this,
                _textureArrow,
                new Rectangle(15, this.PanelHeight / 2, 32, 32),
                null,
                Color.White * ArrowOpacity,
                this.ArrowRotation,
                new Vector2((float)8, (float)16));
        }


        private void DrawFrame(SpriteBatch spriteBatch)
        {
            //Draw outline
            var lineColor = HighlightColor * 0.5f;

            var lineSize = 2;

            //Horizontal
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(this.PanelRectangle.X, 0,                           this.PanelRectangle.Width, lineSize), lineColor);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(this.PanelRectangle.X, this.PanelHeight - lineSize, this.PanelRectangle.Width,      lineSize), lineColor);

            //Vertical
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, 0, lineSize, this.PanelRectangle.Height + (lineSize * 2)), lineColor);
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(this.PanelRectangle.Width                  - lineSize, 0, lineSize, this.PanelHeight), lineColor);
        }

        protected override void DisposeControl()
        {
	        this.PropertyChanged -= OnPropertyChanged;

            this.SlideAnimation = null;

            this.TreeView     = null;
            this.OnPanelClick = null;
            
            this.Tooltip?.Dispose();
            this.Menu?.Dispose();

            base.DisposeControl();
        }
    }
}
