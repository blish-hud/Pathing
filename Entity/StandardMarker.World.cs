using System;
using Blish_HUD;
using Blish_HUD.Entities;
using Blish_HUD.Pathing.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private void InitializeWorld() {
            _verts        = new VertexPositionTexture[4];
            _vertexBuffer = new DynamicVertexBuffer(GameService.Graphics.GraphicsDevice, typeof(VertexPositionTexture), 4, BufferUsage.WriteOnly);

            RecalculateSize(this.Size, this.Scale);
        }

        private void RecalculateSize(Vector2 size, float scale) {
            _verts[0] = new VertexPositionTexture(new Vector3(0,              0,              0),                 new Vector2(1, 1));
            _verts[1] = new VertexPositionTexture(new Vector3(size.X * scale, 0,              0),                 new Vector2(0, 1));
            _verts[2] = new VertexPositionTexture(new Vector3(0,              size.Y * scale, 0),                 new Vector2(1, 0));
            _verts[3] = new VertexPositionTexture(new Vector3(size.X                 * scale, size.Y * scale, 0), new Vector2(0, 0));

            _vertexBuffer.SetData(_verts);
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
            if (this.IsFiltered(EntityRenderTarget.World, _packState) || _texture == null) return;

            // Skip rendering stuff beyond the max view distance
            float maxRender = Math.Min(this.FadeFar, _packState.UserConfiguration.PackMaxViewDistance.Value);
            if (this.DistanceToPlayer > maxRender) return;

            graphicsDevice.RasterizerState = this.CullDirection;

            var modelMatrix = Matrix.CreateTranslation(this.Size.X * this.Scale / -2, this.Size.Y * this.Scale / -2, 0);

            if (this.RotationXyz == Vector3.Zero) {
                modelMatrix *= Matrix.CreateBillboard(this.Position + new Vector3(0, 0, this.HeightOffset),
                                                      new Vector3(camera.Position.X,
                                                                  camera.Position.Y,
                                                                  this.VerticalConstraint == BillboardVerticalConstraint.CameraPosition
                                                                      ? camera.Position.Z
                                                                      : GameService.Gw2Mumble.PlayerCharacter.Position.Z),
                                                      new Vector3(0, 0, 1),
                                                      camera.Forward);
            } else {
                modelMatrix *= Matrix.CreateRotationX(this.RotationXyz.X)
                             * Matrix.CreateRotationY(this.RotationXyz.Y)
                             * Matrix.CreateRotationZ(this.RotationXyz.Z)
                             * Matrix.CreateTranslation(this.Position + new Vector3(0, 0, this.HeightOffset));
            }

            _packState.SharedMarkerEffect.SetEntityState(modelMatrix,
                                                         this.Texture,
                                                         GetOpacity(),
                                                         Math.Min(this.FadeNear, _packState.UserConfiguration.PackMaxViewDistance.Value - (this.FadeFar - this.FadeNear)),
                                                         maxRender,
                                                         this.CanFade && _packState.UserConfiguration.PackFadeMarkersBetweenCharacterAndCamera.Value,
                                                         Tint);

            graphicsDevice.SetVertexBuffer(_vertexBuffer);

            foreach (var pass in _packState.SharedMarkerEffect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }
        }

    }
}
