using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Behavior.Modifier;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Models;
using BhModule.Community.Pathing.UI.Tooltips;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib.Pathable;
using Label = Blish_HUD.Controls.Label;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;
using Panel = Blish_HUD.Controls.Panel;

namespace BhModule.Community.Pathing.UI.Controls.TreeNodes
{
    public class PathingCategoryNode : PathingNode
    {
        private bool _active = true;

        public bool Active
        {
            get => _active;
            set
            {
                if (SetProperty(ref _active, value))
                {
                    UpdateChildrenActiveState();
                    UpdateLabelActiveState();
                }
            }
        }

        private readonly IPackState      _packState;
        public           PathingCategory PathingCategory { get; }

        private readonly IList<IPathingEntity> _entities;

        private readonly bool _forceShowAll = false;

        private int _achievementId;
        private int _achievementBit;
        private bool _achievementHidden;

        public bool IsSearchResult { get; init; }

        public PathingCategoryNode(IPackState packState, PathingCategory pathingCategory, bool showForceAll) 
            : base(pathingCategory.DisplayName)
        {
            _packState           = packState;
            this.PathingCategory = pathingCategory;
            _entities            = CategoryUtil.GetAssociatedPathingEntities(pathingCategory, packState.Entities).ToList();
            _forceShowAll        = showForceAll;

            if (pathingCategory.IsSeparator) {
                this.Checkable       = false;
                this.TextColor       = Color.LightYellow;
                BackgroundOpacity    = 0.3f;
                this.ShowIconTooltip = false;
            } else {
                this.Checkable    = true;

                if (_entities.Count <= 0 && (this.PathingCategory.IsSeparator || this.PathingCategory.Count > 0)) {
                    BackgroundOpacity = 0.3f;
                } else {
                    BackgroundOpacity     = 0.05f;
                    BackgroundOpaqueColor = Color.LightYellow;
                }
            }

            DetectAndBuildContexts();

            if (this.Checkable) {
                this.Checked = !_packState.CategoryStates.GetCategoryInactive(pathingCategory);
                UpdateActiveState(this.Checked);
            }
                
            this.CheckedChanged += CheckboxOnCheckedChanged;
        }

        public override void Build() {
            //Set icon before base build
            IconPaddingTop = 4;

            var iconTextures = new List<PathingTexture>(new List<PathingTexture>()
            {
                new PathingTexture() {
                    Icon = AsyncTexture2D.FromAssetId(255302)
                }
            });
            
            if (PathingCategory.IsSeparator) {
                IconTextures   = iconTextures;
                IconSize       = new Point(25, 25);
                IconPaddingTop = 8;
            } else {
                IconTextures = GetEntityTextures().ToList();
            }

            //Tooltip has to be set before base build
            BuildTooltip();

            base.Build();

            //Update label based on active state after building the label
            UpdateLabelActiveState();

            //Details
            BuildAchievementTexture();

            //Properties
            BuildEntityCount();

            //Context menu
            if(this.Menu != null)
                BuildContextMenu();
        }

        private void BuildAchievementTexture() {
            if (_achievementId <= 0) return;

            var achievementIconContainer = new Panel
            {
                Parent = _propertiesPanel,
                Size   = new Point(30, this.Height)
            };

            var tooltipIcon = new Image
            {
                Parent  = achievementIconContainer,
                Size    = new Point(30, 30),
                Top     = 4,
                Texture = _achievementHidden ? PathingModule.Instance.ContentsManager.GetTexture(@"png\155061+255218.png") : AsyncTexture2D.FromAssetId(155061)
            };

            if (_packState.UserConfiguration.PackShowTooltipsOnAchievements.Value) {
                tooltipIcon.Tooltip = new Tooltip(new AchievementTooltipView(_achievementId, _achievementBit));
            } else {
                tooltipIcon.BasicTooltipText = "Achievement tooltips have been disabled in the settings: Pathing Module Settings > Marker Options > Show Tooltips for Achievements";
            }
        }

