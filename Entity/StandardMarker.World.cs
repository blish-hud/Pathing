using System;
using System.Linq;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker : ICanPick {
        
        private static DynamicVertexBuffer _sharedVertexBuffer;

        private static readonly Vector3[] _faceVerts = {
            new(-0.5f, -0.5f, 0), new(0.5f, -0.5f, 0), new(-0.5f, 0.5f, 0), new(0.5f, 0.5f, 0),
        };

        static StandardMarker() {
            CreateSharedVertexBuffer();
        }

        private static void CreateSharedVertexBuffer() {
            _sharedVertexBuffer = new DynamicVertexBuffer(GameService.Graphics.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);

            var verts = new VertexPositionTexture[_faceVerts.Length];

            for (int i = 0; i < _faceVerts.Length; i++) {
                ref var vert = ref _faceVerts[i];

                verts[i] = new VertexPositionTexture(vert, new Vector2(vert.X < 0 ? 1 : 0, vert.Y < 0 ? 1 : 0));
            }

            _sharedVertexBuffer.SetData(verts);
        }

        private float GetOpacity() {
            float fade = 1f - MathHelper.Clamp((this.DistanceToPlayer - WorldUtil.GameToWorldCoord(this.FadeNear)) / (WorldUtil.GameToWorldCoord(this.FadeFar) - WorldUtil.GameToWorldCoord(this.FadeNear)), 0f, 1f);

            return this.Alpha
                 * fade
                 * _packState.UserConfiguration.PackMaxOpacityOverride.Value
                 * this.AnimatedFadeOpacity
                 * (_packState.UserConfiguration.PackFadePathablesDuringCombat.Value
                        ? (GameService.Gw2Mumble.PlayerCharacter.IsInCombat
                            ? 0.5f
                            : 1f)
                        : 1f);
        }

        private Matrix _modelMatrix = Matrix.Identity;

        public bool RayIntersects(Ray ray) {
            return PickingUtil.IntersectDistance(BoundingBox.CreateFromPoints(_faceVerts.Select(vert => Vector3.Transform(vert, _modelMatrix))), ray) != null;
        }

        private Vector3 ConvertToScreen(Vector3 position, Matrix view, Matrix projection) {
            int screenWidth  = GameService.Graphics.SpriteScreen.Width;
            int screenHeight = GameService.Graphics.SpriteScreen.Height;

            position = Vector3.Transform(position, view);
            position = Vector3.Transform(position, projection);

            float x = position.X / position.Z;
            float y = position.Y / -position.Z;

            x = (x + 1) * screenWidth  / 2;
            y = (y + 1) * screenHeight / 2;

            return new Vector3(x, y, position.Z);
        }

        public override void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera) {
            if (IsFiltered(EntityRenderTarget.World) || _texture == null) return;

            if (!this.InGameVisibility) return;

            // Skip rendering stuff beyond the max view distance
            float maxRender = Math.Min(this.FadeFar, _packState.UserConfiguration.PackMaxViewDistance.Value);
            if (this.DistanceToPlayer > maxRender) return;

            float minRender = Math.Min(this.FadeNear, _packState.UserConfiguration.PackMaxViewDistance.Value - (this.FadeFar - this.FadeNear));

            graphicsDevice.RasterizerState = this.CullDirection;
            var modelMatrix = Matrix.CreateScale(this.Size.X / 2f, this.Size.Y / 2f, 1f) * Matrix.CreateScale(this.Scale);

            var position = this.Position + new Vector3(0, 0, this.HeightOffset);

            if (!this.RotationXyz.HasValue) {
                modelMatrix *= Matrix.CreateBillboard(position,
                                                      new Vector3(camera.Position.X,
                                                                  camera.Position.Y,
                                                                  camera.Position.Z),
                                                      new Vector3(0, 0, 1),
                                                      camera.Forward);
            } else {
                modelMatrix *= Matrix.CreateRotationX(this.RotationXyz.Value.X)
                             * Matrix.CreateRotationY(this.RotationXyz.Value.Y)
                             * Matrix.CreateRotationZ(this.RotationXyz.Value.Z)
                             * Matrix.CreateTranslation(position);
            }

            //

            // Find size of the object in screen space
            var screenVerts = new Vector4[_faceVerts.Length];
            var transformMatrix = Matrix.Multiply(Matrix.Multiply(modelMatrix, _packState.SharedMarkerEffect.View),
                                                  _packState.SharedMarkerEffect.Projection);

            for (int i = 0; i < _faceVerts.Length; i++) {
                screenVerts[i] =  Vector4.Transform(_faceVerts[i], transformMatrix);
                screenVerts[i] /= screenVerts[i].W;
            }

            var bounds = BoundingRectangle.CreateFrom(screenVerts.Select(s => new Point2(s.X, s.Y)).ToArray());
            var pixelSize = new Vector2(bounds.HalfExtents.X * 2 * _packState.SharedMarkerEffect.GraphicsDevice.Viewport.Width,
                                        bounds.HalfExtents.X * 2 * _packState.SharedMarkerEffect.GraphicsDevice.Viewport.Height);

            float limitWidth = MathHelper.Clamp(pixelSize.X, this.MinSize * 2, this.MaxSize * 2);
            float limitHeight = MathHelper.Clamp(pixelSize.Y, this.MinSize * 2, this.MaxSize * 2);

            modelMatrix *= Matrix.CreateTranslation(-position)
                         * Matrix.CreateScale(limitHeight / pixelSize.Y)
                         * Matrix.CreateTranslation(position);

            //

            _packState.SharedMarkerEffect.SetEntityState(modelMatrix,
                                                         this.Texture,
                                                         GetOpacity(),
                                                         minRender,
                                                         maxRender,
                                                         this.CanFade && _packState.UserConfiguration.PackFadeMarkersBetweenCharacterAndCamera.Value,
                                                         this.Tint,
                                                         this.DebugRender);

            _modelMatrix = modelMatrix;

            graphicsDevice.SetVertexBuffer(_sharedVertexBuffer);

            foreach (var pass in _packState.SharedMarkerEffect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

    }
}
