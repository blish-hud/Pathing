using Blish_HUD;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Utility {
    public static class VectorExtensions {

        public static Vector2 XY(this Microsoft.Xna.Framework.Vector3 vector) {
            return new(vector.X, vector.Y);
        }

        public static Vector2 XY(this System.Numerics.Vector3 vector) {
            return new(vector.X, vector.Y);
        }

        public static Vector3 ToScreenSpace(this Vector3 position, Matrix view, Matrix projection) {
            int screenWidth  = GameService.Graphics.SpriteScreen.Width;
            int screenHeight = GameService.Graphics.SpriteScreen.Height;

            position = Vector3.Transform(position, view);
            position = Vector3.Transform(position, projection);

            float x = position.X / position.Z;
            float y = position.Y / -position.Z;

            x = (x + 1) * screenWidth  / 2;
            y = (y + 1) * screenHeight / 2;

            return new Vector3(x, y, position.Z);
        }

    }
}
