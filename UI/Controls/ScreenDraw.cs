using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.UI.Controls {
    public class ScreenDraw : Control {

        protected override CaptureType CapturesInput() {
            return CaptureType.DoNotBlock;
        }

        public override void DoUpdate(GameTime gameTime) {
            if (this.Parent != null) {
                this.Size = this.Parent.Size;
            }
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            Viewport viewport       = spriteBatch.GraphicsDevice.Viewport;
            Vector3  screenPosition = viewport.Project(new Vector3(WorldUtil.GameToWorldCoord(GameService.Gw2Mumble.PlayerCharacter.Position.X), WorldUtil.GameToWorldCoord(GameService.Gw2Mumble.PlayerCharacter.Position.Y), WorldUtil.GameToWorldCoord(GameService.Gw2Mumble.PlayerCharacter.Position.Z)), GameService.Gw2Mumble.PlayerCamera.Projection, GameService.Gw2Mumble.PlayerCamera.View, GameService.Gw2Mumble.PlayerCamera.PlayerView);
            Vector2  spritePosition = new Vector2(screenPosition.X, screenPosition.Y);

            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle((int)screenPosition.X, (int)screenPosition.Y, 50, 40), Color.LightBlue);
            spriteBatch.DrawStringOnCtrl(this, "Hello!", GameService.Content.DefaultFont18, new Rectangle((int)screenPosition.X, (int)screenPosition.Y, 50, 40), Color.Magenta);
        }

    }
}
