using System.Collections.Generic;
using BhModule.Community.Pathing.Behavior;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public abstract class PathingEntity : IPathingEntity {

        protected const float FADEIN_DURATION = 2000;

        public SynchronizedCollection<IBehavior> Behaviors { get; } = new();

        public abstract string CategoryNamespace { get; set; }

        public abstract float TriggerRange { get; set; }

        public float DistanceToPlayer { get; set; }

        public abstract float DrawOrder { get; }

        public abstract void RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity);

        public abstract void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera);

        public virtual void Focus()                      { /* NOOP */ }
        public virtual void Unfocus()                    { /* NOOP */ }
        public virtual void Interact(bool autoTriggered) { /* NOOP */ }
        
        private double _lastFadeStart = 0;
        private bool   _needsFadeIn   = false;

        public float AnimatedFadeOpacity => MathHelper.Clamp((float) (GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds - _lastFadeStart) / FADEIN_DURATION, 0f, 1f);

        public void FadeIn() {
            _needsFadeIn = true;
        }

        public virtual void Update(GameTime gameTime) {
            if (_needsFadeIn) {
                _lastFadeStart = gameTime.ElapsedGameTime.TotalMilliseconds;
                _needsFadeIn   = false;
            }

            this.UpdateBehaviors(gameTime);
        }

    }
}
