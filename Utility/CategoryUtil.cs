using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Utility {
    public static class CategoryUtil {

        public static bool ParentIsActive(this PathingCategory category, IPackState packState)
        {
            var active = !packState.CategoryStates.GetCategoryInactive(category);

            if (category.Parent != null)
            {
                return active && category.Parent.ParentIsActive(packState);
            }

            //Return true if parent is not a node
            return active;
        }

        public static IEnumerable<PathingCategory> FlattenCategories(PathingCategory category)
        {
            yield return category;

            foreach (var subCategory in category)
            {
                foreach (var subSubCategory in FlattenCategories(subCategory))
                {
                    yield return subSubCategory;
                }
            }
        }

        public static string GetPath(this PathingCategory category) {
            return $".{category.Namespace}";
        }

        public static (IEnumerable<PathingCategory>, int skipped) FilterCategories(this IEnumerable<PathingCategory> categories, IPackState packState, bool forceShowAll = false) {
            if (categories == null || packState == null) return (null, 0);

            var subCategories = categories.Where(cat => cat.LoadedFromPack && cat.DisplayName != "" && !cat.IsHidden);

            if (!packState.UserConfiguration.PackEnableSmartCategoryFilter.Value || forceShowAll)
            {
                return (subCategories, 0);
            }

            var filteredSubCategories = new List<PathingCategory>();

            PathingCategory lastCategory = null;

            bool lastIsSeparator = false;

            int skipped = 0;

            // We go bottom to top to check if the categories are potentially relevant to categories below.
            foreach (var subCategory in categories.Reverse())
            {
                if (subCategory.IsSeparator && ((!lastCategory?.IsSeparator ?? false) || lastIsSeparator))
                {
                    // If separator was relevant to this category, we include it.
                    filteredSubCategories.Add(subCategory);
                    lastIsSeparator = true;
                }
                //Check if the category has any loaded/visible children recursively
                else if (subCategory.HasVisibleChildren(packState, true))
                {
                    // If category has visible children, we include it
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

        public static bool HasVisibleChildren(this PathingCategory category, IPackState packState, bool recursively = false) {

            if (packState == null || 
                string.IsNullOrWhiteSpace(category.DisplayName) || 
                !category.LoadedFromPack) return false;

            var mapId = GameService.Gw2Mumble.CurrentMap.Id;

            var hasVisibleEntities = packState.Entities
                                              .ToArray()
                                              .Any(e => e.MapId == mapId && e.Category == category);

            if (hasVisibleEntities) return true;

            if (recursively) {
                foreach (var childCategory in category)
                {
                    if (childCategory.HasVisibleChildren(packState, true))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /* TODO: Expand this sort of filtering functionality out so that it's:
           1. More robust
           2. Easier to customize
           3. Cleaner
           4. More generic so that other parts of the module can easily use it (as the editor is doing as well).
        */

        public static bool UiCategoryIsNotFiltered(PathingCategory category, IPackState packState, IPathingEntity[] pathingEntities = null)
        {
            pathingEntities ??= packState.Entities.ToArray();

            return !string.IsNullOrWhiteSpace(category.DisplayName)
                && (!packState.UserConfiguration.PackEnableSmartCategoryFilter.Value && GetCategoryIsNotFiltered(category, Array.Empty<IPathingEntity>(), LoadedCategoryFilter))
                || GetCategoryIsNotFiltered(category, pathingEntities, CurrentMapCategoryFilter);
        }

        public static bool CurrentMapCategoryFilter(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities)
        {
            IPathingEntity[] searchedEntities = pathingEntities as IPathingEntity[] ?? pathingEntities.ToArray();

            return GetAssociatedPathingEntities(category, searchedEntities).Any(poi => poi.MapId == GameService.Gw2Mumble.CurrentMap.Id)
                || category.Any(c => GetCategoryIsNotFiltered(c, searchedEntities, CurrentMapCategoryFilter));
        }

        public static bool LoadedCategoryFilter(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities)
        {
            return category.LoadedFromPack
                || category.Any(c => GetCategoryIsNotFiltered(c, pathingEntities, LoadedCategoryFilter));
        }

        public static bool GetCategoryIsNotFiltered(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities, Func<PathingCategory, IEnumerable<IPathingEntity>, bool> categoryFilterFunc)
        {
            return categoryFilterFunc(category, pathingEntities);
        }

        public static IEnumerable<IPathingEntity> GetAssociatedPathingEntities(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities)
        {
            return pathingEntities.Where(entity => entity.Category == category);
        }
    }
}
