using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_MINIMAPVISIBILITY = "minimapvisibility";
        private const string ATTR_MAPVISIBILITY     = "mapvisibility";
        private const string ATTR_INGAMEVISIBILITY  = "ingamevisibility";

        public bool MiniMapVisibility { get; set; }
        public bool MapVisibility     { get; set; }
        public bool InGameVisibility  { get; set; }

        /// <summary>
        /// minimapvisibility, mapvisibility, ingamevisibility
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_MapVisibility(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.MiniMapVisibility = _packState.UserResourceStates.Population.MarkerPopulationDefaults.MiniMapVisibility;
            this.MapVisibility     = _packState.UserResourceStates.Population.MarkerPopulationDefaults.MapVisibility;
            this.InGameVisibility  = _packState.UserResourceStates.Population.MarkerPopulationDefaults.InGameVisibility;

            { if (collection.TryPopAttribute(ATTR_MINIMAPVISIBILITY, out var attribute)) this.MiniMapVisibility = attribute.GetValueAsBool(); }
            { if (collection.TryPopAttribute(ATTR_MAPVISIBILITY,     out var attribute)) this.MapVisibility     = attribute.GetValueAsBool(); }
            { if (collection.TryPopAttribute(ATTR_INGAMEVISIBILITY,  out var attribute)) this.InGameVisibility  = attribute.GetValueAsBool(); }
        }

    }
}
