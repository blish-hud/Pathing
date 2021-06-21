using BhModule.Community.Pathing.Entity;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Behavior {
    public abstract class Behavior<T> : IBehavior where T : IPathingEntity {

        protected readonly T _pathingEntity;

        protected Behavior(T pathingEntity) {
            _pathingEntity = pathingEntity;
        }

        public virtual void Update(GameTime gameTime) { /* NOOP */ }

    }
}
