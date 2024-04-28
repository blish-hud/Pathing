using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail :ICanPick {

        private const float TRAIL_WIDTH = 20 * 0.0254f;

        private VertexBuffer[] _sectionBuffers;

        private VertexBuffer PostProcessTrailSection(GraphicsDevice graphicsDevice, IEnumerable<Vector3> points) {
            // Optional trail post-processing

            // TODO: Implement additional post processing options
            //points = PostProcessing_HermiteCurve(points);
            //points = PostProcessing_DouglasPeucker(points);

            // TODO: Make trail resolution a configurable setting
            points = PostProcessing_SetTrailResolution(points, 20f);

            Vector3[] pointsArr = points as Vector3[] ?? points.ToArray();

            float distance = 0f;

            for (int i = 0; i < pointsArr.Length - 1; i++) {
                distance += Vector3.Distance(pointsArr[i], pointsArr[i + 1]);
            }

            return distance > 0
                       ? BuildTrailSection(graphicsDevice, pointsArr, distance)
                       : null;
        }

        private VertexBuffer BuildTrailSection(GraphicsDevice graphicsDevice, IEnumerable<Vector3> points, float distance) {
            Vector3[] pointsArr = points as Vector3[] ?? points.ToArray();
            var verts = new VertexPositionColorTexture[pointsArr.Length * 2];
            float pastDistance = distance;
            var curPoint = pointsArr[0];
            var offset = Vector3.Zero;

            Vector3 lastOffset = Vector3.Zero;
            float flipOver = 1f;
            float normalOffset = TRAIL_WIDTH * this.TrailScale;
            Vector3 modDistance = Vector3.Zero;

            for (int i = 0; i < pointsArr.Length - 1; i++) {
                var nextPoint     = pointsArr[i + 1];
                var pathDirection = nextPoint - curPoint;

                offset = Vector3.Cross(pathDirection, this.IsWall 
                                                        ? Vector3.Cross(pathDirection, Vector3.Forward) 
                                                        : Vector3.Forward);
                offset.Normalize();

                if (lastOffset != Vector3.Zero && Vector3.Dot(offset, lastOffset) < 0) {
                    flipOver *= -1;
                }

                modDistance = offset * normalOffset * flipOver;

                verts[i * 2 + 1] = new VertexPositionColorTexture(curPoint + modDistance,  Color.White, new Vector2(0f, pastDistance / (TRAIL_WIDTH * 2) - 1));
                verts[i * 2]     = new VertexPositionColorTexture(curPoint - modDistance, Color.White, new Vector2(1f, pastDistance / (TRAIL_WIDTH * 2) - 1));

                pastDistance -= Vector3.Distance(curPoint, nextPoint);
                lastOffset = offset;
                curPoint = nextPoint;
            }

            var fleftPoint  = curPoint + modDistance;
            var frightPoint = curPoint - modDistance;

            verts[pointsArr.Length * 2 - 1] = new VertexPositionColorTexture(fleftPoint,  Color.White, new Vector2(0f, pastDistance / (TRAIL_WIDTH * 2) - 1));
            verts[pointsArr.Length * 2 - 2] = new VertexPositionColorTexture(frightPoint, Color.White, new Vector2(1f, pastDistance / (TRAIL_WIDTH * 2) - 1));

            var sectionBuffer = new VertexBuffer(graphicsDevice, VertexPositionColorTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            sectionBuffer.SetData(verts);

            return sectionBuffer;
        }

        private void BuildBuffers(ITrail trail) {
            var buffers = new List<VertexBuffer>();

            using (var gdctx = GameService.Graphics.LendGraphicsDeviceContext()) {
                foreach (var section in trail.TrailSections) {
                    // TODO: Fix cursed Vector3 type conversion
                    var processedBuffer = PostProcessTrailSection(gdctx.GraphicsDevice, section.TrailPoints.Select(v => new Vector3(v.X, v.Y, v.Z)));

                    if (processedBuffer != null) {
                        buffers.Add(processedBuffer);
                    }
                }
            }

            _sectionBuffers = buffers.ToArray();
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

        public bool RayIntersects(Ray ray) {
            for (int s = 0; s < _sectionPoints.Length; s++) {
                ref Vector3[] section = ref _sectionPoints[s];

                for (int i = 0; i < _sectionPoints[s].Length; i++) {
                    ref var point = ref section[i];

                    if (PickingUtil.IntersectDistance(BoundingSphere.CreateFromPoints(new[] { point, point + Vector3.One }), ray) != null) {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera) {
            if (IsFiltered(EntityRenderTarget.World) || _texture is not { HasTexture: true } || _texture.Texture.IsDisposed || !(_sectionBuffers.Length > 0)) return;

            if (!this.InGameVisibility) return;

            graphicsDevice.RasterizerState = this.CullDirection;

            _packState.SharedTrailEffect.SetEntityState(this.Texture,
                                                        Math.Min(this.AnimationSpeed, _packState.UserConfiguration.PackMaxTrailAnimationSpeed.Value),
                                                        Math.Min(this.FadeNear,       _packState.UserConfiguration.PackMaxViewDistance.Value - (this.FadeFar - this.FadeNear)),
                                                        Math.Min(this.FadeFar,        _packState.UserConfiguration.PackMaxViewDistance.Value),
                                                        GetOpacity(),
                                                        _packState.UserResourceStates.Advanced.CharacterTrailFadeMultiplier,
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
