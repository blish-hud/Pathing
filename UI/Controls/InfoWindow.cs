using System;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Effects;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BhModule.Community.Pathing.UI.Controls {
    public class InfoWindow : Container {

        private const double FADEDURATION = 300;

        private static readonly Texture2D _windowTexture;
        private static readonly Texture2D _windowMask;
        private static readonly Texture2D _windowClose;

        static InfoWindow() {
            _windowTexture    = PathingModule.Instance.ContentsManager.GetTexture(@"png\controls\156475+156476.png");
            _windowMask       = PathingModule.Instance.ContentsManager.GetTexture(@"png\controls\156477.png");
            _windowClose      = PathingModule.Instance.ContentsManager.GetTexture(@"png\controls\156106.png");
        }

        private bool   _showing        = false;
        private double _fadeCompletion = 0;

        private readonly IPackState _packState;

        public InfoWindow(IPackState packState) {
            _packState = packState;

            this.Size             = new Point(512, 512);
            this.Location         = new Point(_packState.UserResourceStates.Advanced.InfoWindowXOffsetPixels, _packState.UserResourceStates.Advanced.InfoWindowYOffsetPixels);
            this.HeightSizingMode = SizingMode.AutoSize;
            this.AutoSizePadding  = new Point(40, 70);
            this.SpriteBatchParameters = new SpriteBatchParameters(SpriteSortMode.Immediate,
                                                                   BlendState.Additive,
                                                                   null,
                                                                   null,
                                                                   null,
                                                                   AlphaMaskEffect.SharedInstance);

            this.Opacity = 0f;

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
            if (PathingModule.Instance.Settings.PackInfoDisplayMode.Value == MarkerInfoDisplayMode.NeverDisplay) return;
            if (_showing) return;

            base.Show();
            _showing = true;
            TriggerFade();
        }

        public void Hide(bool withFade) {
            _showing = false;

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
            float fadeLerp = MathHelper.Clamp((float)((_fadeCompletion - gameTime.TotalGameTime.TotalMilliseconds) / FADEDURATION), 0f, 1f);

            this.Opacity = _showing ? 1f - fadeLerp : fadeLerp;

            if (!_showing && fadeLerp <= 0f) {
                this.Visible = false;
            }

            if (PathingModule.Instance.Settings.PackInfoDisplayMode.Value == MarkerInfoDisplayMode.NeverDisplay) {
                Hide();
            }

            base.UpdateContainer(gameTime);
        }

        protected override void OnClick(MouseEventArgs e) {
            base.OnClick(e);

            if (_closeButtonBounds.Contains(this.RelativeMousePosition)) {
                Hide();
            }
        }

        protected override CaptureType CapturesInput() {
            return CaptureType.Filter;
        }

        private Rectangle _closeButtonBounds;

        public override void RecalculateLayout() {
            _closeButtonBounds = new Rectangle(this.Width - 64, 45, 32, 32);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBounds, Rectangle scissor) {
            if (PathingModule.Instance == null) return;

            // Don't show on loading screens or during vistas.  This is not a clean way to do this.
            if (!GameService.GameIntegration.Gw2Instance.IsInGame) return;

            base.Draw(spriteBatch, drawBounds, scissor);
        }

        public override Control TriggerMouseInput(MouseEventType mouseEventType, MouseState ms) {
            // Force us to not block when the mouse touches this control.  This is also not a clean way to do this.
            _ = base.TriggerMouseInput(mouseEventType, ms);

            return null;
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            if (PathingModule.Instance == null) return;

            // Don't show on loading screens or during vistas.
            if (!GameService.GameIntegration.Gw2Instance.IsInGame || PathingModule.Instance.Settings.PackInfoDisplayMode.Value == MarkerInfoDisplayMode.WithoutBackground) return;

            AlphaMaskEffect.SharedInstance.SetEffectState(_croppedMask);

            spriteBatch.DrawOnCtrl(this, _croppedWindow, bounds, Color.White * 0.9f);

            AlphaMaskEffect.SharedInstance.SetEffectState(ContentService.Textures.Pixel);

            spriteBatch.DrawOnCtrl(this, _windowClose, _closeButtonBounds);
        }

    }
}
