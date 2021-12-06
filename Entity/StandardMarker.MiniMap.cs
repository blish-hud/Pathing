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

            bool isMapOpen = GameService.Gw2Mumble.UI.IsMapOpen;
            
            // TODO: Simplify these two map visibility filters
            var  mapMarkerVisibilityLevel = _packState.UserConfiguration.MapMarkerVisibilityLevel.Value;
            bool allowedOnMap             = this.MapVisibility && mapMarkerVisibilityLevel != MapVisibilityLevel.Never;
            if (isMapOpen && !allowedOnMap && mapMarkerVisibilityLevel != MapVisibilityLevel.Always) return null;

            var  miniMapMarkerVisibilityLevel = _packState.UserConfiguration.MiniMapMarkerVisibilityLevel.Value;
            bool allowedOnMiniMap             = this.MiniMapVisibility && miniMapMarkerVisibilityLevel != MapVisibilityLevel.Never;
            if (!isMapOpen && !allowedOnMiniMap && miniMapMarkerVisibilityLevel != MapVisibilityLevel.Always) return null;

            if (!this.ScaleOnMapWithZoom) {
                // Zoom scaling is disabled, so we override back to 1.
                scale = 1f;
            }

            var location = GetScaledLocation(this.Position.X, this.Position.Y, scale, offsets);

            if (!bounds.Contains(location)) return null;

            float drawScale = (float)(1f / scale);

            var drawRect = new RectangleF(location - new Vector2(this.MapDisplaySize / 2f * drawScale, this.MapDisplaySize / 2f * drawScale),
                                          new Vector2(this.MapDisplaySize * drawScale, this.MapDisplaySize * drawScale));

            spriteBatch.Draw(this.Texture, drawRect, this.Tint * opacity);

            // Draw above or below indicator, if applicable.
            // We skip if zoomed out too far or if feature is disabled by the user.
            if (_packState.UserConfiguration.MapShowAboveBelowIndicators.Value && scale < ABOVEBELOWINDICATOR_THRESHOLD) {
                float diff = this.Position.Z - GameService.Gw2Mumble.PlayerCharacter.Position.Z;

                if (Math.Abs(diff) > VERTICALOFFSET_THRESHOLD) {
                    var indicatorPosition = new RectangleF(drawRect.Right - _aboveTexture.Width * drawScale,
                                                           drawRect.Top,
                                                           _aboveTexture.Width  * drawScale,
                                                           _aboveTexture.Height * drawScale);

                    spriteBatch.Draw(diff > 0 ? _aboveTexture : _belowTexture, indicatorPosition, Color.White * opacity);
                }
            }

            return drawRect;
        }

    }
}
