using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_HEIGHTOFFSET = "heightoffset";

        public float HeightOffset { get; set; }

        /// <summary>
        /// heightoffset
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_HeightOffset(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.HeightOffset = _packState.UserResourceStates.Population.MarkerPopulationDefaults.HeightOffset;

            { if (collection.TryPopAttribute(ATTR_HEIGHTOFFSET, out var attribute)) this.HeightOffset = attribute.GetValueAsFloat(this.HeightOffset); }
        }

    }
}
