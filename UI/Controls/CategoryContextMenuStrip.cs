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

        public CategoryContextMenuStrip(IPackState packState, PathingCategory pathingCategory) {
            _packState       = packState;
            _pathingCategory = pathingCategory;
        }

        // TODO: Make category filtering less janky.

        private (IEnumerable<PathingCategory> SubCategories, int Skipped) GetSubCategories() {
            var filteredSubCategories = new List<PathingCategory>();

            PathingCategory lastCategory = null;

            bool lastIsSeparator = false;

            int skipped = 0;

            // We go bottom to top to check if the categories are potentially relevant to categories below.
            foreach (var subCategory in _pathingCategory.Reverse()) {
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
            (IEnumerable<PathingCategory> subCategories, int skipped) = GetSubCategories();

            foreach (var subCategory in subCategories) {
                this.AddMenuItem(new CategoryContextMenuStripItem(_packState, subCategory));
            }

            if (skipped > 0 && _packState.UserConfiguration.PackShowWhenCategoriesAreFiltered.Value) {
                this.AddMenuItem(new ContextMenuStripItem() {
                                     // LOCALIZE: Skipped categories menu item
                                     Text    = $"{skipped} Categories Are Hidden",
                                     Enabled = false,
                                     BasicTooltipText = "Categories hidden because they are for markers on a different map.\n\nYou can disable this filter by toggling\nPathing Module Settings > Show Categories From All Maps."
                                 });
            }

            base.OnShown(e);
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
