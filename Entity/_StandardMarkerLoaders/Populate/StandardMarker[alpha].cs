using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_ALPHA = "alpha";

        public float Alpha { get; set; }

        /// <summary>
        /// alpha
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Alpha(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.Alpha = _packState.UserResourceStates.Population.MarkerPopulationDefaults.Alpha;

            { if (collection.TryPopAttribute(ATTR_ALPHA, out var attribute)) this.Alpha = MathHelper.Clamp(attribute.GetValueAsFloat(this.Alpha), 0f, 1f); }
        }

    }
}
