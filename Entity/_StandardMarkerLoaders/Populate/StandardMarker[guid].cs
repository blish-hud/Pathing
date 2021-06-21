using System;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_GUID = "guid";

        public Guid Guid { get; set; }

        /// <summary>
        /// guid
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Guid(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.Guid = _packState.UserResourceStates.Population.MarkerPopulationDefaults.Guid;

            { if (collection.TryPopAttribute(ATTR_GUID, out var attribute)) this.Guid = attribute.GetValueAsGuid(); }

            if (this.Guid == Guid.Empty) {
                this.Guid = Guid.NewGuid();
            }
        }

    }
}
