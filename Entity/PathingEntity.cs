using System;
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

        [Description("A unique identifier used to track the state of certain behaviors between launch sessions.")]
        [Category("Behavior")]
        public Guid Guid { get; set; }

        [Browsable(false)]
        public SafeList<IBehavior> Behaviors { get; } = new SafeList<IBehavior>();

        public PathingCategory Category { get; }

        public abstract float TriggerRange { get; set; }

        [Browsable(false)]
        public bool DebugRender => this.EditTag != null && _packState.EditorStates.SelectedPathingEntities.Contains(this);

        [Description("Indicates the distance the entity is from the player.")]
        [Category("State Debug")]
        public float DistanceToPlayer { get; set; } = -1;

        [Browsable(false)]
        public abstract float DrawOrder { get; }

        [Description("Indicates if the entity is currently filtered.")]
        [Category("State Debug")]
        public bool BehaviorFiltered { get; private set; }

        [Browsable(false)]
        public int? EditTag { get; protected set; }

        [Browsable(false)]
        public int MapId { get; set; }

        protected readonly IPackState _packState;

        protected PathingEntity(IPackState packState, IPointOfInterest pointOfInterest) {
            _packState    = packState;

            this.MapId    = pointOfInterest.MapId;
            this.Category = pointOfInterest.ParentPathingCategory ?? _packState.RootCategory;
        }

        public abstract RectangleF? RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, double offsetX, double offsetY, double scale, float opacity);

        public abstract void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera);

        public virtual void Focus()                      { /* NOOP */ }
        public virtual void Unfocus()                    { /* NOOP */ }
        public virtual void Interact(bool autoTriggered) { /* NOOP */ }

        private double _lastFadeStart = 0;
        private bool   _needsFadeIn   = true;

        bool _wasInactive = false;

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
            if (!_packState.CategoryStates.GetNamespaceInactive(this.Category.Namespace)) {
                if (_wasInactive) {
                    OnCategoryActivated();
                }

                UpdateBehaviors(gameTime);
                _wasInactive = false;
            } else {
                if (!_wasInactive) {
                    OnCategoryDeactivated();
                }

                _needsFadeIn = true;
                _wasInactive = true;
            }
        }

        // Rising edge
        protected virtual void OnCategoryActivated() { /* NOOP */ }


        // Falling edge
        protected virtual void OnCategoryDeactivated() {
            this.Unfocus();
        }

        private void UpdateBehaviors(GameTime gameTime) {
            bool filtered = false;

            foreach (var behavior in this.Behaviors) {
                behavior.Update(gameTime);

                if (behavior is ICanFilter filter) {
                    filtered |= filter.IsFiltered();
                }
            }

            this.BehaviorFiltered = _packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value && filtered;

            HandleBehavior();
        }

        public abstract void HandleBehavior();

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
            if (_packState.CategoryStates.GetNamespaceInactive(this.Category.Namespace)) return true;

            return this.BehaviorFiltered && !_packState.UserConfiguration.PackShowHiddenMarkersReducedOpacity.Value;
        }

        protected Vector2 GetScaledLocation(double x, double y, double scale, double offsetX, double offsetY) {
            _packState.MapStates.EventCoordsToMapCoords(x, y, out double mapX, out double mapY);

            var scaledLocation = new Vector2((float)((mapX - _packState.CachedMumbleStates.MapCenterX) / scale),
                                             (float)((mapY - _packState.CachedMumbleStates.MapCenterY) / scale));

            if (!_packState.CachedMumbleStates.IsMapOpen && _packState.CachedMumbleStates.IsCompassRotationEnabled) {
                scaledLocation = Vector2.Transform(scaledLocation, Matrix.CreateRotationZ((float)_packState.CachedMumbleStates.CompassRotation));
            }

            scaledLocation += new Vector2((float)offsetX, (float)offsetY);

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
