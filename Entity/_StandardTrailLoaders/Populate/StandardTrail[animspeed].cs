using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_ANIMATIONSPEED = "animspeed";

        // TacO default is 1.
        public float AnimationSpeed { get; set; }

        /// <summary>
        /// animspeed
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_AnimationSpeed(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.AnimationSpeed = _packState.UserResourceStates.Population.TrailPopulationDefaults.AnimSpeed;

            { if (collection.TryPopAttribute(ATTR_ANIMATIONSPEED, out var attribute)) this.AnimationSpeed = attribute.GetValueAsFloat(this.AnimationSpeed); }
        }

    }
}
