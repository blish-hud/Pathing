using BhModule.Community.Pathing.Entity;
using Neo.IronLua;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Scripting.Extensions {
    internal static class PathingCategoryScriptExtensions {

        private static PackInitiator _packInitiator;

        internal static void SetPackInitiator(PackInitiator packInitiator) {
            _packInitiator = packInitiator;
        }

        public static bool IsVisible(this PathingCategory category) {
            return !_packInitiator.PackState.CategoryStates.GetCategoryInactive(category);
        }

        public static void Show(this PathingCategory category) {
            _packInitiator.PackState.CategoryStates.SetInactive(category, false);
        }

        public static void Hide(this PathingCategory category) {
            _packInitiator.PackState.CategoryStates.SetInactive(category, true);
        }

        public static LuaTable GetMarkers(this PathingCategory category) {
            return GetMarkers(category, false);
        }

        public static LuaTable GetMarkers(this PathingCategory category, bool getAll) {
            var markers = new LuaTable();

            foreach (var pathable in _packInitiator.PackState.Entities) {
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

        public static LuaTable GetTrails(this PathingCategory category) {
            return GetTrails(category, false);
        }

        public static LuaTable GetTrails(this PathingCategory category, bool getAll)  {
            var trails = new LuaTable();

            foreach (var pathable in _packInitiator.PackState.Entities) {
                if (pathable is StandardTrail trail) {
                    if (pathable.Category == category) {
                        trails.Add(trail);
                    } else if (getAll) {
                        if (category.Root) {
                            trails.Add(trail);
                            continue;
                        }

                        foreach (var parentCategory in pathable.Category.GetParents()) {
                            if (parentCategory == category) {
                                trails.Add(trail);
                                break;
                            }
                        }
                    }
                }
            }

            return trails;
        }
    }
}
