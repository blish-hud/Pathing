using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using Blish_HUD;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Utility {
    public static class CategoryUtil {

        public static bool GetCategoryIsNotFiltered(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities) {
            IPathingEntity[] searchedEntities = pathingEntities as IPathingEntity[] ?? pathingEntities.ToArray();

            return (GetAssociatedPathingEntities(category, searchedEntities).Any(poi => poi.MapId == GameService.Gw2Mumble.CurrentMap.Id) || category.Any(c => GetCategoryIsNotFiltered(c, searchedEntities)));
        }

        public static IEnumerable<IPathingEntity> GetAssociatedPathingEntities(PathingCategory category, IEnumerable<IPathingEntity> pathingEntities) {
            return pathingEntities.Where(entity => entity.Category == category);
        }

    }
}
