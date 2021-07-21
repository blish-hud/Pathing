using System.Collections.Generic;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.State;
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

        public float DistanceToPlayer { get; set; } = -1;

        public abstract float DrawOrder { get; }
        
        protected readonly IPackState _packState;

        protected PathingEntity(IPackState packState) {
            _packState = packState;
        }

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
                _lastFadeStart = gameTime.TotalGameTime.TotalMilliseconds;
                _needsFadeIn   = false;
            }

            // Only update behaviors found within enabled category namespaces.
            if (!_packState.CategoryStates.GetNamespaceInactive(this.CategoryNamespace)) {
                this.UpdateBehaviors(gameTime);
            }
        }

        private void UpdateBehaviors(GameTime gameTime) {
            lock (this.Behaviors.SyncRoot) {
                for (int i = 0; i < this.Behaviors.Count; i++) {
                    this.Behaviors[i].Update(gameTime);
                }

                if (this.DistanceToPlayer <= this.TriggerRange) {
                    this.Focus();
                } else {
                    this.Unfocus();
                }
            }
        }

        public bool IsFiltered(EntityRenderTarget renderTarget) {
            // If all pathables are disabled.
            if (!_packState.UserConfiguration.GlobalPathablesEnabled.Value) return true;

            if (renderTarget == EntityRenderTarget.World) {
                // If world pathables are disabled.
                if (!_packState.UserConfiguration.PackWorldPathablesEnabled.Value) return true;
            } else /*if (EntityRenderTarget.Map.HasFlag(renderTarget))*/ {
                // If map pathables are disabled.
                if (!_packState.UserConfiguration.MapPathablesEnabled.Value) return true;

                // TODO: Consider moving IsMapOpen toggles into here.
            }

            // If category is disabled.
            if (_packState.CategoryStates.GetNamespaceInactive(this.CategoryNamespace)) return true;

            if (_packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value) {
                lock (this.Behaviors.SyncRoot) {
                    for (int i = 0; i < this.Behaviors.Count; i++) {
                        if (this.Behaviors[i] is ICanFilter filterable) {
                            // If behavior is filtering.
                            if (filterable.IsFiltered()) return true;
                        }
                    }
                }
            }

            return false;
        }

    }
}
