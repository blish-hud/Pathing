using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Behavior.Modifier;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Tooltips;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls.TreeNodes
{
    public class PathingCategoryNode : PathingNode
    {
        private readonly IPackState      _packState;
        public           PathingCategory PathingCategory { get; }

        private readonly IList<IPathingEntity> _entities;

        private readonly bool _forceShowAll = false;

        private readonly int _achievementId;

        public bool IsSearchResult { get; init; }

        public PathingCategoryNode(IPackState packState, PathingCategory pathingCategory, bool showForceAll) 
            : base(pathingCategory.DisplayName)
        {
            _packState           = packState;
            this.PathingCategory = pathingCategory;
            _entities            = CategoryUtil.GetAssociatedPathingEntities(pathingCategory, packState.Entities).ToList();
            _forceShowAll        = showForceAll;

            if (pathingCategory.IsSeparator) {
                this.Checkable    = false;
                this.TextColor    = Color.LightYellow;
                BackgroundOpacity = 0.3f;
            } else {
                this.Checkable    = true;

                if (_entities.Count <= 0 && (this.PathingCategory.IsSeparator || this.PathingCategory.Count > 0)) {
                    BackgroundOpacity = 0.3f;
                } else {
                    BackgroundOpacity     = 0.05f;
                    BackgroundOpaqueColor = Color.LightYellow;
                }
            }

            if (this.PathingCategory.TryGetAchievementId(out var achievementId)) 
                _achievementId = achievementId;
            
            if (this.Checkable) 
                this.Checked = !_packState.CategoryStates.GetCategoryInactive(pathingCategory);
            
            this.CheckedChanged += CheckboxOnCheckedChanged;
        }

        public override void Build() {
            //Set icon before base build
            IconPaddingTop = 4;

            if (PathingCategory.IsSeparator) {
                Icon           = AsyncTexture2D.FromAssetId(255302);
                IconSize       = new Point(25, 25);
                IconPaddingTop = 8;
            }
            else
                Icon        = GetIconFile();

            base.Build();

            //Details
            BuildAchievementTexture();

            //Properties
            BuildEntityCount();

            if(this.Menu != null)
                BuildContextMenu();
        }

        private void BuildAchievementTexture() {
            if (_achievementId <= 0) return;

            this.PathingCategory.TryGetAchievementBit(out var achievementBit);

            _ = new Image(AsyncTexture2D.FromAssetId(155062))
            {
                Parent  = _propertiesPanel,
                Size    = new Point(this.Height - 5, this.Height - 5),
                Top     = 2,
                Tooltip = new Tooltip(new AchievementTooltipView(_achievementId, achievementBit)),
            };
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
                        TextColor        = Color.Orange,
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
                    TextColor = Color.LightGreen,
                    StrokeText = true,
                    BasicTooltipText = this.BasicTooltipText
                };
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
                    Width   = this.Parent.Width - 14,
                    Parent  = this,
                    Visible = this.Expanded,
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

        private AsyncTexture2D GetIconFile() {
            return _entities.FirstOrDefault() switch {
                null => null,
                StandardMarker marker => marker.Texture,
                StandardTrail trail => trail.Texture,
                _ => null
            };

        }

        //private void DetectAndBuildContexts()
        //{
        //    if (PathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_ID, out var achievementAttr))
        //    {

        //        var achievementBit = -1;
        //        if (PathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_BIT, out var achievementBitAttr))
        //        {
        //            if (InvariantUtil.TryParseInt(achievementBitAttr, out int achievementBitParsed))
        //            {
        //                achievementBit = achievementBitParsed;
        //            }
        //        }

        //        // TODO: Add as a context so that multiple characteristics can be accounted for.

        //        if (!InvariantUtil.TryParseInt(achievementAttr, out int achievementId)) return;

        //        if (achievementId < 0) return;

        //        if (_packState.UserConfiguration.PackShowTooltipsOnAchievements.Value)
        //        {
        //            this.Tooltip = new Tooltip(new AchievementTooltipView(achievementId, achievementBit));
        //        }

        //        if (_packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value)
        //        {
        //            this.Enabled = !_packState.AchievementStates.IsAchievementHidden(achievementId, achievementBit);

        //            if (!this.Enabled && this._checkbox != null)
        //            {
        //                this._checkbox.Checked = false;
        //            }
        //        }
        //    }
        //    else if (PathingCategory.ExplicitAttributes.TryGetAttribute("tip-description", out var descriptionAttr))
        //    {
        //        this.Tooltip = new Tooltip(new DescriptionTooltipView(null, descriptionAttr.Value));
        //    }
        //}

        protected override void DisposeControl()
        {

            base.DisposeControl();
        }
    }
}
