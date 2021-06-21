using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Utility {
    public static class VectorExtensions {

        public static Vector2 XY(this Microsoft.Xna.Framework.Vector3 vector) {
            return new(vector.X, vector.Y);
        }

        public static Vector2 XY(this System.Numerics.Vector3 vector) {
            return new(vector.X, vector.Y);
        }

    }
}
