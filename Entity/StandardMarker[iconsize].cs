using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_ICONSIZE = "iconsize";

        /// <summary>
        /// The size of the icon.
        /// </summary>
        public float Size { get; set; }

        /// <summary>
        /// iconsize
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_IconSize(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.Size = _packState.UserResourceStates.Population.MarkerPopulationDefaults.IconSize;

            { if (collection.TryPopAttribute(ATTR_ICONSIZE, out var attribute)) { this.Size = attribute.GetValueAsFloat(attribute.GetValueAsFloat(_packState.UserResourceStates.Population.MarkerPopulationDefaults.IconSize / 2f) * 2f); } }
        }

    }
}
