using System.Collections.Generic;
using System.ComponentModel;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Entity {
    public abstract class PathingEntity : IPathingEntity {

        protected const float FADEIN_DURATION = 800;

        [Browsable(false)]
        public IList<IBehavior> Behaviors { get; } = new SafeList<IBehavior>();

        [DisplayName("Type")]
        public string CategoryNamespace => this.Category.Namespace;

        public PathingCategory Category { get; }

        public abstract float TriggerRange { get; set; }

        [Browsable(false)]
        public bool DebugRender => _packState.EditorStates.SelectedPathingEntities.Contains(this);

        [Browsable(false)]
        public float DistanceToPlayer { get; set; } = -1;

        [Browsable(false)]
        public abstract float DrawOrder { get; }

        [Browsable(false)]
        public int MapId { get; set; }

        protected readonly IPackState _packState;

        protected PathingEntity(IPackState packState, IPointOfInterest pointOfInterest) {
            this.MapId             = pointOfInterest.MapId;
            this.Category          = pointOfInterest.ParentPathingCategory;

            _packState = packState;
        }

        public abstract RectangleF? RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity);

        public abstract void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera);

        public virtual void Focus()                      { /* NOOP */ }
        public virtual void Unfocus()                    { /* NOOP */ }
        public virtual void Interact(bool autoTriggered) { /* NOOP */ }
        
        private double _lastFadeStart = 0;
        private bool   _needsFadeIn   = true;

        [Browsable(false)]
        public float AnimatedFadeOpacity => MathHelper.Clamp((float) (GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds - _lastFadeStart) / FADEIN_DURATION, 0f, 1f);

        public void FadeIn() {
            _needsFadeIn = true;
        }

        public virtual void Update(GameTime gameTime) {
            // If all markers are disabled, skip updates.
            if (!_packState.UserConfiguration.GlobalPathablesEnabled.Value) return;

            if (_needsFadeIn) {
                _lastFadeStart = gameTime.TotalGameTime.TotalMilliseconds;
                _needsFadeIn   = false;
            }

            // Only update behaviors found within enabled category namespaces.
            if (!_packState.CategoryStates.GetNamespaceInactive(this.CategoryNamespace)) {
                this.UpdateBehaviors(gameTime);
            } else {
                _needsFadeIn = true;
            }
        }

        private void UpdateBehaviors(GameTime gameTime) {
            foreach (var behavior in this.Behaviors) {
                behavior.Update(gameTime);
            }

            if (this.DistanceToPlayer <= this.TriggerRange) {
                this.Focus();
            } else {
                this.Unfocus();
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
                foreach (var behavior in this.Behaviors) {
                    if (behavior is ICanFilter filterable) {
                        // If behavior is filtering.
                        if (filterable.IsFiltered()) return true;
                    }
                }
            }

            return false;
        }

        protected Vector2 GetScaledLocation(double x, double y, double scale, (double X, double Y) offsets) {
            (double mapX, double mapY) = _packState.MapStates.EventCoordsToMapCoords(x, y, this.MapId);

            var scaledLocation = new Vector2((float)((mapX - GameService.Gw2Mumble.UI.MapCenter.X) / scale),
                                             (float)((mapY - GameService.Gw2Mumble.UI.MapCenter.Y) / scale));

            if (!GameService.Gw2Mumble.UI.IsMapOpen && GameService.Gw2Mumble.UI.IsCompassRotationEnabled) {
                scaledLocation = Vector2.Transform(scaledLocation, Matrix.CreateRotationZ((float)GameService.Gw2Mumble.UI.CompassRotation));
            }

            scaledLocation += new Vector2((float)offsets.X, (float)offsets.Y);

            return scaledLocation;
        }

        public virtual void Unload() {
            // Unload and clear behaviors.
            foreach (var behavior in this.Behaviors) {
                behavior.Unload();
            }

            this.Behaviors.Clear();
        }

    }
}
