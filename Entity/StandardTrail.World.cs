﻿using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const float TRAIL_WIDTH = 20 * 0.0254f;

        private VertexBuffer[] _sectionBuffers;

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

                verts[i * 2 + 1] = new VertexPositionColorTexture(leftPoint,  Color.White, new Vector2(0f, pastDistance / (TRAIL_WIDTH * 2) - 1));
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