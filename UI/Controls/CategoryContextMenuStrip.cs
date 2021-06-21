using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.State;
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

        private bool CategoryIsNotFiltered(PathingCategory category) {
            return category.DisplayName != string.Empty
                && _packState.UserConfiguration.PackShowCategoriesFromAllMaps.Value
                || (category.Pathables.Any(poi => poi.MapId == _packState.CurrentMapId)
                 || category.Any(CategoryIsNotFiltered));
        }

        private string BuildTooltipText(PathingCategory category) {
            int markerQty = category.Pathables.Count(poi => poi.Type == PointOfInterestType.Marker);
            int trailQty  = category.Pathables.Count(poi => poi.Type == PointOfInterestType.Trail);

            return $"Contains:\n{markerQty} Markers\n{trailQty} Trails";
        }

        private IEnumerable<PathingCategory> GetSubCategories() {
            var filteredSubCategories = new List<PathingCategory>();

            PathingCategory lastCategory = null;

            bool lastIsSeparator = false;

            foreach (var subCategory in _pathingCategory.Reverse()) {
                if (subCategory.IsSeparator && ((!lastCategory?.IsSeparator ?? false) || lastIsSeparator)) {
                    // If separator was relevant to this category, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = true;
                } else if (CategoryIsNotFiltered(subCategory)) {
                    // If category was not filtered, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = false;
                } else {
                    lastIsSeparator = false;
                    continue;
                }

                lastCategory = subCategory;
            }
            
            return Enumerable.Reverse(filteredSubCategories);
        }

        protected override void OnShown(EventArgs e) {
            foreach (var subCategory in GetSubCategories()) {
                var categoryMenuItem = this.AddMenuItem(new CategoryContextMenuStripItem() { Text = subCategory.DisplayName });

                if (subCategory.Any(CategoryIsNotFiltered)) {
                    categoryMenuItem.Submenu = new CategoryContextMenuStrip(_packState, subCategory);
                }
                
                categoryMenuItem.BasicTooltipText = BuildTooltipText(subCategory);

                if (!subCategory.IsSeparator) {
                    categoryMenuItem.CanCheck       =  true;
                    categoryMenuItem.Checked        =  !_packState.CategoryStates.GetCategoryInactive(subCategory);
                    categoryMenuItem.CheckedChanged += delegate(object sender, CheckChangedEvent checkChangedEvent) {
                        _packState.CategoryStates.SetInactive(subCategory, !checkChangedEvent.Checked);
                    };
                }
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
