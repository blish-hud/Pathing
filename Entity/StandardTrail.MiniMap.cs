using System;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private readonly struct SectionExtents {

            public float MinX { get; }
            public float MinY { get; }
            public float MaxX { get; }
            public float MaxY { get; }

            public SectionExtents(float minX, float minY, float maxX, float maxY) {
                this.MinX = minX;
                this.MinY = minY;
                this.MaxX = maxX;
                this.MaxY = maxY;
            }

        }

        private Vector3[][]      _extentsSource;
        private SectionExtents[] _sectionExtents;
        private SectionExtents   _trailExtents;

        public override bool IsVisibleOnMap() {
            if (IsFiltered(EntityRenderTarget.Map) || this.Texture == null || _sectionPoints == null) return false;

            bool isMapOpen = _packState.CachedMumbleStates.IsMapOpen;

            // TODO: Simplify these two map visibility filters
            var mapTrailVisibilityLevel = _packState.UserConfiguration.MapTrailVisibilityLevel.Value;
            bool allowedOnMap = this.MapVisibility && mapTrailVisibilityLevel != MapVisibilityLevel.Never;
            if (isMapOpen && !allowedOnMap && mapTrailVisibilityLevel != MapVisibilityLevel.Always) return false;

            var miniMapTrailVisibilityLevel = _packState.UserConfiguration.MiniMapTrailVisibilityLevel.Value;
            bool allowedOnMiniMap = this.MiniMapVisibility && miniMapTrailVisibilityLevel != MapVisibilityLevel.Never;
            if (!isMapOpen && !allowedOnMiniMap && miniMapTrailVisibilityLevel != MapVisibilityLevel.Always) return false;

            return true;
        }

        private void UpdateExtents(Vector3[][] sectionPoints) {
            var sectionExtents = new SectionExtents[sectionPoints.Length];

            float trailMinX = float.MaxValue, trailMinY = float.MaxValue;
            float trailMaxX = float.MinValue, trailMaxY = float.MinValue;

            for (int s = 0; s < sectionPoints.Length; s++) {
                var section = sectionPoints[s];

                float minX = float.MaxValue, minY = float.MaxValue;
                float maxX = float.MinValue, maxY = float.MinValue;

                for (int i = 0; i < section.Length; i++) {
                    minX = Math.Min(minX, section[i].X);
                    minY = Math.Min(minY, section[i].Y);
                    maxX = Math.Max(maxX, section[i].X);
                    maxY = Math.Max(maxY, section[i].Y);
                }

                sectionExtents[s] = new SectionExtents(minX, minY, maxX, maxY);

                trailMinX = Math.Min(trailMinX, minX);
                trailMinY = Math.Min(trailMinY, minY);
                trailMaxX = Math.Max(trailMaxX, maxX);
                trailMaxY = Math.Max(trailMaxY, maxY);
            }

            _sectionExtents = sectionExtents;
            _trailExtents   = new SectionExtents(trailMinX, trailMinY, trailMaxX, trailMaxY);
            _extentsSource  = sectionPoints;
        }

        // True if every point within the extents projects to the same outside side of the bounds
        private static bool IsOutside(in MapProjection projection, in SectionExtents extents, Rectangle bounds) {
            var a = projection.Project(extents.MinX, extents.MinY);
            var b = projection.Project(extents.MaxX, extents.MinY);
            var c = projection.Project(extents.MinX, extents.MaxY);
            var d = projection.Project(extents.MaxX, extents.MaxY);

            return (a.X <  bounds.Left   && b.X <  bounds.Left   && c.X <  bounds.Left   && d.X <  bounds.Left)
                || (a.X >= bounds.Right  && b.X >= bounds.Right  && c.X >= bounds.Right  && d.X >= bounds.Right)
                || (a.Y <  bounds.Top    && b.Y <  bounds.Top    && c.Y <  bounds.Top    && d.Y <  bounds.Top)
                || (a.Y >= bounds.Bottom && b.Y >= bounds.Bottom && c.Y >= bounds.Bottom && d.Y >= bounds.Bottom);
        }

        public override RectangleF? RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, double offsetX, double offsetY, double scale, float opacity) {
            var sectionPoints = _sectionPoints;

            if (this.Texture == null || sectionPoints == null) return null;

            var projection = GetMapProjection(scale, offsetX, offsetY);

            if (!ReferenceEquals(_extentsSource, sectionPoints)) {
                UpdateExtents(sectionPoints);
            }

            if (IsOutside(in projection, in _trailExtents, bounds)) return null;

            var sectionExtents = _sectionExtents;

            bool  fadeVertically = _packState.UserConfiguration.MapFadeVerticallyDistantTrailSegments.Value;
            float playerZ        = GameService.Gw2Mumble.PlayerCharacter.Position.Z;
            float trailWidth     = _packState.UserConfiguration.MapTrailWidth.Value;
            var   trailColor     = this.TrailSampleColor;

            bool lastPointInBounds = false;

            for (int s = 0; s < sectionPoints.Length; s++) {
                var trailSection = sectionPoints[s];

                if (trailSection.Length < 2) continue;

                if (IsOutside(in projection, in sectionExtents[s], bounds)) {
                    // Only the first segment can draw, and only if the previous section ended in bounds
                    if (lastPointInBounds) {
                        DrawSegment(spriteBatch, trailSection, 0,
                                    projection.Project(trailSection[0].X, trailSection[0].Y),
                                    projection.Project(trailSection[1].X, trailSection[1].Y),
                                    opacity, fadeVertically, playerZ, trailWidth, trailColor);
                    }

                    lastPointInBounds = false;
                    continue;
                }

                var thisPoint = projection.Project(trailSection[0].X, trailSection[0].Y);

                for (int i = 0; i < trailSection.Length - 1; i++) {
                    var nextPoint = projection.Project(trailSection[i + 1].X, trailSection[i + 1].Y);

                    bool inBounds = false;

                    if (lastPointInBounds | (inBounds = bounds.Contains(nextPoint))) {
                        DrawSegment(spriteBatch, trailSection, i, thisPoint, nextPoint, opacity, fadeVertically, playerZ, trailWidth, trailColor);
                    }

                    lastPointInBounds = inBounds;
                    thisPoint         = nextPoint;
                }
            }

            return null;
        }

        private void DrawSegment(SpriteBatch spriteBatch, Vector3[] trailSection, int i, Vector2 thisPoint, Vector2 nextPoint, float opacity, bool fadeVertically, float playerZ, float trailWidth, Color trailColor) {
            float drawOpacity = opacity;

            if (fadeVertically) {
                float averageVert  = (trailSection[i].Z + trailSection[i + 1].Z) / 2f;
                drawOpacity *= MathHelper.Clamp(1f - Math.Abs(averageVert - playerZ) * 0.005f, 0.15f, 1f);
            }

            float distance = Vector2.Distance(thisPoint, nextPoint);
            float angle    = (float)Math.Atan2(nextPoint.Y - thisPoint.Y, nextPoint.X - thisPoint.X);
            DrawLine(spriteBatch, thisPoint, angle, distance, trailColor * drawOpacity, trailWidth);
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
