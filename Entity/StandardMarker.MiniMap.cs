using System;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const double ABOVEBELOWINDICATOR_THRESHOLD = 2.0d;
        private const float  VERTICALOFFSET_THRESHOLD      = 30f;

        private static readonly Texture2D _aboveTexture = PathingModule.Instance.ContentsManager.GetTexture(@"png\1130638.png");
        private static readonly Texture2D _belowTexture = PathingModule.Instance.ContentsManager.GetTexture(@"png\1130639.png");
        
        public override RectangleF? RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity) {
            if (IsFiltered(EntityRenderTarget.Map) || this.Texture == null) return null;

            if ((!this.MapVisibility     || !_packState.UserConfiguration.MapShowMarkersOnFullscreen.Value) && GameService.Gw2Mumble.UI.IsMapOpen) return null;
            if ((!this.MiniMapVisibility || !_packState.UserConfiguration.MapShowMarkersOnCompass.Value)    && !GameService.Gw2Mumble.UI.IsMapOpen) return null;

            var location = GetScaledLocation(this.Position.X, this.Position.Y, scale, offsets);

            if (!bounds.Contains(location)) return null;

            float drawScale = (float)(0.3f / scale);

            var drawRect = new RectangleF(location - new Vector2(this.Texture.Width / 2f * drawScale, this.Texture.Height / 2f * drawScale),
                                         new Vector2(this.Texture.Width * drawScale, this.Texture.Height * drawScale));

            spriteBatch.Draw(this.Texture, drawRect, this.Tint);

            // Draw above or below indicator, if applicable.
            // We skip if zoomed out too far or if feature is disabled by the user.
            if (_packState.UserConfiguration.MapShowAboveBelowIndicators.Value && scale < ABOVEBELOWINDICATOR_THRESHOLD) {
                float diff = this.Position.Z - GameService.Gw2Mumble.PlayerCharacter.Position.Z;

                if (Math.Abs(diff) > VERTICALOFFSET_THRESHOLD) {
                    spriteBatch.Draw(diff > 0 ? _aboveTexture : _belowTexture,
                                     new Rectangle(location.ToPoint(), new Point(RoundToInt(_aboveTexture.Width / scale), RoundToInt(_aboveTexture.Height / scale))),
                                     null,
                                     Color.White,
                                     0f,
                                     Vector2.Zero,
                                     SpriteEffects.None,
                                     0f);
                }
            }

            return drawRect;
        }

        private static int RoundToInt(double value) {
            return value >= 0
                       ? (int)(value + 0.5d)
                       : (int)(value - 0.5d);
        }

    }
}
