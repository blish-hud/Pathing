using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Editor.TypeEditors;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_XPOS = "xpos";
        private const string ATTR_YPOS = "ypos";
        private const string ATTR_ZPOS = "zpos";

        [Description("The primary attributes used to determine where to position a marker.")]
        [Category("Appearance")]
        [Editor(typeof(Vector3Editor), typeof(UITypeEditor))]
        public Vector3 Position { get; set; }

        /// <summary>
        /// xpos, ypos, zpos
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Position(AttributeCollection collection, IPackResourceManager resourceManager) {
            float positionX = 0f,
                  positionY = 0f,
                  positionZ = 0f;

            { if (collection.TryPopAttribute(ATTR_XPOS, out var attribute)) positionX = attribute.GetValueAsFloat(); }
            { if (collection.TryPopAttribute(ATTR_YPOS, out var attribute)) positionY = attribute.GetValueAsFloat(); }
            { if (collection.TryPopAttribute(ATTR_ZPOS, out var attribute)) positionZ = attribute.GetValueAsFloat(); }

            // z and y are transposed due to the different coordinate systems
            this.Position = new Vector3(positionX, positionZ, positionY);
        }

    }
}
