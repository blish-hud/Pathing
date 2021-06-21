using System.Collections.Generic;
using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.State;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public interface IPathingEntity : IEntity {

        /// <summary>
        /// Filters or modifiers which add functionality to the <see cref="IPathingEntity"/>.
        /// </summary>
        SynchronizedCollection<IBehavior> Behaviors { get; }

        float TriggerRange { get; set; }

        float DistanceToPlayer { get; set; }

        string CategoryNamespace { get; }

        void RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity);

        void Focus();
        void Unfocus();
        void Interact(bool autoTriggered);

        float AnimatedFadeOpacity { get; }

        void FadeIn();

    }

    public static class PathingEntityImpl {

        public static void UpdateBehaviors(this IPathingEntity pathingEntity, GameTime gameTime) {
            lock (pathingEntity.Behaviors.SyncRoot) {
                for (int i = 0; i < pathingEntity.Behaviors.Count; i++) {
                    pathingEntity.Behaviors[i].Update(gameTime);
                }
            }
        }

        public static bool IsFiltered(this IPathingEntity pathingEntity, EntityRenderTarget renderTarget, IPackState packState) {
            // If all pathables are disabled.
            if (!packState.UserConfiguration.GlobalPathablesEnabled.Value) return true;

            if (renderTarget == EntityRenderTarget.World) {
                // If world pathables are disabled.
                if (!packState.UserConfiguration.PackWorldPathablesEnabled.Value) return true;
            } else /*if (EntityRenderTarget.Map.HasFlag(renderTarget))*/ {
                // If map pathables are disabled.
                if (!packState.UserConfiguration.MapPathablesEnabled.Value) return true;

                // TODO: Consider moving IsMapOpen toggles into here.
            }

            // If category is disabled.
            if (packState.CategoryStates.GetNamespaceInactive(pathingEntity.CategoryNamespace)) return true;

            if (packState.UserConfiguration.PackAllowMarkersToAutomaticallyHide.Value) {
                lock (pathingEntity.Behaviors.SyncRoot) {
                    for (int i = 0; i < pathingEntity.Behaviors.Count; i++) {
                        if (pathingEntity.Behaviors[i] is ICanFilter filterable) {
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
