using Gw2Sharp.WebApi.V2.Models;

namespace BhModule.Community.Pathing.Utility {
    public static class Gw2SharpModelExtensions {

        public static Microsoft.Xna.Framework.Rectangle ToXnaRectangle(this Rectangle rectangle) {
            return new Microsoft.Xna.Framework.Rectangle((int)rectangle.TopLeft.X,
                                                         (int)rectangle.TopLeft.Y,
                                                         (int)rectangle.Width,
                                                         (int)rectangle.Height);
        }

    }
}
