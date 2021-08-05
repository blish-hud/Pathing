using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_HEIGHTOFFSET = "heightoffset";

        [Description("Renders the marker the specified amount higher than the actual position.")]
        [Category("Appearance")]
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
