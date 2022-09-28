using System.ComponentModel;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_ALPHA = "alpha";

        [Description("Specifies the opacity of a marker or trail where 1 is opaque and 0 is fully transparent. Values outside of this range will be clamped to this range.")]
        [Category("Appearance")]
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