        private void BuildEntityCount() {
            if (_entities.Count <= 0)
            {
                if (!this.PathingCategory.IsSeparator && this.PathingCategory.Count <= 0) {
                    _ = new Label
                    {
                        Parent           = _propertiesPanel,
                        Text             = "not loaded",
                        Height           = this.PanelHeight,
                        AutoSizeWidth    = true,
                        Font             = GameService.Content.DefaultFont16,
                        TextColor        = StandardColors.Yellow,
                        StrokeText       = true,
                        BasicTooltipText = "No markers or trails have been loaded for this category, likely because there are none for the current map."
                    };
                }
                
                return;
            }

            var markerCount = _entities.OfType<StandardMarker>().Count();

            if (markerCount > 0) {
                var affix = markerCount > 1 ? "markers" : "marker";

                _ = new Label
                {
                    Parent           = _propertiesPanel,
                    Text             = $"{markerCount} {affix}",
                    Height           = this.PanelHeight,
                    AutoSizeWidth    = true,
                    Font             = GameService.Content.DefaultFont16,
                    TextColor        = Color.LightBlue,
                    StrokeText       = true,
                    BasicTooltipText = this.BasicTooltipText
                };
            }

            var trailCount = _entities.OfType<StandardTrail>().Count();

            if (trailCount > 0)
            {
                var affix = trailCount > 1 ? "trails" : "trail";

                _ = new Label
                {
                    Parent = _propertiesPanel,
                    Text = $"{trailCount} {affix}",
                    Height = this.PanelHeight,
                    AutoSizeWidth = true,
                    Font = GameService.Content.DefaultFont16,
                    TextColor = Color.LightYellow,
                    StrokeText = true,
                    BasicTooltipText = this.BasicTooltipText
                };
            }
        }

        private void BuildTooltip() {
            //Note: Tip name is not displayed at the moment
            if (PathingCategory.ExplicitAttributes.TryGetAttribute("tip-description", out var descriptionAttr))
            {
                this.BasicTooltipText = descriptionAttr.Value;
            }
        }

        protected override void BuildContextMenu() {
            base.BuildContextMenu();

            BuildCopyItem();
            BuildCopyPath();

            if(this.IsSearchResult)
                BuildOpenInTree();
        }

        private void BuildCopyItem() {
            if (!this.PathingCategory.IsSeparator                   || 
                !this.PathingCategory.TryGetCopy(out var copyValue) || 
                string.IsNullOrWhiteSpace(copyValue)) return;

            var stripItem = new ContextMenuStripItem($"Copy: {copyValue}")
            {
                Parent = this.Menu
            };

            this.PathingCategory.TryGetCopyMessage(out var copyMessage);

            if(string.IsNullOrWhiteSpace(copyMessage))
                copyMessage = CopyModifier.DEFAULT_COPYMESSAGE;

            stripItem.Click += (_, _) => CopyToClipboard(copyValue, copyMessage);
        }

        private void BuildCopyPath() {
            var stripItem = new ContextMenuStripItem("Copy Path")
            {
                Parent = this.Menu
            };

            stripItem.Click += (_, _) => CopyToClipboard(this.PathingCategory.GetPath(), "Path copied to clipboard");
        }

        private void BuildOpenInTree() {
            var stripItem = new ContextMenuStripItem("Open In Category Tree")
            {
                Parent = this.Menu
            };

            stripItem.Click += (_, _) => {
                var treeView = this.TreeView;

                treeView.LoadNodes();
                treeView.NavigateToPath(this.PathingCategory.GetPath());
            };
        }

        private void CheckboxOnCheckedChanged(object sender, CheckChangedEvent e) {
            if (this.Enabled && !this.PathingCategory.IsSeparator)
            {
                _packState.CategoryStates.SetInactive(this.PathingCategory, !e.Checked);
            }

            UpdateActiveState(e.Checked);

            if (!ParentIsActive() && e.Checked)
            {
                ScreenNotification.ShowNotification("One or more of the parent categories are inactive.",
                                                    ScreenNotification.NotificationType.Warning,
                                                    null,
                                                    2);
            }
        }

        public void UpdateActiveState(bool active) {
            Active = active && ParentIsActive();

            //De-activated if achievement is completed
            if (this.Active && this._achievementHidden && _packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value) {
                this.Active = false;
            }
        }

