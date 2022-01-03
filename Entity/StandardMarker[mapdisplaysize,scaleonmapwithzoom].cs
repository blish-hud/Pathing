using BhModule.Community.Pathing.Utility;
using System.Runtime.CompilerServices;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_MAPDISPLAYSIZE     = "mapdisplaysize";
        private const string ATTR_SCALEONMAPWITHZOOM = "scaleonmapwithzoom";

        public float MapDisplaySize     { get; set; }
        public bool  ScaleOnMapWithZoom { get; set; }

        /// <summary>
        /// mapdisplaysize, scaleonmapwithzoom
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_MapScaling(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.MapDisplaySize     = _packState.UserResourceStates.Population.MarkerPopulationDefaults.MapDisplaySize;
            this.ScaleOnMapWithZoom = _packState.UserResourceStates.Population.MarkerPopulationDefaults.ScaleOnMapWithZoom;

            { if (collection.TryPopAttribute(ATTR_MAPDISPLAYSIZE,     out var attribute)) this.MapDisplaySize     = attribute.GetValueAsFloat(this.MapDisplaySize); }
            { if (collection.TryPopAttribute(ATTR_SCALEONMAPWITHZOOM, out var attribute)) this.ScaleOnMapWithZoom = attribute.GetValueAsBool(); }
        }

    }
}
