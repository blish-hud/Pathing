﻿using System;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

	public override void RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity) {
            if (IsFiltered(EntityRenderTarget.Map) || this.Texture == null) return;

            if ((!this.MapVisibility     || !_packState.UserConfiguration.MapShowTrailsOnFullscreen.Value) && GameService.Gw2Mumble.UI.IsMapOpen) return;
            if ((!this.MiniMapVisibility || !_packState.UserConfiguration.MapShowTrailsOnCompass.Value)   && !GameService.Gw2Mumble.UI.IsMapOpen) return;

            bool lastPointInBounds = false;

            foreach (var trailSection in _sectionPoints) {
                for (int i = 0; i < trailSection.Length - 1; i++) {
                    var thisPoint = GetScaledLocation(trailSection[i].X, trailSection[i].Y, scale, offsets);
                    var nextPoint = GetScaledLocation(trailSection[i + 1].X, trailSection[i + 1].Y, scale, offsets);

                    bool inBounds = false;

                    if (lastPointInBounds | (inBounds = bounds.Contains(nextPoint))) {
                        float drawOpacity = opacity;

                        if (_packState.UserConfiguration.MapFadeVerticallyDistantTrailSegments.Value) {
                            float averageVert  = (trailSection[i].Z + trailSection[i + 1].Z) / 2f;
                            drawOpacity *= MathHelper.Clamp(1f - Math.Abs(averageVert - GameService.Gw2Mumble.PlayerCharacter.Position.Z) * 0.005f, 0.15f, 1f);
                        }

                        float distance = Vector2.Distance(thisPoint, nextPoint);
                        float angle    = (float)Math.Atan2(nextPoint.Y - thisPoint.Y, nextPoint.X - thisPoint.X);
                        DrawLine(spriteBatch, thisPoint, angle, distance, this.TrailSampleColor * drawOpacity, 2f);
                    }

                    lastPointInBounds = inBounds;
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
