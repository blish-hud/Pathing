using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace BhModule.Community.Pathing.UI.Controls {
    internal class DonateHero : Container {

        private AsyncTexture2D _backgroundTexture = AsyncTexture2D.FromAssetId(1234872);
        private AsyncTexture2D _heartTexture = AsyncTexture2D.FromAssetId(156127);

        private BlueButton _donateButton;

        public DonateHero() {
            _donateButton = new BlueButton() {
                Text = "Donate",
                Parent = this,
            };

            _donateButton.Click += delegate {
                Process.Start("https://ko-fi.com/freesnow");
            };
        }

        public override void RecalculateLayout() {
            if (_donateButton != null) {
                _donateButton.Top = this.Height / 2 - 12;
                _donateButton.Right = this.Width - 12;
            }
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (!this.MouseOver) {
                spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, bounds, Color.Black * 0.75f);
            }

            spriteBatch.DrawOnCtrl(this, _backgroundTexture, bounds, Color.Pink * 0.8f);

            spriteBatch.DrawOnCtrl(this, _heartTexture, new Rectangle(-8, -8, 64, 64));
            spriteBatch.DrawOnCtrl(this, _heartTexture, new Rectangle(-8 + 32, -8, 64, 64));
            spriteBatch.DrawOnCtrl(this, _heartTexture, new Rectangle(-8 + 64, -8, 64, 64));
            spriteBatch.DrawStringOnCtrl(this, "Consider  helping  Freesnöw  with  server  expenses:", GameService.Content.DefaultFont18, bounds, Color.White, false, HorizontalAlignment.Center);
        }

    }
}
