using Blish_HUD;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Utility {
    public static class PickingUtil {

        public static Ray CalculateRay(Point mouseLocation, Matrix view, Matrix projection) {
            var nearPoint = GameService.Graphics.GraphicsDevice.Viewport.Unproject(new Vector3(mouseLocation.X, mouseLocation.Y, 0f),
                                                                                   projection,
                                                                                   view,
                                                                                   Matrix.Identity);

            var farPoint = GameService.Graphics.GraphicsDevice.Viewport.Unproject(new Vector3(mouseLocation.X, mouseLocation.Y, 1f),
                                                                                  projection,
                                                                                  view,
                                                                                  Matrix.Identity);

            var direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public static float? IntersectDistance(BoundingBox box, Point mouseLocation, Matrix view, Matrix projection) {
            return IntersectDistance(box, CalculateRay(mouseLocation, view, projection));
        }

        public static float? IntersectDistance(BoundingBox box, Ray ray) {
            return ray.Intersects(box);
        }

    }
}
