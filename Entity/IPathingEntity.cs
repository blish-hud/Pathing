using System;
using System.Collections.Generic;
using BhModule.Community.Pathing.Behavior;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Entity {
    public interface IPathingEntity : IEntity {

        /// <summary>
        /// Filters or modifiers which add functionality to the <see cref="IPathingEntity"/>.
        /// </summary>
        IList<IBehavior> Behaviors { get; }

        bool BehaviorFiltered { get; }

        float TriggerRange { get; set; }

        float DistanceToPlayer { get; set; }

        int MapId { get; }

        PathingCategory Category { get; }

        bool DebugRender { get; }

        int? EditTag { get; }

        bool IsFiltered(EntityRenderTarget renderTarget);

        RectangleF? RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, double offsetX, double offsetY, double scale, float opacity);

        void Focus();
        void Unfocus();
        void Interact(bool autoTriggered);

        float AnimatedFadeOpacity { get; }

        void FadeIn();

        void Unload();

    }
}
