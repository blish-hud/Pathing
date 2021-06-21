using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;
using TmfLib.Pathable;
using TmfLib.Prototype;
using BhModule.Community.Pathing.Utility;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail : PathingEntity {

        private static readonly Logger Logger = Logger.GetLogger<StandardTrail>();

        private const float TRAIL_WIDTH = 20 * 0.0254f;

        private readonly IPackState _packState;

        public override float DrawOrder => float.MaxValue;

        private VertexBuffer[] _sectionBuffers;

        private Vector3[][] _sectionPoints;

        public StandardTrail(IPackState packState, ITrail trail) {
            _packState = packState;

            Initialize(trail);
        }

        private void Populate(AttributeCollection collection, IPackResourceManager resourceManager) {
            Populate_Type(collection, resourceManager);
            Populate_Alpha(collection, resourceManager);
            Populate_AnimationSpeed(collection, resourceManager);
            Populate_Tint(collection, resourceManager);
            Populate_TrailScale(collection, resourceManager);
            Populate_FadeNearAndFar(collection, resourceManager);
            Populate_Texture(collection, resourceManager);
            Populate_Cull(collection, resourceManager);
            Populate_MapVisibility(collection, resourceManager);
            Populate_CanFade(collection, resourceManager);

            Populate_Behaviors(collection, resourceManager);
        }

        private VertexBuffer PostProcessTrailSection(IEnumerable<Vector3> points) {
            // Optional trail post-processing

            // TODO: Make trail resolution a configurable setting
            points = PostProcessing_SetTrailResolution(points, 20f);

            // TODO: Implement additional post processing options
            //points = PostProcessing_HermiteCurve(points);
            //points = PostProcessing_DouglasPeucker(points);

            Vector3[] pointsArr = points as Vector3[] ?? points.ToArray();
            
            float distance = 0f;

            for (int i = 0; i < pointsArr.Length - 1; i++) {
                distance += Vector3.Distance(pointsArr[i], pointsArr[i + 1]);
            }

            return distance > 0
                       ? BuildTrailSection(pointsArr, distance)
                       : null;
        }

        private VertexBuffer BuildTrailSection(IEnumerable<Vector3> points, float distance) {
            Vector3[] pointsArr = points as Vector3[] ?? points.ToArray();

            var verts = new VertexPositionColorTexture[pointsArr.Length * 2];

            float pastDistance = distance;

            var offsetDirection = new Vector3(0, 0, -1);

            var curPoint = pointsArr[0];

            var offset = Vector3.Zero;

            for (int i = 0; i < pointsArr.Length - 1; i++) {
                var nextPoint     = pointsArr[i + 1];
                var pathDirection = nextPoint - curPoint;

                offset = Vector3.Cross(pathDirection, offsetDirection);
                offset.Normalize();

                var leftPoint  = curPoint + (offset * TRAIL_WIDTH  * this.TrailScale);
                var rightPoint = curPoint + (offset * -TRAIL_WIDTH * this.TrailScale);

                verts[i * 2 + 1] = new VertexPositionColorTexture(leftPoint, Color.White,  new Vector2(0f, pastDistance / (TRAIL_WIDTH * 2) - 1));
                verts[i * 2]     = new VertexPositionColorTexture(rightPoint, Color.White, new Vector2(1f, pastDistance / (TRAIL_WIDTH * 2) - 1));

                pastDistance -= Vector3.Distance(curPoint, nextPoint);

                curPoint = nextPoint;
            }

            var fleftPoint  = curPoint + (offset * TRAIL_WIDTH);
            var frightPoint = curPoint + (offset * -TRAIL_WIDTH);

            verts[pointsArr.Length * 2 - 1] = new VertexPositionColorTexture(fleftPoint,  Color.White, new Vector2(0f, pastDistance / (TRAIL_WIDTH * 2) - 1));
            verts[pointsArr.Length * 2 - 2] = new VertexPositionColorTexture(frightPoint, Color.White, new Vector2(1f, pastDistance / (TRAIL_WIDTH * 2) - 1));

            var sectionBuffer = new VertexBuffer(GameService.Graphics.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            sectionBuffer.SetData(verts);

            return sectionBuffer;
        }

        private void Initialize(ITrail trail) {
            var _trailSections = new List<Vector3[]>(trail.TrailSections.Count());
            foreach (var trailSection in trail.TrailSections) {
                _trailSections.Add(PostProcessing_DouglasPeucker(trailSection.TrailPoints.Select(v => new Vector3(v.X, v.Y, v.Z))).ToArray());
            }

            _sectionPoints = _trailSections.ToArray();

            Populate(trail.GetAggregatedAttributes(), trail.ResourceManager);

            BuildBuffers(trail);
            //BuildSplines(trail);

            if (true) {
                this.FadeIn();
            }
        }

        private void BuildBuffers(ITrail trail) {
            var buffers = new List<VertexBuffer>();

            foreach (var section in trail.TrailSections) {
                // TODO: Fix cursed Vector3 type conversion
                var processedBuffer = PostProcessTrailSection(section.TrailPoints.Select(v => new Vector3(v.X, v.Y, v.Z)));

                if (processedBuffer != null) {
                    buffers.Add(processedBuffer);
                }
            }

            _sectionBuffers = buffers.ToArray();
        }

        private Vector2 GetScaledLocation(double x, double y, double scale, (double X, double Y) offsets) {
            (double mapX, double mapY) = _packState.MapStates.EventCoordsToMapCoords(x, y);

            return new Vector2((float) (mapX / scale - offsets.X),
                               (float) (mapY / scale - offsets.Y));
        }

        public override void RenderToMiniMap(SpriteBatch spriteBatch, Rectangle bounds, (double X, double Y) offsets, double scale, float opacity) {
            if (this.IsFiltered(EntityRenderTarget.Map, _packState) || this.Texture == null) return;

            if (!this.MapVisibility     && GameService.Gw2Mumble.UI.IsMapOpen) return;
            if (!this.MiniMapVisibility && !GameService.Gw2Mumble.UI.IsMapOpen) return;

            foreach (var trailSection in _sectionPoints) {
                for (int i = 0; i < trailSection.Length - 1; i++) {
                    var thisPoint = GetScaledLocation(trailSection[i].X,     trailSection[i].Y, scale, offsets);
                    var nextPoint = GetScaledLocation(trailSection[i + 1].X, trailSection[i + 1].Y, scale, offsets);

                    float distance = Vector2.Distance(thisPoint, nextPoint);
                    float angle    = (float)Math.Atan2(nextPoint.Y - thisPoint.Y, nextPoint.X - thisPoint.X);
                    DrawLine(spriteBatch, thisPoint, angle, distance, this.TrailSampleColor * opacity, 2f);
                }
            }

        }

        protected void DrawLine(SpriteBatch spriteBatch, Vector2 position, float angle, float distance, Color color, float thickness) {
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

        private float GetOpacity() {
            return this.Alpha
                 * _packState.UserConfiguration.PackMaxOpacityOverride.Value
                 * this.AnimatedFadeOpacity
                 * (_packState.UserConfiguration.PackFadePathablesDuringCombat.Value
                        ? (GameService.Gw2Mumble.PlayerCharacter.IsInCombat
                               ? 0.5f
                               : 1f)
                        : 1f);
        }

        public override void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera) {
            if (this.IsFiltered(EntityRenderTarget.World, _packState) || this.Texture == null || !(_sectionBuffers.Length > 0)) return;

            graphicsDevice.RasterizerState = this.CullDirection;

            _packState.SharedTrailEffect.SetEntityState(this.Texture,
                                                        Math.Min(this.AnimationSpeed, _packState.UserConfiguration.PackMaxTrailAnimationSpeed.Value),
                                                        Math.Min(this.FadeNear,       _packState.UserConfiguration.PackMaxViewDistance.Value - (this.FadeFar - this.FadeNear)),
                                                        Math.Min(this.FadeFar,        _packState.UserConfiguration.PackMaxViewDistance.Value),
                                                        GetOpacity(),
                                                        0.25f,
                                                        this.CanFade && _packState.UserConfiguration.PackFadeTrailsAroundCharacter.Value,
                                                        this.Tint);

            for (int i = 0; i < _sectionBuffers.Length; i++) {
                ref var vertexBuffer = ref _sectionBuffers[i];

                graphicsDevice.SetVertexBuffer(vertexBuffer);

                foreach (var pass in _packState.SharedTrailEffect.CurrentTechnique.Passes) {
                    pass.Apply();

                    graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, vertexBuffer.VertexCount - 2);
                }
            }
        }

    }
}
