using BhModule.Community.Pathing.Entity;
using Neo.IronLua;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Scripting.Extensions {
    internal static class PathingCategoryScriptExtensions {

        public static bool IsVisible(this PathingCategory category) {
            return !PathingModule.Instance.PackInitiator.PackState.CategoryStates.GetCategoryInactive(category);
        }

        public static void Show(this PathingCategory category) {
            PathingModule.Instance.PackInitiator.PackState.CategoryStates.SetInactive(category, false);
        }

        public static void Hide(this PathingCategory category) {
            PathingModule.Instance.PackInitiator.PackState.CategoryStates.SetInactive(category, true);
        }

        public static LuaTable GetMarkers(this PathingCategory category) {
            return GetMarkers(category, false);
        }

        public static LuaTable GetMarkers(this PathingCategory category, bool getAll) {
            var markers = new LuaTable();

            foreach (var pathable in PathingModule.Instance.PackInitiator.PackState.Entities) {
                if (pathable is StandardMarker marker) {
                    if (pathable.Category == category) {
                        markers.Add(marker);
                    } else if (getAll) {
                        if (category.Root) {
                            markers.Add(marker);
                            continue;
                        }

                        foreach (var parentCategory in pathable.Category.GetParents()) {
                            if (parentCategory == category) {
                                markers.Add(marker);
                                break;
                            }
                        }
                    }
                }
            }

            return markers;
        }

    }
}
