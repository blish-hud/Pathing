using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Entity.Effects;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail :ICanPick {

        private const float TRAIL_WIDTH = 20 * 0.0254f;

        // Must be even to keep the triangle strip winding consistent between chunks
        private const int CHUNK_PRIMITIVES = 256;

        private readonly struct SectionBuffer {

            public VertexBuffer     Buffer         { get; }
            public int              PrimitiveCount { get; }
            public BoundingSphere   Bounds         { get; }
            public BoundingSphere[] ChunkBounds    { get; }

            public SectionBuffer(VertexBuffer buffer, int primitiveCount, BoundingSphere bounds, BoundingSphere[] chunkBounds) {
                this.Buffer         = buffer;
                this.PrimitiveCount = primitiveCount;
                this.Bounds         = bounds;
                this.ChunkBounds    = chunkBounds;
            }

        }

        private SectionBuffer[] _sectionBuffers;

        private SectionBuffer? PostProcessTrailSection(GraphicsDevice graphicsDevice, IEnumerable<Vector3> points) {
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
                       : (SectionBuffer?)null;
        }

        private SectionBuffer BuildTrailSection(GraphicsDevice graphicsDevice, IEnumerable<Vector3> points, float distance) {
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

            int primitiveCount = verts.Length - 2;
            var chunkBounds    = new BoundingSphere[(primitiveCount + CHUNK_PRIMITIVES - 1) / CHUNK_PRIMITIVES];

            for (int c = 0; c < chunkBounds.Length; c++) {
                int start = c * CHUNK_PRIMITIVES;
                int end   = Math.Min(start + CHUNK_PRIMITIVES, primitiveCount) + 2;

                chunkBounds[c] = GetBounds(verts, start, end);
            }

            return new SectionBuffer(sectionBuffer, primitiveCount, GetBounds(verts, 0, verts.Length), chunkBounds);
        }

        private static BoundingSphere GetBounds(VertexPositionColorTexture[] verts, int start, int end) {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            for (int i = start; i < end; i++) {
                min = Vector3.Min(min, verts[i].Position);
                max = Vector3.Max(max, verts[i].Position);
            }

            var   center = (min + max) / 2f;
            float radius = 0f;

            for (int i = start; i < end; i++) {
                radius = Math.Max(radius, Vector3.Distance(center, verts[i].Position));
            }

            return new BoundingSphere(center, radius);
        }

        private void BuildBuffers(ITrail trail) {
            var buffers = new List<SectionBuffer>();

            using (var gdctx = GameService.Graphics.LendGraphicsDeviceContext()) {
                foreach (var section in trail.TrailSections) {
                    // TODO: Fix cursed Vector3 type conversion
                    var processedBuffer = PostProcessTrailSection(gdctx.GraphicsDevice, section.TrailPoints.Select(v => new Vector3(v.X, v.Y, v.Z)));

                    if (processedBuffer.HasValue) {
                        buffers.Add(processedBuffer.Value);
                    }
                }
            }

            _sectionBuffers = buffers.ToArray();
        }

        internal void BuildBuffers(Vector3[] trail) {
            var buffers = new List<SectionBuffer>();

            using (var gdctx = GameService.Graphics.LendGraphicsDeviceContext()) {
                // TODO: Fix cursed Vector3 type conversion
                var processedBuffer = PostProcessTrailSection(gdctx.GraphicsDevice, trail.Select(v => new Vector3(v.X, v.Y, v.Z)));

                if (processedBuffer.HasValue) {
                    buffers.Add(processedBuffer.Value);
                }
            }

            _sectionBuffers = buffers.ToArray();
        }

        private float GetOpacity() {
            return this.Alpha
                 * _packState.UserConfiguration.PackMaxOpacityOverride.Value
                 * this.AnimatedFadeOpacity
                 * (_packState.UserConfiguration.PackFadePathablesDuringCombat.Value
                        ? (_packState.CachedMumbleStates.IsInCombat
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

            float fadeFar = Math.Min(this.FadeFar, _packState.UserConfiguration.PackMaxViewDistance.Value);

            var trailEffect = _packState.SharedTrailEffect;

            trailEffect.SetEntityState(this.Texture,
                                       Math.Min(this.AnimationSpeed, _packState.UserConfiguration.PackMaxTrailAnimationSpeed.Value),
                                       Math.Min(this.FadeNear,       _packState.UserConfiguration.PackMaxViewDistance.Value - (this.FadeFar - this.FadeNear)),
                                       fadeFar,
                                       GetOpacity(),
                                       _packState.UserResourceStates.Advanced.CharacterTrailFadeMultiplier,
                                       this.CanFade && _packState.UserConfiguration.PackFadeTrailsAroundCharacter.Value,
                                       this.Tint);

            var sectionBuffers = _sectionBuffers;

            for (int i = 0; i < sectionBuffers.Length; i++) {
                ref var section = ref sectionBuffers[i];

                if (!IsVisible(trailEffect, section.Bounds, fadeFar)) continue;

                var chunkBounds = section.ChunkBounds;

                // Only the ends are trimmed so that each section remains a single draw
                int first = 0;
                int last  = chunkBounds.Length - 1;

                if (last > 0) {
                    while (first <= last && !IsVisible(trailEffect, in chunkBounds[first], fadeFar)) first++;
                    while (last > first && !IsVisible(trailEffect, in chunkBounds[last], fadeFar)) last--;

                    if (first > last) continue;
                }

                int startPrimitive = first * CHUNK_PRIMITIVES;
                int endPrimitive   = Math.Min((last + 1) * CHUNK_PRIMITIVES, section.PrimitiveCount);

                DrawRange(graphicsDevice, trailEffect, section.Buffer, startPrimitive, endPrimitive - startPrimitive);
            }
        }

        // False only if the shader would clip everything within the bounds
        private static bool IsVisible(TrailEffect trailEffect, in BoundingSphere bounds, float fadeFar) {
            float nearestDistance = (Vector3.Distance(trailEffect.PlayerPosition, bounds.Center) - bounds.Radius) / 0.0254f;

            return !(nearestDistance > fadeFar) && trailEffect.Frustum.Intersects(bounds);
        }

        private static void DrawRange(GraphicsDevice graphicsDevice, TrailEffect trailEffect, VertexBuffer vertexBuffer, int startVertex, int primitiveCount) {
            graphicsDevice.SetVertexBuffer(vertexBuffer);

            foreach (var pass in trailEffect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, startVertex, primitiveCount);
            }
        }

    }
}
