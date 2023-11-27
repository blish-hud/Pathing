using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BhModule.Community.Pathing.UI.Controls {
    internal class SettingHero : Control {

        private AsyncTexture2D _backgroundTexture = AsyncTexture2D.FromAssetId(156353);

        private AsyncTexture2D _icon;
        public AsyncTexture2D Icon {
            get => _icon;
            set => SetProperty(ref _icon, value, true);
        }

        public string Text { get; set; }

        private Rectangle _rectIcon = Rectangle.Empty;
        private Rectangle _rectText = Rectangle.Empty;

        public override void RecalculateLayout() {
            if (_icon != null && _icon.HasTexture) {
                _rectIcon = new Rectangle(this.Width / 2 - _icon.Width / 2,
                                          20, 
                                          _icon.Width, 
                                          _icon.Height);
            }

            _rectText = new Rectangle(0, this.Height - 60, this.Width, 40);
        }

        protected override void Paint(SpriteBatch spriteBatch, Rectangle bounds) {
            //spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, this.Height - 1, this.Width, 1), Color.Black);

            if (_backgroundTexture.HasTexture && this.MouseOver) {
                spriteBatch.DrawOnCtrl(this, _backgroundTexture, new Rectangle(0, this.Height - 256, _backgroundTexture.Width, _backgroundTexture.Height), null, Color.Black, 0f, Vector2.Zero, SpriteEffects.FlipVertically);
            }

            if (_icon != null && _icon.HasTexture) {
                spriteBatch.DrawOnCtrl(this, _icon, _rectIcon);

                spriteBatch.DrawStringOnCtrl(this,
                    this.Text,
                    GameService.Content.DefaultFont18,
                    _rectText,
                    this.MouseOver 
                        ? Color.White 
                        : Control.StandardColors.Tinted, 
                    false, 
                    HorizontalAlignment.Center);
            }
        }

    }
}
