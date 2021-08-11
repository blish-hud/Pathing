using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Editor.Entity {
    public interface IAxisHandle {

        Vector3 Origin { get; set; }

        void HandleActivated(Ray ray);

    }
}