        public bool ParentIsActive() {
            if (this.Parent is PathingCategoryNode parentNode) {
                return Checked && parentNode.ParentIsActive();
            } 
                
            return this.PathingCategory.ParentIsActive(_packState);
        }

        private void UpdateChildrenActiveState()
        {
            foreach (var child in this.ChildBaseNodes
                                      .OfType<PathingCategoryNode>()
                                      .Where(n => n.Checkable))
            {
                child.UpdateActiveState(child.Checked);
            }
        }

        protected override void OnParentChanged()
        {
            base.OnParentChanged();

            if(Checkable)
                UpdateActiveState(Checked);
        }

        public void UpdateLabelActiveState()
        {
            if (LabelControl != null)
            {
                LabelControl.TextColor  = this.Active ? this.TextColor : Color.LightGray * 0.7f;
                LabelControl.StrokeText = this.Active;
                LabelControl.ShowShadow = !this.Active;
            }
        }

        public int AddSubNodes(bool forceShowAll) {
            if (this.PathingCategory.Count <= 0) return 0;

            (IEnumerable<PathingCategory> subCategories, int skipped) = this.PathingCategory.FilterCategories(_packState, forceShowAll);

            foreach (var subCategory in subCategories) {
                if (subCategory == null) continue;

                //Parent has changed while building
                if (this.Parent == null) break;

                _ = new PathingCategoryNode(_packState, subCategory, forceShowAll)
                {
                    Width          = this.Parent.Width - 14,
                    Parent         = this,
                    Visible        = this.Expanded,
                    IsSearchResult = this.IsSearchResult
                };
            }

            if (skipped > 0 && this.Parent != null && _packState.UserConfiguration.PackShowWhenCategoriesAreFiltered.Value)
            {
                var showAllSkippedCategories = new LabelNode($"{skipped} hidden (click to show)", AsyncTexture2D.FromAssetId(358463))
                {
                    Clickable = true,
                    Width = this.Parent.Width - 14,
                    TextColor = Color.LightYellow,
                    BasicTooltipText = string.Format(Strings.Info_HiddenCategories, _packState.UserConfiguration.PackEnableSmartCategoryFilter.DisplayName),
                    Parent = this
                };

                //TODO Make clickable
                // The control is disabled, so the .Click event won't fire.  We cheat by just doing LeftMouseButtonReleased.
                showAllSkippedCategories.LeftMouseButtonReleased += ShowAllSkippedCategories_LeftMouseButtonReleased;
            }

            return skipped;
        }

        protected override void OnShown(EventArgs e) {

            ClearChildNodes();
            AddSubNodes(_forceShowAll);

            base.OnShown(e);
        }

        protected override void OnHidden(EventArgs e) {
            HideChildren();

            base.OnHidden(e);
        }

        private void ShowAllSkippedCategories_LeftMouseButtonReleased(object sender, MouseEventArgs e)
        {
            ClearChildNodes();

            AddSubNodes(true);
        }

        private IEnumerable<PathingTexture> GetEntityTextures() {
            var uniqueTextures = new HashSet<Texture2D>();

            return _entities
                  .Select(e => e switch {
                       StandardMarker marker => new PathingTexture { Icon = marker.Texture, Tint = marker.Tint },
                       StandardTrail trail => new PathingTexture { Icon   = trail.Texture, Tint  = trail.Tint },
                       _ => null
                   })
                  .Where(pt => pt?.Icon?.Texture != null && uniqueTextures.Add(pt.Icon.Texture));
        }

        private void DetectAndBuildContexts() {
            this.PathingCategory.TryGetAchievementId(out _achievementId);
            this.PathingCategory.TryGetAchievementBit(out _achievementBit);

            if (_packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value) {
                _achievementHidden = _packState.AchievementStates.IsAchievementHidden(_achievementId, _achievementBit);

                if (_achievementHidden)
                    CheckDisabled = true;
            }
        }

        protected override void DisposeControl()
        {
            this.Tooltip?.Dispose();
            this.Menu?.Dispose();

            base.DisposeControl();
        }
    }
}
