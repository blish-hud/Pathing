using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_COLOR = "color"; //
        private const string ATTR_TINT  = "tint";  // Blish HUD only - extended color parsing.

        public Color Tint { get; set; }

        /// <summary>
        /// color, tint
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Tint(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.Tint = _packState.UserResourceStates.Population.TrailPopulationDefaults.Tint;

            { if (collection.TryPopAttribute(ATTR_COLOR, out var attribute)) this.Tint = attribute.GetValueAsColor(this.Tint); }
            { if (collection.TryPopAttribute(ATTR_TINT,  out var attribute)) this.Tint = attribute.GetValueAsColor(this.Tint); }

            if (this.Tint != _packState.UserResourceStates.Population.TrailPopulationDefaults.Tint) {
                this.TrailSampleColor = this.Tint;
            }
        }

    }
}
