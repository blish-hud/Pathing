using System;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const double ABOVEBELOWINDICATOR_THRESHOLD = 2.0d;
        private const float  VERTICALOFFSET_THRESHOLD      = 30f;

        private static readonly Texture2D _aboveTexture = PathingModule.Instance.ContentsManager.GetTexture(@"png\1130638.png");
        private static readonly Texture2D _belowTexture = PathingModule.Instance.ContentsManager.GetTexture(@"png\1130639.png");
        
        public override void RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity) {
            if (this.IsFiltered(EntityRenderTarget.Map, _packState) || this.Texture == null) return;

            if (!this.MapVisibility && GameService.Gw2Mumble.UI.IsMapOpen) return;
            if (!this.MiniMapVisibility && !GameService.Gw2Mumble.UI.IsMapOpen) return;

            var scaledCoords = _packState.MapStates.EventCoordsToMapCoords(this.Position.X, this.Position.Y);

            var location = new Vector2((float) (scaledCoords.X / scale - offsets.X),
                                       (float) (scaledCoords.Y / scale - offsets.Y));

            if (!bounds.Contains(location)) return;

            spriteBatch.Draw(this.Texture,
                             location,
                             null,
                             this.Tint * opacity,
                             GameService.Gw2Mumble.UI.IsCompassRotationEnabled ? (float) GameService.Gw2Mumble.UI.CompassRotation : 0f,
                             new Vector2(this.Texture.Width / 2f, this.Texture.Height / 2f),
                             (float) (0.3f / scale),
                             SpriteEffects.None,
                             0f);

            // Draw above or below indicator, if applicable.
            // We skip if zoomed out too far or if feature is disabled by the user.
            if (_packState.UserConfiguration.MapShowAboveBelowIndicators.Value && scale < ABOVEBELOWINDICATOR_THRESHOLD) {
                float diff = this.Position.Z - GameService.Gw2Mumble.PlayerCharacter.Position.Z;
                
                if (Math.Abs(diff) < VERTICALOFFSET_THRESHOLD) return;

                spriteBatch.Draw(diff > 0 ? _aboveTexture : _belowTexture,
                                 new Rectangle(location.ToPoint(), new Point((int) (_aboveTexture.Width / scale), (int) (_aboveTexture.Height / scale))),
                                 null,
                                 Color.White,
                                 0f,
                                 Vector2.Zero,
                                 SpriteEffects.None,
                                 0f);
            }

        }

    }
}
