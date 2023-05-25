using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Utility {
    public static class PickingUtil {

        public static Ray CalculateRay(GraphicsDevice graphicsDevice, Point mouseLocation, Matrix view, Matrix projection) {
            var nearPoint = graphicsDevice.Viewport.Unproject(new Vector3(mouseLocation.X, mouseLocation.Y, 0f),
                                                              projection,
                                                              view,
                                                              Matrix.Identity);

            var farPoint = graphicsDevice.Viewport.Unproject(new Vector3(mouseLocation.X, mouseLocation.Y, 1f),
                                                            projection,
                                                            view,
                                                            Matrix.Identity);

            var direction = farPoint - nearPoint;
            direction.Normalize();

            return new Ray(nearPoint, direction);
        }

        public static float? IntersectDistance(GraphicsDevice graphicsDevice, BoundingBox box, Point mouseLocation, Matrix view, Matrix projection) {
            return IntersectDistance(box, CalculateRay(graphicsDevice, mouseLocation, view, projection));
        }

        public static float? IntersectDistance(BoundingBox box, Ray ray) {
            return ray.Intersects(box);
        }

        public static float? IntersectDistance(BoundingSphere sphere, Ray ray) {
            return ray.Intersects(sphere);
        }

    }
}
