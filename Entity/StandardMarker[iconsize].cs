using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_ICONSIZE = "iconsize";

        public float Scale { get; set; }

        /// <summary>
        /// iconsize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_IconSize(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.Scale = _packState.UserResourceStates.Population.MarkerPopulationDefaults.IconSize;

            { if (collection.TryPopAttribute(ATTR_ICONSIZE, out var attribute)) this.Scale = attribute.GetValueAsFloat(this.Scale); }
        }

    }
}
