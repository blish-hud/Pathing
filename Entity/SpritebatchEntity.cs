using System.Collections.Generic;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Entity {
    public class SpritebatchEntity : IEntity {

        public List<Control> Controls { get; } = new();

        public float DrawOrder => float.MinValue;

        public void Render(GraphicsDevice graphicsDevice, IWorld world, ICamera camera) {
            //using (var spritebatch = new SpriteBatch(graphicsDevice)) {
            //    foreach (var control in this.Controls) {
            //        control.
            //    }
            //}
        }

        public void Update(GameTime gameTime) {
            
        }
    }
}
