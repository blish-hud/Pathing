using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Controls.Effects;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeView
{
    public class MarkerNode : TreeNodeBase
    {
        private Image _iconControl;
        private Label _labelControl;

        private Color _textColor = Color.White;

        public Color TextColor
        {
            get => _textColor;
            set {
                if (SetProperty(ref _textColor, value) || _labelControl == null) return;
               
                _labelControl.TextColor = value;

                if (_iconControl != null)
                    _iconControl.Tint = value;
                
            }
        }

        private readonly string         _text;
        private readonly AsyncTexture2D _icon;

        private readonly IPackState _packState;
        private readonly PathingCategory _pathingCategory;
        private readonly bool _forceShowAll;

        private bool _selectable;
        public bool Selectable {
            get => _selectable;
            set {
                if (!SetProperty(ref _selectable, value)) return;

                if(value)
                    this._checkbox?.Show();
                else {
                    this._checkbox?.Hide();
                }
            }
        } 

        private bool _selected;

        public bool Selected
        {
            get => _selected;
            private set {
                SetProperty(ref _selected, value);
            }
        }

        private Checkbox _checkbox { get; set; }

        public MarkerNode(IPackState packState, PathingCategory pathingCategory, bool forceShowAll, string text = null)
        {
            _packState       = packState;
            _pathingCategory = pathingCategory;
            _forceShowAll    = forceShowAll;

            this._text = text ?? pathingCategory.DisplayName;
            this.Name  = this._text;

            if (pathingCategory.IsSeparator) {
                this.Selectable   = false;
                this.TextColor    = Color.LightYellow;
                BackgroundOpacity = 0.3f;

            } else {
                this.Selectable            = true;

                BackgroundOpacity     = 0.05f;
                BackgroundOpaqueColor = Color.LightYellow;
            }

            this.EffectBehind = new ScrollingHighlightEffect(this);

            if (this.Selectable)
                this.Selected = !_packState.CategoryStates.GetCategoryInactive(_pathingCategory);

            //TODO: Get icon?
            //_icon = pathingCategory.;

            this.ShowBackground = true;
            this.PanelHeight    = 30;
        }

        public virtual void Build()
        {
            BuildDetailsPanel();
            BuildCheckbox();
            BuildIcon();
	        BuildLabel();
        }


        private FlowPanel DetailsPanel;
        private void BuildDetailsPanel()
        {
            if (DetailsPanel != null) throw new InvalidOperationException("Requirements panel already exists.");

            DetailsPanel = new FlowPanel()
            {
                Parent         = this,
                FlowDirection  = ControlFlowDirection.LeftToRight,
                Size           = new Point(this.ContentRegion.Width, this.PanelHeight),
                Location       = new Point(28,                       1),
                ControlPadding = new Vector2(5, 0),
                CanScroll      = false,
                ShowTint       = DevMode
            };
        }

        private void BuildIcon() {
            if (_icon == null) return;
            
            _iconControl = new Image(_icon)
            {
                Parent = DetailsPanel,
                Size   = new Point(this.Height, this.Height),
            };
        }

        private void BuildLabel()
        {
            _labelControl?.Dispose();

            var iconPadding  = _iconControl == null ? 5 : 35;
            var arrowPadding = this.Expandable ? 25 : 0;

            var xPos = iconPadding + arrowPadding;

            _labelControl = new Label
            {
                Parent        = DetailsPanel,
                Text          = this._text,
                Height        = this.PanelHeight,
                //Width = 600,
                AutoSizeWidth = true,
                Font          = GameService.Content.DefaultFont16,
                TextColor     = this.TextColor,
                StrokeText    = true
            };
        }


        public void BuildCheckbox()
        {
            if (!this.Selectable) return;

            var checkboxContainer = new Panel
            {
                Parent = DetailsPanel,
                Size   = new Point(this.PanelHeight / 2 + 5, this.PanelHeight),
            };

            this._checkbox = new Checkbox
            {
                Parent  = checkboxContainer,
                Left    = 5,
                Size    = new Point(this.PanelHeight, this.PanelHeight),
                Checked = this.Selected
            };

            this._checkbox.CheckedChanged += CheckboxOnCheckedChanged;
        }

        private void CheckboxOnCheckedChanged(object sender, CheckChangedEvent e) {
            if (this.Enabled && !_pathingCategory.IsSeparator)
            {
                _packState.CategoryStates.SetInactive(_pathingCategory, !e.Checked);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            if (this.ChildBaseNodes.Any()) {
                ShowChildren();
                
                return; //TODO: Any reason to not return here?
            }

            var skipped = AddSubNodes(_forceShowAll);

            if (skipped > 0 && _packState.UserConfiguration.PackShowWhenCategoriesAreFiltered.Value)
            {
                var showAllSkippedCategories = new LabelNode($"{skipped} hidden (click to show)", AsyncTexture2D.FromAssetId(358463))
                {   
                    Width = this.Parent.Width - 14,
                    // LOCALIZE: Skipped categories menu item
                    //Enabled          = false,
                    TextColor = Color.LightYellow,
                    //CanCheck         = true, //TODO Make clickable
                    BasicTooltipText = string.Format(Strings.Info_HiddenCategories, _packState.UserConfiguration.PackEnableSmartCategoryFilter.DisplayName),
                    Parent           = this
                };

                showAllSkippedCategories.Build();

                //TODO Make clickable
                // The control is disabled, so the .Click event won't fire.  We cheat by just doing LeftMouseButtonReleased.
                showAllSkippedCategories.LeftMouseButtonReleased += ShowAllSkippedCategories_LeftMouseButtonReleased;
            }

            if (skipped == 0 && !this.ChildBaseNodes.Any()) {
                //Selectable = false;

                //this.AddChild(new LabelNode("No marker packs loaded...")
                //{
                //    Enabled = false,
                //});
            }

            base.OnShown(e);
        }

        private int AddSubNodes(bool forceAll) {

            (IEnumerable<PathingCategory> subCategories, int skipped) = GetSubCategories(forceAll);

            foreach (var subCategory in subCategories)
            {
                var subNode = new MarkerNode(_packState, subCategory, _forceShowAll)
                {
                    Width  = this.Parent.Width - 14,
                    Parent = this
                };

                subNode.Build();
            }

            return skipped;
        }

        private void ShowAllSkippedCategories_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            this.ClearChildNodes();

            AddSubNodes(true);
        }

        protected override void OnHidden(EventArgs e)
        {
            HideChildren();
            
            base.OnHidden(e);
        }

        private (IEnumerable<PathingCategory> SubCategories, int Skipped) GetSubCategories(bool forceShowAll = false)
        {
            // We only show subcategories with a non-empty DisplayName (explicitly setting it to "" will hide it) and
            // was loaded by one of the packs (since those still around from unloaded packs will remain).
            var subCategories = _pathingCategory.Where(cat => cat.LoadedFromPack && cat.DisplayName != "" && !cat.IsHidden);

            if (!_packState.UserConfiguration.PackEnableSmartCategoryFilter.Value || forceShowAll)
            {
                return (subCategories, 0);
            }

            var filteredSubCategories = new List<PathingCategory>();

            PathingCategory lastCategory = null;

            bool lastIsSeparator = false;

            int skipped = 0;

            // We go bottom to top to check if the categories are potentially relevant to categories below.
            foreach (var subCategory in subCategories.Reverse())
            {
                if (subCategory.IsSeparator && ((!lastCategory?.IsSeparator ?? false) || lastIsSeparator))
                {
                    // If separator was relevant to this category, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = true;
                }
                else if (CategoryUtil.UiCategoryIsNotFiltered(subCategory, _packState))
                {
                    // If category was not filtered, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = false;
                }
                else
                {
                    lastIsSeparator = false;
                    if (!subCategory.IsSeparator) skipped++;
                    continue;
                }

                lastCategory = subCategory;
            }

            return (Enumerable.Reverse(filteredSubCategories), skipped);
        }

        protected override void OnClick(MouseEventArgs e)
        {
            //Mouse is over checkbox
            if (this._checkbox != null && this._checkbox.AbsoluteBounds.Contains(Control.Input.Mouse.Position))
                return;

            base.OnClick(e);
        }

        protected override void DisposeControl()
        {
            _iconControl?.Dispose();
            _labelControl?.Dispose();

            base.DisposeControl();
        }
    }
}
