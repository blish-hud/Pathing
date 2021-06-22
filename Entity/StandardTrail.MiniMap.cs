using System;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private Vector2 GetScaledLocation(double x, double y, double scale, (double X, double Y) offsets) {
            (double mapX, double mapY) = _packState.MapStates.EventCoordsToMapCoords(x, y);

            return new Vector2((float)(mapX / scale - offsets.X),
                               (float)(mapY / scale - offsets.Y));
        }

        public override void RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity) {
            if (this.IsFiltered(EntityRenderTarget.Map, _packState) || this.Texture == null) return;

            if (!this.MapVisibility && GameService.Gw2Mumble.UI.IsMapOpen) return;
            if (!this.MiniMapVisibility && !GameService.Gw2Mumble.UI.IsMapOpen) return;

            foreach (var trailSection in _sectionPoints) {
                for (int i = 0; i < trailSection.Length - 1; i++) {
                    var thisPoint = GetScaledLocation(trailSection[i].X, trailSection[i].Y, scale, offsets);
                    var nextPoint = GetScaledLocation(trailSection[i + 1].X, trailSection[i + 1].Y, scale, offsets);

                    float distance = Vector2.Distance(thisPoint, nextPoint);
                    float angle = (float)Math.Atan2(nextPoint.Y - thisPoint.Y, nextPoint.X - thisPoint.X);
                    DrawLine(spriteBatch, thisPoint, angle, distance, this.TrailSampleColor * opacity, 2f);
                }
            }

        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 position, float angle, float distance, Color color, float thickness) {
            spriteBatch.Draw(ContentService.Textures.Pixel,
                             position,
                             null,
                             color,
                             angle,
                             Vector2.Zero,
                             new Vector2(distance, thickness),
                             SpriteEffects.None,
                             0f);
        }

    }
}
