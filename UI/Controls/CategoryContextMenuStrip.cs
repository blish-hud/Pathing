using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.Utility;
using Blish_HUD.Controls;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.UI.Controls {
    public class CategoryContextMenuStrip : ContextMenuStrip {

        private readonly IPackState      _packState;
        private readonly PathingCategory _pathingCategory;

        private bool _forceShowAll = false;

        public CategoryContextMenuStrip(IPackState packState, PathingCategory pathingCategory, bool forceShowAll) {
            _packState       = packState;
            _pathingCategory = pathingCategory;

            _forceShowAll = forceShowAll;
        }

        // TODO: Make category filtering less janky.

        private (IEnumerable<PathingCategory> SubCategories, int Skipped) GetSubCategories(bool forceShowAll = false) {
            var subCategories = _pathingCategory.Where(cat => cat.LoadedFromPack);

            if (!_packState.UserConfiguration.PackEnableSmartCategoryFilter.Value || forceShowAll) {
                return (subCategories, 0);
            }

            var filteredSubCategories = new List<PathingCategory>();

            PathingCategory lastCategory = null;

            bool lastIsSeparator = false;

            int skipped = 0;

            // We go bottom to top to check if the categories are potentially relevant to categories below.
            foreach (var subCategory in subCategories.Reverse()) {
                if (subCategory.IsSeparator && ((!lastCategory?.IsSeparator ?? false) || lastIsSeparator)) {
                    // If separator was relevant to this category, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = true;
                } else if (CategoryUtil.UiCategoryIsNotFiltered(subCategory, _packState)) {
                    // If category was not filtered, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = false;
                } else {
                    lastIsSeparator = false;
                    if (!subCategory.IsSeparator) skipped++;
                    continue;
                }

                lastCategory = subCategory;
            }
            
            return (Enumerable.Reverse(filteredSubCategories), skipped);
        }

        protected override void OnShown(EventArgs e) {
            (IEnumerable<PathingCategory> subCategories, int skipped) = GetSubCategories(_forceShowAll);

            foreach (var subCategory in subCategories) {
                this.AddMenuItem(new CategoryContextMenuStripItem(_packState, subCategory, _forceShowAll));
            }

            if (skipped > 0 && _packState.UserConfiguration.PackShowWhenCategoriesAreFiltered.Value) {
                var showAllSkippedCategories = new ContextMenuStripItem() {
                    // LOCALIZE: Skipped categories menu item
                    Text = $"{skipped} Categories Are Hidden",
                    Enabled = false,
                    CanCheck = true,
                    BasicTooltipText = string.Format(Strings.Info_HiddenCategories, _packState.UserConfiguration.PackEnableSmartCategoryFilter.DisplayName)
                };

                this.AddMenuItem(showAllSkippedCategories);

                // The control is disabled, so the .Click event won't fire.  We cheat by just doing LeftMouseButtonReleased.
                showAllSkippedCategories.LeftMouseButtonReleased += ShowAllSkippedCategories_LeftMouseButtonReleased;
            }

            base.OnShown(e);
        }

        private void ShowAllSkippedCategories_LeftMouseButtonReleased(object sender, Blish_HUD.Input.MouseEventArgs e) {
            this.ClearChildren();

            (IEnumerable<PathingCategory> subCategories, int skipped) = GetSubCategories(true);

            foreach (var subCategory in subCategories) {
                this.AddMenuItem(new CategoryContextMenuStripItem(_packState, subCategory, true));
            }
        }

        protected override void OnHidden(EventArgs e) {
            foreach (var cmsiChild in this.Children.Select(otherChild => otherChild as ContextMenuStripItem)) {
                cmsiChild?.Submenu?.Hide();
            }

            this.ClearChildren();

            base.OnHidden(e);
        }

    }
}
