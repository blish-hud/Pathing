using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using TmfLib.Attributes;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_XPOS = "xpos";
        private const string ATTR_YPOS = "ypos";
        private const string ATTR_ZPOS = "zpos";

        public Vector3 Position { get; set; }

        /// <summary>
        /// xpos, ypos, zpos
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Position(AttributeReader collection, IPackResourceManager resourceManager) {
            float positionX = 0f,
                  positionY = 0f,
                  positionZ = 0f;

            collection.TryConsumeFloat(AttrKeys.XPos, out positionX);
            collection.TryConsumeFloat(AttrKeys.YPos, out positionY);
            collection.TryConsumeFloat(AttrKeys.ZPos, out positionZ);

            // z and y are transposed due to the different coordinate systems
            this.Position = new Vector3(positionX, positionZ, positionY);
        }

    }
}
