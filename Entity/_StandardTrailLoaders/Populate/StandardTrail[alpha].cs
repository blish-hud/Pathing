using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_ALPHA = "alpha";

        public float Alpha { get; set; }

        /// <summary>
        /// animspeed
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Alpha(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.Alpha = _packState.UserResourceStates.Population.TrailPopulationDefaults.Alpha;

            { if (collection.TryPopAttribute(ATTR_ALPHA, out var attribute)) this.Alpha = attribute.GetValueAsFloat(this.Alpha); }
        }

    }
}
