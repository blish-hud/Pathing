using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Editor.Entity {
    public class TranslateAxisHandle : IEntity, IAxisHandle, ICanPick {

        public Vector3 Origin { get; set; } = Vector3.Zero;

        private readonly Matrix _axisTransform;

        public float DrawOrder => float.MinValue;

        private readonly VertexBuffer _buffer;
        private readonly BasicEffect  _effect;

        private float _mouseIndex;
        private float _mouseOffset;
        private bool  _handleActive = false;

        public TranslateAxisHandle(Color axisColor, Matrix axisTransform) {
            _axisTransform = axisTransform;

            _effect = new BasicEffect(GameService.Graphics.GraphicsDevice) {
                VertexColorEnabled = true,
                Alpha              = 0.4f
            };

            var verts = new VertexPositionColor[_faceIndexes.Length * 3];

            for (int i = 0; i < _faceIndexes.Length; i++) {
                ref var faceDef = ref _faceIndexes[i];

                for (int f = 0; f < 3; f++) {
                    verts[i * 3 + f] = new VertexPositionColor(_arrowVerts[faceDef[f] - 1], axisColor);
                }
            }

            _buffer = new VertexBuffer(GameService.Graphics.GraphicsDevice, VertexPositionColor.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            _buffer.SetData(verts);
        }

        public bool RayIntersects(Ray ray) {
            return PickingUtil.IntersectDistance(BoundingBox.CreateFromPoints(_arrowVerts.Select(vert => Vector3.Transform(vert, _modelMatrix))), ray) != null;
        }

        private Matrix _modelMatrix = Matrix.Identity;

        public void HandleActivated(Ray ray) {
            _handleActive = true;

            _mouseIndex = ray.Position.Z;
        }

        public void Update(GameTime gameTime) {
            if (_handleActive) {
                _mouseOffset = PickingUtil.CalculateRay(GameService.Input.Mouse.Position, GameService.Gw2Mumble.PlayerCamera.View, GameService.Gw2Mumble.PlayerCamera.Projection).Position.Z;
            } else {
                _mouseIndex  = 0;
                _mouseOffset = 0;
            }
        }

        public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera) {
            graphicsDevice.SetVertexBuffer(_buffer);

            _effect.World      = _modelMatrix = Matrix.CreateScale(0.08f) * _axisTransform * Matrix.CreateTranslation(0, 0, -(_mouseOffset - _mouseIndex)) * Matrix.CreateTranslation(this.Origin);
            _effect.View       = GameService.Gw2Mumble.PlayerCamera.View;
            _effect.Projection = GameService.Gw2Mumble.PlayerCamera.Projection;

            foreach (var pass in _effect.CurrentTechnique.Passes) {
                pass.Apply();

                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _buffer.VertexCount);
            }
        }

        private static readonly int[][] _faceIndexes = {
            new []{ 2, 48, 1 },
            new []{ 1, 48, 25 },
            new []{ 1, 25, 24 },
            new []{ 24, 25, 26 },
            new []{ 24, 26, 27 },
            new []{ 48, 2, 47 },
            new []{ 47, 2, 3 },
            new []{ 47, 3, 46 },
            new []{ 46, 3, 4 },
            new []{ 46, 4, 45 },
            new []{ 45, 4, 5 },
            new []{ 45, 5, 44 },
            new []{ 44, 5, 6 },
            new []{ 44, 6, 43 },
            new []{ 43, 6, 7 },
            new []{ 43, 7, 8 },
            new []{ 43, 8, 42 },
            new []{ 42, 8, 9 },
            new []{ 42, 9, 41 },
            new []{ 41, 9, 10 },
            new []{ 41, 10, 40 },
            new []{ 40, 10, 11 },
            new []{ 40, 11, 39 },
            new []{ 39, 11, 12 },
            new []{ 39, 12, 38 },
            new []{ 38, 12, 13 },
            new []{ 38, 13, 37 },
            new []{ 37, 13, 36 },
            new []{ 36, 13, 14 },
            new []{ 36, 14, 35 },
            new []{ 35, 14, 15 },
            new []{ 35, 15, 34 },
            new []{ 34, 15, 16 },
            new []{ 34, 16, 33 },
            new []{ 33, 16, 17 },
            new []{ 33, 17, 32 },
            new []{ 32, 17, 18 },
            new []{ 32, 18, 31 },
            new []{ 31, 18, 19 },
            new []{ 31, 19, 20 },
            new []{ 31, 20, 30 },
            new []{ 30, 20, 21 },
            new []{ 30, 21, 29 },
            new []{ 29, 21, 22 },
            new []{ 29, 22, 28 },
            new []{ 28, 22, 23 },
            new []{ 28, 23, 27 },
            new []{ 27, 23, 24 },
            new []{ 24, 49, 1 },
            new []{ 22, 49, 23 },
            new []{ 20, 49, 21 },
            new []{ 18, 49, 19 },
            new []{ 16, 49, 17 },
            new []{ 14, 49, 15 },
            new []{ 12, 49, 13 },
            new []{ 10, 49, 11 },
            new []{ 8, 49, 9 },
            new []{ 6, 49, 7 },
            new []{ 4, 49, 5 },
            new []{ 2, 49, 3 },
            new []{ 13, 49, 14 },
            new []{ 21, 49, 22 },
            new []{ 5, 49, 6 },
            new []{ 15, 49, 16 },
            new []{ 17, 49, 18 },
            new []{ 19, 49, 20 },
            new []{ 23, 49, 24 },
            new []{ 1, 49, 2 },
            new []{ 3, 49, 4 },
            new []{ 7, 49, 8 },
            new []{ 9, 49, 10 },
            new []{ 11, 49, 12 },
            new []{ 48, 73, 25 },
            new []{ 25, 73, 50 },
            new []{ 25, 50, 26 },
            new []{ 26, 50, 51 },
            new []{ 26, 51, 27 },
            new []{ 27, 51, 52 },
            new []{ 27, 52, 28 },
            new []{ 28, 52, 53 },
            new []{ 28, 53, 29 },
            new []{ 29, 53, 54 },
            new []{ 29, 54, 30 },
            new []{ 30, 54, 55 },
            new []{ 30, 55, 31 },
            new []{ 31, 55, 56 },
            new []{ 31, 56, 32 },
            new []{ 32, 56, 57 },
            new []{ 32, 57, 33 },
            new []{ 33, 57, 58 },
            new []{ 33, 58, 34 },
            new []{ 34, 58, 59 },
            new []{ 34, 59, 35 },
            new []{ 35, 59, 60 },
            new []{ 35, 60, 36 },
            new []{ 36, 60, 61 },
            new []{ 36, 61, 37 },
            new []{ 37, 61, 62 },
            new []{ 37, 62, 38 },
            new []{ 38, 62, 63 },
            new []{ 38, 63, 39 },
            new []{ 39, 63, 64 },
            new []{ 39, 64, 40 },
            new []{ 40, 64, 65 },
            new []{ 40, 65, 41 },
            new []{ 41, 65, 66 },
            new []{ 41, 66, 42 },
            new []{ 42, 66, 67 },
            new []{ 42, 67, 43 },
            new []{ 43, 67, 68 },
            new []{ 43, 68, 44 },
            new []{ 44, 68, 69 },
            new []{ 44, 69, 45 },
            new []{ 45, 69, 70 },
            new []{ 45, 70, 46 },
            new []{ 46, 70, 71 },
            new []{ 46, 71, 47 },
            new []{ 47, 71, 72 },
            new []{ 47, 72, 48 },
            new []{ 48, 72, 73 },
            new []{ 73, 63, 50 },
            new []{ 50, 63, 62 },
            new []{ 50, 62, 51 },
            new []{ 51, 62, 61 },
            new []{ 51, 61, 52 },
            new []{ 52, 61, 60 },
            new []{ 52, 60, 53 },
            new []{ 53, 60, 59 },
            new []{ 53, 59, 54 },
            new []{ 54, 59, 58 },
            new []{ 54, 58, 55 },
            new []{ 55, 58, 57 },
            new []{ 55, 57, 56 },
            new []{ 63, 73, 64 },
            new []{ 64, 73, 72 },
            new []{ 64, 72, 65 },
            new []{ 65, 72, 71 },
            new []{ 65, 71, 66 },
            new []{ 66, 71, 70 },
            new []{ 66, 70, 67 },
            new []{ 67, 70, 69 },
            new []{ 67, 69, 68 },
        };

        private static readonly Vector3[] _arrowVerts = {
            new(-1.000000f, 0.000000f, 12.000000f), new(-0.965926f, 0.258819f, 12.000000f), new(-0.866025f, 0.500000f, 12.000000f), new(-0.707107f, 0.707107f, 12.000000f), new(-0.500000f, 0.866025f, 12.000000f),
            new(-0.258819f, 0.965926f, 12.000000f), new(0.000000f, 1.000000f, 12.000000f), new(0.258819f, 0.965926f, 12.000000f), new(0.500000f, 0.866025f, 12.000000f), new(0.707107f, 0.707107f, 12.000000f),
            new(0.866025f, 0.500000f, 12.000000f), new(0.965926f, 0.258819f, 12.000000f), new(1.000000f, -0.000000f, 12.000000f), new(0.965926f, -0.258819f, 12.000000f), new(0.866025f, -0.500000f, 12.000000f),
            new(0.707107f, -0.707107f, 12.000000f), new(0.500000f, -0.866025f, 12.000000f), new(0.258819f, -0.965926f, 12.000000f), new(-0.000000f, -1.000000f, 12.000000f), new(-0.258819f, -0.965926f, 12.000000f),
            new(-0.500000f, -0.866025f, 12.000000f), new(-0.707107f, -0.707107f, 12.000000f), new(-0.866025f, -0.500000f, 12.000000f), new(-0.965926f, -0.258819f, 12.000000f), new(-0.500000f, -0.000000f, 12.000000f),
            new(-0.482963f, -0.129410f, 12.000000f), new(-0.433013f, -0.250000f, 12.000000f), new(-0.353553f, -0.353553f, 12.000000f), new(-0.250000f, -0.433013f, 12.000000f), new(-0.129410f, -0.482963f, 12.000000f),
            new(0.000000f, -0.500000f, 12.000000f), new(0.129410f, -0.482963f, 12.000000f), new(0.250000f, -0.433013f, 12.000000f), new(0.353553f, -0.353553f, 12.000000f), new(0.433013f, -0.250000f, 12.000000f),
            new(0.482963f, -0.129410f, 12.000000f), new(0.500000f, 0.000000f, 12.000000f), new(0.482963f, 0.129410f, 12.000000f), new(0.433013f, 0.250000f, 12.000000f), new(0.353553f, 0.353553f, 12.000000f),
            new(0.250000f, 0.433013f, 12.000000f), new(0.129410f, 0.482963f, 12.000000f), new(0.000000f, 0.500000f, 12.000000f), new(-0.129410f, 0.482963f, 12.000000f), new(-0.250000f, 0.433013f, 12.000000f),
            new(-0.353553f, 0.353553f, 12.000000f), new(-0.433013f, 0.250000f, 12.000000f), new(-0.482963f, 0.129410f, 12.000000f), new(0.000000f, -0.000000f, 16.000000f), new(-0.500000f, -0.000000f, 0.000000f),
            new(-0.482963f, -0.129410f, 0.000000f), new(-0.433013f, -0.250000f, 0.000000f), new(-0.353553f, -0.353553f, 0.000000f), new(-0.250000f, -0.433013f, 0.000000f), new(-0.129410f, -0.482963f, 0.000000f),
            new(0.000000f, -0.500000f, 0.000000f), new(0.129410f, -0.482963f, 0.000000f), new(0.250000f, -0.433013f, 0.000000f), new(0.353553f, -0.353553f, 0.000000f), new(0.433013f, -0.250000f, 0.000000f),
            new(0.482963f, -0.129410f, 0.000000f), new(0.500000f, 0.000000f, 0.000000f), new(0.482963f, 0.129410f, 0.000000f), new(0.433013f, 0.250000f, 0.000000f), new(0.353553f, 0.353553f, 0.000000f), new(0.250000f, 0.433013f, 0.000000f),
            new(0.129410f, 0.482963f, 0.000000f), new(0.000000f, 0.500000f, 0.000000f), new(-0.129410f, 0.482963f, 0.000000f), new(-0.250000f, 0.433013f, 0.000000f), new(-0.353553f, 0.353553f, 0.000000f),
            new(-0.433013f, 0.250000f, 0.000000f), new(-0.482963f, 0.129410f, 0.000000f)
        };

    }
}
