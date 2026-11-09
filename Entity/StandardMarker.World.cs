using System;
using System.Linq;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker : ICanPick {
        
        private static          DynamicVertexBuffer _sharedVertexBuffer;
        private static readonly Vector4[]           _clipVerts = new Vector4[4];

        private static readonly Vector3[] _faceVerts = {
            new(-0.5f, -0.5f, 0), new(0.5f, -0.5f, 0), new(-0.5f, 0.5f, 0), new(0.5f, 0.5f, 0),
        };

        static StandardMarker() {
            CreateSharedVertexBuffer();
        }

        private static void CreateSharedVertexBuffer() {
            using (var gdctx = GameService.Graphics.LendGraphicsDeviceContext()) {
                _sharedVertexBuffer = new DynamicVertexBuffer(gdctx.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);
            }

            var verts = new VertexPositionTexture[_faceVerts.Length];

            for (int i = 0; i < _faceVerts.Length; i++) {
                ref var vert = ref _faceVerts[i];

                verts[i] = new VertexPositionTexture(vert, new Vector2(vert.X < 0 ? 1 : 0, vert.Y < 0 ? 1 : 0));
            }

            _sharedVertexBuffer.SetData(verts);
        }

        private float GetOpacity() {
            if (this.BehaviorFiltered) {
                return 0.5f;
            }

            float fade = 1f - MathHelper.Clamp((this.DistanceToPlayer - WorldUtil.GameToWorldCoord(this.FadeNear)) / (WorldUtil.GameToWorldCoord(this.FadeFar) - WorldUtil.GameToWorldCoord(this.FadeNear)), 0f, 1f);

            return this.Alpha
                 * fade
                 * _packState.UserConfiguration.PackMaxOpacityOverride.Value
                 * this.AnimatedFadeOpacity
                 * (_packState.UserConfiguration.PackFadePathablesDuringCombat.Value
                        ? (_packState.CachedMumbleStates.IsInCombat
                            ? 0.5f
                            : 1f)
                        : 1f);
        }

        private Matrix _modelMatrix = Matrix.Identity;

        // Set when the min/max size scale has not been applied to _modelMatrix
        private float?  _pendingSizeScale;
        private Vector3 _pendingSizePosition;

        private static Matrix GetSizeScaleMatrix(Vector3 position, float sizeScale) {
            return Matrix.CreateTranslation(-position)
                 * Matrix.CreateScale(sizeScale)
                 * Matrix.CreateTranslation(position);
        }

        // True if every vertex is outside the same clip plane
        private static bool IsOutsideClipVolume(Vector4[] clipVerts) {
            int outside = ~0;

            for (int i = 0; i < clipVerts.Length; i++) {
                ref var v = ref clipVerts[i];

                int planes = 0;

                if (v.W < 0)    planes |= 1;
                if (v.X < -v.W) planes |= 2;
                if (v.X > v.W)  planes |= 4;
                if (v.Y < -v.W) planes |= 8;
                if (v.Y > v.W)  planes |= 16;

                outside &= planes;
            }

            return outside != 0;
        }

        public bool RayIntersects(Ray ray) {
            var modelMatrix = _modelMatrix;

            if (_pendingSizeScale.HasValue) {
                modelMatrix *= GetSizeScaleMatrix(_pendingSizePosition, _pendingSizeScale.Value);
            }

            return PickingUtil.IntersectDistance(BoundingBox.CreateFromPoints(_faceVerts.Select(vert => Vector3.Transform(vert, modelMatrix))), ray) != null;
        }
        
        public override void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera) {
            if (IsFiltered(EntityRenderTarget.World) || _texture is not { HasTexture: true } || _texture.Texture.IsDisposed) return;
            if (!this.InGameVisibility) return;

            // Skip rendering stuff beyond the max view distance
            float maxRender = Math.Min(this.FadeFar, _packState.UserConfiguration.PackMaxViewDistance.Value);
            if (this.DistanceToPlayer > maxRender) return;

            float minRender = Math.Min(this.FadeNear, _packState.UserConfiguration.PackMaxViewDistance.Value - (this.FadeFar - this.FadeNear));

            graphicsDevice.RasterizerState = this.CullDirection;
            var modelMatrix = Matrix.CreateScale(this.Size * 2f * _packState.UserConfiguration.PackMarkerScale.Value, this.Size * 2f * _packState.UserConfiguration.PackMarkerScale.Value, 1f);

            var position = this.Position + new Vector3(0, 0, this.HeightOffset);

            // Only applied to the model matrix if the marker isn't culled
            float? sizeScaleToApply = null;

            if (!this.RotationXyz.HasValue) {
                modelMatrix *= Matrix.CreateBillboard(position,
                                                      new Vector3(camera.Position.X,
                                                                  camera.Position.Y,
                                                                  camera.Position.Z),
                                                      new Vector3(0, 0, 1),
                                                      camera.Forward);
                
                // Enforce min/max size
                var transformMatrix = Matrix.Multiply(Matrix.Multiply(modelMatrix, _packState.SharedMarkerEffect.View),
                                                      _packState.SharedMarkerEffect.Projection);

                for (int i = 0; i < _faceVerts.Length; i++) {
                    _clipVerts[i] = Vector4.Transform(_faceVerts[i], transformMatrix);
                }

                float minY = _clipVerts[0].Y * (1f / _clipVerts[0].W);
                float maxY = minY;

                for (int i = 1; i < _clipVerts.Length; i++) {
                    float y = _clipVerts[i].Y * (1f / _clipVerts[i].W);

                    minY = y < minY ? y : minY;
                    maxY = y > maxY ? y : maxY;
                }

                float pixelSizeY = (maxY - minY) * _packState.SharedMarkerEffect.GraphicsDevice.Viewport.Height;
                float limitY     = MathHelper.Clamp(pixelSizeY, this.MinSize * _packState.UserConfiguration.PackMarkerScale.Value * 4, this.MaxSize * 4);
                float sizeScale  = limitY / pixelSizeY;

                sizeScaleToApply = sizeScale;

                var center = (_clipVerts[0] + _clipVerts[1] + _clipVerts[2] + _clipVerts[3]) / 4f;

                for (int i = 0; i < _clipVerts.Length; i++) {
                    _clipVerts[i] = center + (_clipVerts[i] - center) * sizeScale;
                }
            } else {
                modelMatrix *= Matrix.CreateRotationX(this.RotationXyz.Value.X)
                             * Matrix.CreateRotationY(this.RotationXyz.Value.Y)
                             * Matrix.CreateRotationZ(this.RotationXyz.Value.Z)
                             * Matrix.CreateTranslation(position);

                var transformMatrix = Matrix.Multiply(Matrix.Multiply(modelMatrix, _packState.SharedMarkerEffect.View),
                                                      _packState.SharedMarkerEffect.Projection);

                for (int i = 0; i < _faceVerts.Length; i++) {
                    _clipVerts[i] = Vector4.Transform(_faceVerts[i], transformMatrix);
                }
            }

            bool outside = IsOutsideClipVolume(_clipVerts);

            if (sizeScaleToApply.HasValue && !outside) {
                modelMatrix      *= GetSizeScaleMatrix(position, sizeScaleToApply.Value);
                sizeScaleToApply =  null;
            }

            _modelMatrix         = modelMatrix;
            _pendingSizeScale    = sizeScaleToApply;
            _pendingSizePosition = position;

            if (outside) return;

            _packState.SharedMarkerEffect.SetEntityState(modelMatrix,
                                                         this.Texture,
                                                         GetOpacity(),
                                                         minRender,
                                                         maxRender,
                                                         this.CanFade && _packState.UserConfiguration.PackFadeMarkersBetweenCharacterAndCamera.Value && !this.BehaviorFiltered,
                                                         this.Tint,
                                                         this.DebugRender);

            var prevBlendState = graphicsDevice.BlendState;

            if (this.Occlude) {
                graphicsDevice.BlendState = BlendState.Opaque;
            }

            graphicsDevice.SetVertexBuffer(_sharedVertexBuffer);

            foreach (var pass in _packState.SharedMarkerEffect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

            graphicsDevice.BlendState = prevBlendState;
        }

    }
}
