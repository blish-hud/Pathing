using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Microsoft.Xna.Framework;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_ROTATE  = "rotate";
        private const string ATTR_ROTATEX = ATTR_ROTATE + "-x";
        private const string ATTR_ROTATEY = ATTR_ROTATE + "-y";
        private const string ATTR_ROTATEZ = ATTR_ROTATE + "-z";

        [DisplayName("Rotate")]
        [Description("Allows you to statically rotate a marker instead of it automatically facing the player.")]
        [Category("Appearance")]
        public Vector3 RotationXyz { get; set; }
        
        /// <summary>
        /// rotate, rotate-x, rotate-y, rotate-z
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Rotation(AttributeCollection collection, IPackResourceManager resourceManager) {
            float rotationX = _packState.UserResourceStates.Population.MarkerPopulationDefaults.RotateXyz.X,
                  rotationY = _packState.UserResourceStates.Population.MarkerPopulationDefaults.RotateXyz.Y,
                  rotationZ = _packState.UserResourceStates.Population.MarkerPopulationDefaults.RotateXyz.Z;

            { if (collection.TryPopAttribute(ATTR_ROTATEX, out var attribute)) rotationX = MathHelper.ToRadians(attribute.GetValueAsFloat()); }
            { if (collection.TryPopAttribute(ATTR_ROTATEY, out var attribute)) rotationY = MathHelper.ToRadians(attribute.GetValueAsFloat()); }
            { if (collection.TryPopAttribute(ATTR_ROTATEZ, out var attribute)) rotationZ = MathHelper.ToRadians(attribute.GetValueAsFloat()); }

            { // Support <... rotation="1,2,3" ...> notation
                if (collection.TryPopAttribute(ATTR_ROTATE, out var attribute)) {
                    float[] rotations = attribute.GetValueAsFloats().Select(MathHelper.ToRadians).ToArray();

                    if (rotations.Length > 0) rotationX = rotations[0];
                    if (rotations.Length > 1) rotationY = rotations[1];
                    if (rotations.Length > 2) rotationZ = rotations[2];
                }
            }

            this.RotationXyz = new Vector3(rotationX, rotationY, rotationZ);
        }

    }
}
