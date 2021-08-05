using System.ComponentModel;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_COLOR = "color";
        private const string ATTR_TINT  = "tint";  // Blish HUD only - extended color parsing.

        [Description("Tints the marker or trail with the color provided. Powerful when reusing existing marker icons or trail textures to make them differ in color.")]
        [Category("Appearance")]
        public Color Tint { get; set; }

        /// <summary>
        /// color, tint
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Tint(AttributeCollection collection, IPackResourceManager resourceManager) {
            this.Tint = _packState.UserResourceStates.Population.MarkerPopulationDefaults.Tint;

            { if (collection.TryPopAttribute(ATTR_COLOR, out var attribute)) this.Tint = attribute.GetValueAsColor(this.Tint); }

            // Apply second to override specifically for Blish HUD.
            { if (collection.TryPopAttribute(ATTR_TINT, out var attribute)) this.Tint = attribute.GetValueAsColor(this.Tint); }
        }

    }
}
