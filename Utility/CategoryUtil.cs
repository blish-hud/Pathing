using System.Linq;
using Blish_HUD;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Utility {
    public static class CategoryUtil {

        public static bool GetCategoryIsNotFiltered(PathingCategory category) {
            return (category.Pathables.Any(poi => poi.MapId == GameService.Gw2Mumble.CurrentMap.Id) || category.Any(GetCategoryIsNotFiltered));
        }

    }
}
