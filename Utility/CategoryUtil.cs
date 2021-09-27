using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Utility {
    public static class CategoryUtil {

        /* TODO: Expand this sort of filtering functionality out so that it's:
           1. More robust
           2. Easier to customize
           3. Cleaner
           4. More generic so that other parts of the module can easily use it (as the editor is doing as well).
        */

        public static bool UiCategoryIsNotFiltered(PathingCategory category, IPackState packState, IPathingEntity[] pathingEntities = null) {
            pathingEntities ??= packState.Entities.ToArray();

            return !string.IsNullOrWhiteSpace(category.DisplayName)
                && (packState.UserConfiguration.PackShowCategoriesFromAllMaps.Value && GetCategoryIsNotFiltered(category, Array.Empty<IPathingEntity>(), LoadedCategoryFilter))
                || GetCategoryIsNotFiltered(category, pathingEntities, CurrentMapCategoryFilter);
        }

        public static bool CurrentMapCategoryFilter(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities) {
            IPathingEntity[] searchedEntities = pathingEntities as IPathingEntity[] ?? pathingEntities.ToArray();

            return GetAssociatedPathingEntities(category, searchedEntities).Any(poi => poi.MapId == GameService.Gw2Mumble.CurrentMap.Id)
                || category.Any(c => GetCategoryIsNotFiltered(c, searchedEntities, CurrentMapCategoryFilter));
        }

        public static bool LoadedCategoryFilter(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities) {
            return category.LoadedFromPack
                || category.Any(c => GetCategoryIsNotFiltered(c, pathingEntities, LoadedCategoryFilter));
        }

        public static bool GetCategoryIsNotFiltered(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities, Func<PathingCategory, IEnumerable<IPathingEntity>, bool> categoryFilterFunc) {
            return categoryFilterFunc(category, pathingEntities);
        }

        public static IEnumerable<IPathingEntity> GetAssociatedPathingEntities(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities) {
            return pathingEntities.Where(entity => entity.Category == category);
        }

    }
}
