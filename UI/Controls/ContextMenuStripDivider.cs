using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.UI.Controls {
    internal class ContextMenuStripDivider : ContextMenuStripItem {

        public ContextMenuStripDivider() {
            this.EffectBehind = null;
        }

        public override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);

            this.Height = 2;
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.White * 0.8f);
        }

    }
}
