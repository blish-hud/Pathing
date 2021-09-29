using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Behavior.Filter;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Tooltips;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TmfLib.Pathable;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.UI.Controls {
    public class CategoryContextMenuStripItem : ContextMenuStripItem {

        private const int BULLET_SIZE        = 18;
        private const int HORIZONTAL_PADDING = 6;

        private const int TEXT_LEFTPADDING = HORIZONTAL_PADDING + BULLET_SIZE + HORIZONTAL_PADDING;

        private readonly IPackState      _packState;
        private readonly PathingCategory _pathingCategory;

        private readonly List<(Texture2D, string, Action)> _contexts;

        public CategoryContextMenuStripItem(IPackState packState, PathingCategory pathingCategory) {
            _packState       = packState;
            _pathingCategory = pathingCategory;

            _contexts = new List<(Texture2D, string, Action)>();

            BuildCategoryMenu();
            DetectAndBuildContexts();
        }

        private void BuildCategoryMenu() {
            this.Text = _pathingCategory.DisplayName;

            if (_packState.CategoryStates == null) return;

            // TODO: Yikes, filter is getting called a lot.  Let's pass this down from the last time we calcualted it.
            if (_pathingCategory.Any(c => CategoryUtil.UiCategoryIsNotFiltered(c, _packState))) {
                this.Submenu = new CategoryContextMenuStrip(_packState, _pathingCategory);
            }

            if (!_pathingCategory.IsSeparator) {
                this.CanCheck = true;
                this.Checked  = !_packState.CategoryStates.GetCategoryInactive(_pathingCategory);
            }
        }

        private void AddAchievementContext(int achievementId) {
            if (achievementId < 0) return;

            PathingModule.Instance.Gw2ApiManager.Gw2ApiClient.V2.Achievements.GetAsync(achievementId).ContinueWith((achievementTask) => {
                this.BasicTooltipText = $"[Achievement]\r\n\r\n {DrawUtil.WrapText(GameService.Content.DefaultFont14, achievementTask.Result.Description, 300)}\r\n\r\n{DrawUtil.WrapText(GameService.Content.DefaultFont14, achievementTask.Result.Requirement, 300)}";
            }, TaskContinuationOptions.NotOnFaulted);

            _contexts.Add((PathingModule.Instance.ContentsManager.GetTexture(@"png/context/155062.png"), "", () => { }));
            _contexts.Add((PathingModule.Instance.ContentsManager.GetTexture(@"png/context/102365.png"), "", () => { }));
        }

        private void DetectAndBuildContexts() {
            if (_pathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_ID, out var achievementAttr)) {

                // TODO: Add as a context so that multiple characteristics can be accounted for.

                if (!InvariantUtil.TryParseInt(achievementAttr, out int achievementId)) return;

                if (achievementId < 0) return;

                this.Tooltip = new Tooltip(new AchievementTooltipView(achievementId));

                if (_packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value) {
                    this.Enabled = !_packState.AchievementStates.IsAchievementHidden(achievementId, -1);

                    if (!this.Enabled) {
                        this.Checked = false;
                    }
                }
            } else if (_pathingCategory.ExplicitAttributes.TryGetAttribute("tip-description", out var descriptionAttr)) {
                this.Tooltip = new Tooltip(new DescriptionTooltipView(null, descriptionAttr.Value));
            }
        }

        protected override void OnCheckedChanged(CheckChangedEvent e) {
            if (this.Enabled) {
                _packState.CategoryStates.SetInactive(_pathingCategory, !e.Checked);
            }

            base.OnCheckedChanged(e);
        }

        protected override void OnClick(MouseEventArgs e) {
            if (base.CanCheck) {
                // If CTRL is held down when clicked, uncheck all adjacent menu items except for this one.
                if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Ctrl)) {
                    foreach (var childMenuItem in this.Parent?.Children.Where(child => child is CategoryContextMenuStripItem).Cast<CategoryContextMenuStripItem>() ?? Enumerable.Empty<CategoryContextMenuStripItem>()) {
                        childMenuItem.Checked = (childMenuItem == this);
                    }

                    return;
                }

                if (GameService.Input.Keyboard.ActiveModifiers.HasFlag(ModifierKeys.Shift)) {
                    this.Checked = true;
                }
            }

            base.OnClick(e);
        }

        public override void RecalculateLayout() {
            var textSize = GameService.Content.DefaultFont14.MeasureString(this.Text);
            int nWidth   = (int) textSize.Width + TEXT_LEFTPADDING + TEXT_LEFTPADDING + (TEXT_LEFTPADDING * _contexts.Count);

            this.Width = this.Parent != null
                ? Math.Max(this.Parent.Width - 4, nWidth)
                : nWidth;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            base.Paint(spriteBatch, bounds);

            int rightOffset = 18;

            for (int i = 0; i < _contexts.Count; i++) {
                spriteBatch.DrawOnCtrl(this, _contexts[i].Item1, new Rectangle(bounds.Right - rightOffset - (18 * (_contexts.Count - i)) + 1, this.Height / 2 - 8, 16, 16));
            }
        }

    }
}
