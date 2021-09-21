using System;
using BhModule.Community.Pathing.UI.Effects;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.UI.Controls {
    public class InfoWindow : Container {

        private const double FADEDURATION = 300;

        private static readonly Texture2D _windowTexture;
        private static readonly Texture2D _windowMask;
        private static readonly Texture2D _windowClose;

        private bool   _showing        = false;
        private double _fadeCompletion = 0;

        static InfoWindow() {
            _windowTexture    = PathingModule.Instance.ContentsManager.GetTexture(@"png\controls\156475+156476.png");
            _windowMask       = PathingModule.Instance.ContentsManager.GetTexture(@"png\controls\156477.png");
            _windowClose      = PathingModule.Instance.ContentsManager.GetTexture(@"png\controls\156106.png");
        }

        public InfoWindow() {
            this.Size             = new Point(512, 512);
            this.Location         = new Point(300, 200);
            this.HeightSizingMode = SizingMode.AutoSize;
            this.AutoSizePadding  = new Point(40, 70);
            this.SpriteBatchParameters = new SpriteBatchParameters(SpriteSortMode.Immediate,
                                                                   BlendState.Additive,
                                                                   null,
                                                                   null,
                                                                   null,
                                                                   AlphaMaskEffect.SharedInstance);

            this.ClipsBounds = false;
        }

        private Texture2D _croppedWindow = _windowTexture;
        private Texture2D _croppedMask   = _windowMask;

        protected override void OnResized(ResizedEventArgs e) {
            base.OnResized(e);

            _croppedMask = _windowMask.GetRegion(new Rectangle(0,
                                                               512 - Math.Min(this.Height, _windowMask.Height),
                                                               Math.Min(this.Width,  _windowMask.Width),
                                                               Math.Min(this.Height, _windowMask.Height)));

            _croppedWindow = _windowTexture.GetRegion(new Rectangle(0,
                                                                    0,
                                                                    Math.Min(this.Size.X, _windowTexture.Width),
                                                                    Math.Min(this.Size.Y, _windowTexture.Height)));
        }

        private void TriggerFade() {
            float fadeOffset = _showing ? 1f - this.Opacity : this.Opacity;

            _fadeCompletion = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds + (FADEDURATION * fadeOffset);
        }

        public override void Show() {
            if (_showing) return;

            base.Show();
            _showing = true;
            TriggerFade();
        }

        public void Hide(bool withFade) {
            if (withFade) {
                Hide();
            } else {
                base.Hide();
            }
        }

        public override void Hide() {
            _showing = false;
            TriggerFade();
        }

        public override void UpdateContainer(GameTime gameTime) {
            float fadeLerp = MathHelper.Clamp((float)((gameTime.TotalGameTime.TotalMilliseconds - _fadeCompletion) / FADEDURATION), 0f, 1f);

            this.Opacity = _showing ? fadeLerp : 1f - fadeLerp;

            base.UpdateContainer(gameTime);
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            if (_closeButtonBounds.Contains(this.RelativeMousePosition)) {
                Hide();
            }
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Mouse;
        }

        private Rectangle _closeButtonBounds;

        public override void RecalculateLayout() {
            _closeButtonBounds = new Rectangle(this.Width - 64, 45, 32, 32);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            AlphaMaskEffect.SharedInstance.SetEffectState(_croppedMask);

            spriteBatch.DrawOnCtrl(this, _croppedWindow, bounds, Color.White * 0.9f);

            AlphaMaskEffect.SharedInstance.SetEffectState(ContentService.Textures.Pixel);

            spriteBatch.DrawOnCtrl(this, _windowClose, _closeButtonBounds);
        }

    }
}
