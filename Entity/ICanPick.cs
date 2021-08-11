using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Entity {
    public interface ICanPick {

        bool RayIntersects(Ray ray);

    }
}