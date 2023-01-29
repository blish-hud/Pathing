using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Behavior.Filter;
using BhModule.Community.Pathing.Behavior.Modifier;
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

        private readonly bool _forceShowAll = false;

        private readonly List<(Texture2D, string, Action)> _contexts;

        public CategoryContextMenuStripItem(IPackState packState, PathingCategory pathingCategory, bool forceShowAll) {
            _packState       = packState;
            _pathingCategory = pathingCategory;
            
            _contexts     = new List<(Texture2D, string, Action)>();
            _forceShowAll = forceShowAll;

            BuildCategoryMenu();
            DetectAndBuildContexts();
        }

        private void BuildCategoryMenu() {
            this.Text = _pathingCategory.DisplayName;

            if (_packState.CategoryStates == null) return;

            // TODO: Yikes, filter is getting called a lot.  Let's pass this down from the last time we calcualted it.
            if (_forceShowAll && _pathingCategory.Any() 
             || _pathingCategory.Any(c => CategoryUtil.UiCategoryIsNotFiltered(c, _packState))) {
                this.Submenu = new CategoryContextMenuStrip(_packState, _pathingCategory, _forceShowAll);
            }

            /*if (!_pathingCategory.IsSeparator) {*/
                this.CanCheck = true;
                this.Checked  = !_packState.CategoryStates.GetCategoryInactive(_pathingCategory);
            /*}*/
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

                var achievementBit = -1;
                if (_pathingCategory.TryGetAggregatedAttributeValue(AchievementFilter.ATTR_BIT, out var achievementBitAttr))
                {
                    if (InvariantUtil.TryParseInt(achievementBitAttr, out int achievementBitParsed))
                    {
                        achievementBit = achievementBitParsed;
                    }
                }

                // TODO: Add as a context so that multiple characteristics can be accounted for.

                if (!InvariantUtil.TryParseInt(achievementAttr, out int achievementId)) return;

                if (achievementId < 0) return;

                this.Tooltip = new Tooltip(new AchievementTooltipView(achievementId, achievementBit));

                if (_packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value) {
                    this.Enabled = !_packState.AchievementStates.IsAchievementHidden(achievementId, achievementBit);

                    if (!this.Enabled) {
                        this.Checked = false;
                    }
                }
            } else if (_pathingCategory.ExplicitAttributes.TryGetAttribute("tip-description", out var descriptionAttr)) {
                this.Tooltip = new Tooltip(new DescriptionTooltipView(null, descriptionAttr.Value));
            }
        }

        protected override void OnCheckedChanged(CheckChangedEvent e) {
            if (this.Enabled && !_pathingCategory.IsSeparator) {
                _packState.CategoryStates.SetInactive(_pathingCategory, !e.Checked);
            }

            base.OnCheckedChanged(e);
        }

        private bool TryGetCopyDetails(out string copyValue, out string copyMessage) {
            copyValue   = string.Empty;
            copyMessage = CopyModifier.DEFAULT_COPYMESSAGE;

            if (_pathingCategory.ExplicitAttributes.TryGetAttribute(CopyModifier.PRIMARY_ATTR_NAME, out var copyValueAttr)) {
                copyValue = copyValueAttr.GetValueAsString();

                if (_packState.UserConfiguration.PackMarkerConsentToClipboard.Value == MarkerClipboardConsentLevel.Never) {
                    // The player has disabled clipboard access.
                    return false;
                }


                if (_pathingCategory.ExplicitAttributes.TryGetAttribute(CopyModifier.ATTR_MESSAGE, out var copyMessageAttr)) {
                    copyMessage = copyMessageAttr.GetValueAsString();
                }

                copyMessage = string.Format(copyMessage, copyValue);
                return true;
            }

            return false;
        }

        protected override void OnClick(MouseEventArgs e) {
            if (_pathingCategory.IsSeparator && TryGetCopyDetails(out string copyValue, out string copyMessage)) {
                ClipboardUtil.WindowsClipboardService.SetTextAsync(copyValue).ContinueWith(t => {
                    if (t.IsCompleted && t.Result) {
                        ScreenNotification.ShowNotification(string.Format(copyMessage, copyValue),
                                                            ScreenNotification.NotificationType.Info,
                                                            null,
                                                            2);
                    }
                });
            } else if (!_pathingCategory.IsSeparator) {
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
            // Eww - this is a work around to let us hide the checkbox.
            // We are using the checkbox to get around a bug that prevents clicks from firing.
            this.CanCheck = !_pathingCategory.IsSeparator;
            base.Paint(spriteBatch, bounds);
            this.CanCheck = true;

            int rightOffset = 18;

            for (int i = 0; i < _contexts.Count; i++) {
                spriteBatch.DrawOnCtrl(this, _contexts[i].Item1, new Rectangle(bounds.Right - rightOffset - (18 * (_contexts.Count - i)) + 1, this.Height / 2 - 8, 16, 16));
            }
        }

    }
}