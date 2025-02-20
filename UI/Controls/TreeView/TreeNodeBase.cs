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

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public abstract class TreeNodeBase : Container
    {
        public bool DevMode = false;

        public TreeView TreeView { get; protected set; }

        private AsyncTexture2D _textureArrow = AsyncTexture2D.FromAssetId(155909);

        public    int       NodeIndex         { get; set; }
        protected int       NodeDepth         { get; set; }
        public    bool      Expanded          { get; set; }
        public    bool      Expandable        { get; set; } = true;
        public    bool      ExpandedByDefault { get; set; } = false;
        public    bool      ShowBackground    { get; set; } = true;
        public    Color     BackgroundOpaqueColor = Color.Black;
        public    float     BackgroundOpacity     = 0.3f;
        public    bool      MouseOverItemDetails { get; set; }
        public    int       PaddingLeft          { get; set; } = 14;
        public    int       PanelHeight          { get; set; } = 0;
        public    Rectangle PanelRectangle       => new Rectangle(2, 2, this.Width, this.PanelHeight - 4);
        public    float     ArrowRotation        { get; set; } = 0f;

        protected float ArrowOpacity = 1f;

        public string Name { get; set; }

        public event EventHandler<MouseEventArgs> OnPanelClick;

        public IList<TreeNodeBase> ChildBaseNodes { get; } = new List<TreeNodeBase>();

        private Tween SlideAnimation { get; set; }

        protected TreeNodeBase()
        {
            this.Visible         =  false; //Invisible until parent is set
            this.PropertyChanged += OnPropertyChanged;
        }

        public virtual void Initialize()
        {
            if (this.ExpandedByDefault)
                Expand();
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
                case TreeView treeView:
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

            this.ChildBaseNodes.Add(newChild);

			ReflowChildLayout(this.ChildBaseNodes);
        }

		protected override void OnChildRemoved(ChildChangedEventArgs e)
        {
            if(e.ChangedChild is TreeNodeBase newChild)
				this.ChildBaseNodes.Remove(newChild);

	        base.OnChildRemoved(e);
        }

        protected override void OnMouseMoved(MouseEventArgs e)
        {
            var height = this.PanelHeight;

            this.MouseOverItemDetails = this.RelativeMousePosition.Y <= height;

            if (this.MouseOverItemDetails && this.ChildBaseNodes.Any() && this.Expandable)
                this.EffectBehind?.Enable();
            else
                this.EffectBehind?.Disable();

            base.OnMouseMoved(e);
        }

        //Reposition the child container items        
        private int ReflowChildLayout(IEnumerable<TreeNodeBase> containerChildren)
        {
            var lastBottom = this.PanelHeight;

            foreach (var child in containerChildren)
            {
                child.Location = new Point(this.PaddingLeft, lastBottom);
       
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
            RecalculateParentLayout();
        }

        public void Collapse()
        {
            if (!this.Expanded) return;

            //Have to set before recalculating below
            this.Expanded = false;

            HideChildren();

            Animate(this.PanelHeight);
            RecalculateParentLayout();
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

            if (this.Parent is TreeView list)
            {
                list.RecalculateLayout();
            }
        }

        private void UpdateContentRegion()
        {
            int bottomChild = ReflowChildLayout(this.ChildBaseNodes);

            this.ContentRegion = this.ChildBaseNodes.Any()
                                     ? new Rectangle(0, 0, this.Width, bottomChild)
                                     : new Rectangle(0, 0, this.Width,      this.PanelHeight);

            this.Height = this.Expanded ? this.ContentRegion.Bottom : this.PanelHeight;
        }

        public void ClearChildNodes() {
            var controlsQueue = new Queue<Control>(this.ChildBaseNodes);

            while (controlsQueue.Count > 0)
            {
                var control = controlsQueue.Dequeue();

                control.Parent = null;
                control.Dispose();
            }
        }

        protected override void OnClick(MouseEventArgs e)
        {
            if (e.EventType == MouseEventType.LeftMouseButtonReleased && 
                this.MouseOverItemDetails                             && this.ChildBaseNodes.Any() &&
                this.Expandable)
            {
                this.OnPanelClick?.Invoke(this, e);

                Toggle();
                
                //TODO: Play sound
                //ServiceContainer.AudioService.PlayMenuClick();
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds)
        {
            if (this.ShowBackground)
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, this.PanelRectangle, BackgroundOpaqueColor * BackgroundOpacity);

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
