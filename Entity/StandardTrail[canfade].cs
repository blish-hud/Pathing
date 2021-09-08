using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_CANFADE = "canfade";

        public bool CanFade { get; set; } = true;

        /// <summary>
        /// canfade
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_CanFade(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.CanFade = _packState.UserResourceStates.Population.MarkerPopulationDefaults.CanFade;

            { if (collection.TryPopAttribute(ATTR_CANFADE, out var attribute)) this.CanFade = attribute.GetValueAsBool(); }
        }

    }
}
