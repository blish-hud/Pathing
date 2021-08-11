using System;
using BhModule.Community.Pathing.Entity;
using Blish_HUD;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Editor.Entity {
    public class TranslateTool : IEntity {
        
        public float DrawOrder => 0;

        public Vector3 ToolOrigin { get; set; }
        
        private IPathingEntity _target;

        private TranslateAxisHandle _xAxis;
        private TranslateAxisHandle _yAxis;
        private TranslateAxisHandle _zAxis;

        public TranslateTool(StandardMarker target) {
            _target = target;

            BuildAxis(this.ToolOrigin = target.Position);
        }

        public TranslateTool(Vector3 toolOrigin) {
            BuildAxis(this.ToolOrigin = toolOrigin);
        }

        private void BuildAxis(Vector3 origin) {
            _xAxis = new TranslateAxisHandle(Color.Blue,       Matrix.CreateRotationX(MathHelper.PiOver2)) { Origin = origin };
            _yAxis = new TranslateAxisHandle(Color.Red,        Matrix.CreateRotationY(MathHelper.PiOver2)) { Origin = origin };
            _zAxis = new TranslateAxisHandle(Color.LightGreen, Matrix.Identity) { Origin                            = origin };

            GameService.Graphics.World.AddEntities(new []{_xAxis, _yAxis, _zAxis});
        }

        public void Update(GameTime gameTime) {
            //_xAxis.Update(gameTime);
            //_yAxis.Update(gameTime);
            //_zAxis.Update(gameTime);
        }

        public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera) {
            //_xAxis.Render(graphicsDevice, world, camera);
            //_yAxis.Render(graphicsDevice, world, camera);
            //_zAxis.Render(graphicsDevice, world, camera);
        }

    }
}
