using System.Collections.Generic;
using BhModule.Community.Pathing.Behavior;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public interface IPathingEntity : IEntity {

        /// <summary>
        /// Filters or modifiers which add functionality to the <see cref="IPathingEntity"/>.
        /// </summary>
        IList<IBehavior> Behaviors { get; }

        float TriggerRange { get; set; }

        float DistanceToPlayer { get; set; }

        string CategoryNamespace { get; }

        void RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity);

        void Focus();
        void Unfocus();
        void Interact(bool autoTriggered);

        float AnimatedFadeOpacity { get; }

        void FadeIn();

        void Unload();

    }
}
