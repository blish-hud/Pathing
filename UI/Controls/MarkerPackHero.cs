using System;
using System.Diagnostics;
using System.IO;
using BhModule.Community.Pathing.UI.Presenter;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Flurl;
using Flurl.Http;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.UI.Controls {
    public class MarkerPackHero : Container {

        // TODO: MarkerPackHero really should be a view.

        private static readonly Logger Logger = Logger.GetLogger<MarkerPackHero>();

        private const int DEFAULT_WIDTH  = 500;
        private const int DEFAULT_HEIGHT = 170;

        private const int EDGE_PADDING = 20;

        private const double FADE_DURATION = 150;

        #region Load Static

        private static readonly Texture2D _textureHeroBackground;

        static MarkerPackHero() {
            _textureHeroBackground = PathingModule.Instance.ContentsManager.GetTexture(@"png\controls\155209.png");
        }

        #endregion

        private readonly PackRepoPresenter.MarkerPackPkg _markerPackPkg;

        private readonly BlueButton _downloadButton;
        private readonly BlueButton _infoButton;

        private double _hoverTick;

        public MarkerPackHero(PackRepoPresenter.MarkerPackPkg markerPackPkg) {
            _markerPackPkg = markerPackPkg;

            this.SuspendLayout();

            _downloadButton = new BlueButton() {
                Text   = Strings.Repo_Download,
                Width  = 90,
                Parent = this
            };

            _infoButton = new BlueButton() {
                Text    = Strings.Repo_Info,
                Width   = 90,
                Visible = _markerPackPkg.Info != null,
                Parent  = this
            };

            _downloadButton.Click += DownloadButtonOnClick;
            _infoButton.Click += InfoButtonOnClick;

            this.Size    = new Point(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            this.Padding = new Thickness(13, 0, 0, 9);

            this.ResumeLayout(true);
        }

        private async void DownloadButtonOnClick(object sender, MouseEventArgs e) {
            _downloadButton.Enabled = false;
            _downloadButton.Text    = "Downloading...";

            byte[] downloadedPack;

            try {
                 downloadedPack = await _markerPackPkg.Download.WithHeader("user-agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36").GetBytesAsync();
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to download marker pack!");
                return;
            }

            string fullPath = Path.Combine(Utility.DataDirUtil.MarkerDir, _markerPackPkg.FileName);
            
            if (!File.Exists(fullPath)) {
                File.WriteAllBytes(fullPath, downloadedPack);
            } else {
                Logger.Warn($"Module already exists at path '{fullPath}'.");
                return;
            }

            Logger.Info($"Marker pack saved to '{fullPath}'.");

            _downloadButton.Text    = "Downloaded";
            //_downloadButton.Enabled = true;
        }

        private void InfoButtonOnClick(object sender, MouseEventArgs e) {
            if (Url.IsValid(_markerPackPkg.Info)) {
                Process.Start(_markerPackPkg.Info);
            }
        }

        protected override void OnMouseEntered(MouseEventArgs e) {
            _hoverTick = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;

            base.OnMouseEntered(e);
        }

        protected override void OnMouseLeft(MouseEventArgs e) {
            _hoverTick = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds;

            base.OnMouseLeft(e);
        }

        private float GetHoverFade() {
            double duration = GameService.Overlay.CurrentGameTime.TotalGameTime.TotalMilliseconds - _hoverTick;

            float offset = MathHelper.Lerp(0f, 0.4f, MathHelper.Clamp((float)(duration / FADE_DURATION), 0f, 1f));

            return this.MouseOver
                       ? 0.4f + offset
                       : 0.8f - offset;
        }

        public override void RecalculateLayout() {
            _downloadButton.Location = new Point(this.Width           - _downloadButton.Width - EDGE_PADDING / 2, this.Height - 20 - _downloadButton.Height / 2);
            _infoButton.Location     = new Point(_downloadButton.Left - _infoButton.Width     - 5,                this.Height - 20 - _downloadButton.Height / 2);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // Background
            spriteBatch.DrawOnCtrl(this, _textureHeroBackground, new Rectangle(-9, -13, _textureHeroBackground.Width, _textureHeroBackground.Height), Color.White * GetHoverFade());

            // Name and description
            spriteBatch.DrawStringOnCtrl(this, _markerPackPkg.Name.Replace(" ", "  "), GameService.Content.DefaultFont18, new Rectangle(EDGE_PADDING, EDGE_PADDING / 2, bounds.Width - EDGE_PADDING * 2, 40), ContentService.Colors.Chardonnay);
            spriteBatch.DrawStringOnCtrl(this, _markerPackPkg.Description.Replace(@"\r\n", "\n").Replace(@"\n", "\n"), GameService.Content.DefaultFont14, new Rectangle(EDGE_PADDING, 40 + EDGE_PADDING / 2, bounds.Width - EDGE_PADDING * 2, bounds.Height - 200), StandardColors.Default, true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Black bottom bar
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, bounds.Height - 40, bounds.Width, 40), Color.Black * 0.8f);

            // Categories
            spriteBatch.DrawStringOnCtrl(this, $"{Strings.Repo_Categories}: {_markerPackPkg.Categories}", GameService.Content.DefaultFont12, new Rectangle(EDGE_PADDING, bounds.Height - 40, _infoButton.Left - EDGE_PADDING / 2, 40), StandardColors.Default);
        }

    }
}
