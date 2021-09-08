using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_FADENEAR = "fadenear";
        private const string ATTR_FADEFAR  = "fadefar";

        public  float FadeNear { get; set; }
        public float FadeFar { get; set; }

        /// <summary>
        /// fadenear, fadefar
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_FadeNearAndFar(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.FadeNear = _packState.UserResourceStates.Population.TrailPopulationDefaults.FadeNear;
            this.FadeFar  = _packState.UserResourceStates.Population.TrailPopulationDefaults.FadeFar;

            { if (collection.TryPopAttribute(ATTR_FADENEAR, out var attribute)) this.FadeNear = attribute.GetValueAsFloat(this.FadeNear); }
            { if (collection.TryPopAttribute(ATTR_FADEFAR,  out var attribute)) this.FadeFar  = attribute.GetValueAsFloat(this.FadeFar); }
        }

    }
}
