using System;
using System.Diagnostics;
using BhModule.Community.Pathing.MarkerPackRepo;
using BhModule.Community.Pathing.UI.Presenter;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Input;
using Blish_HUD.Settings;
using Flurl;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Humanizer;

namespace BhModule.Community.Pathing.UI.Controls {
    public class MarkerPackHero : Container {

        // TODO: MarkerPackHero really probably should be a view.

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

        private readonly MarkerPackPkg _markerPackPkg;

        private readonly BlueButton _downloadButton;
        private readonly BlueButton _infoButton;

        private readonly Checkbox _keepUpdatedCheckbox;

        private readonly SettingEntry<bool> DoAutoUpdate;

        private readonly string _lastUpdateStr = "";

        private double _hoverTick;

        public MarkerPackHero(MarkerPackPkg markerPackPkg, SettingCollection settings) {
            this.DoAutoUpdate = settings.DefineSetting(markerPackPkg.Name + "_AutoUpdate", true);

            _markerPackPkg = markerPackPkg;

            if (markerPackPkg.LastUpdate != default) {
                _lastUpdateStr = $"Last update {markerPackPkg.LastUpdate.Humanize()}";
            }

            this.SuspendLayout();

            _keepUpdatedCheckbox = new Checkbox() {
                Text             = "Keep Updated",
                BasicTooltipText = "If checked, new pack versions will be automatically downloaded on launch.",
                Parent           = this,
                Checked          = this.DoAutoUpdate.Value,
                Enabled          = markerPackPkg.CurrentDownloadDate != default
            };

            _downloadButton = new BlueButton() {
                Text             = Strings.Repo_Download,
                Width            = 90,
                BasicTooltipText = ((double)Math.Round(_markerPackPkg.Size, 2)).Megabytes().Humanize(),
                Parent           = this
            };

            _infoButton = new BlueButton() {
                Text             = Strings.Repo_Info,
                Width            = 90,
                Visible          = _markerPackPkg.Info != null,
                BasicTooltipText = _markerPackPkg.Info,
                Parent           = this
            };

            _downloadButton.Click               += DownloadButtonOnClick;
            _infoButton.Click                   += InfoButtonOnClick;
            _keepUpdatedCheckbox.CheckedChanged += KeepUpdatedCheckbox_CheckedChanged;

            this.Size    = new Point(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            this.Padding = new Thickness(13, 0, 0, 9);

            this.ResumeLayout(true);
        }

        private void KeepUpdatedCheckbox_CheckedChanged(object sender, CheckChangedEvent e) {
            DoAutoUpdate.Value = e.Checked;
        }

        private void DownloadButtonOnClick(object sender, MouseEventArgs e) {
            _downloadButton.Enabled = false;
            Utility.PackHandlingUtil.DownloadPack(_markerPackPkg, OnComplete);
        }

        private static void OnComplete(MarkerPackPkg markerPackPkg, bool success) {
            markerPackPkg.IsDownloading = false;

            if (success) {
                markerPackPkg.CurrentDownloadDate = DateTime.UtcNow;
            }
        }

        private void UpdateDownloadButtonState() {
            string downloadText    = "Download";
            bool   downloadEnabled = true;

            if (_markerPackPkg.CurrentDownloadDate != default) {
                downloadEnabled = false;

                if (PathingModule.Instance.PackInitiator.IsLoading) {
                    downloadText    = "Loading...";
                } else if (_markerPackPkg.LastUpdate > _markerPackPkg.CurrentDownloadDate) {
                    downloadText    = "Update";
                    downloadEnabled = true;
                } else {
                    downloadText = "Up to Date";
                }
            }

            if (_markerPackPkg.IsDownloading) {
                downloadText    = "Downloading...";
                downloadEnabled = false;
            }

            _downloadButton.Text    = downloadText;
            _downloadButton.Enabled = downloadEnabled;
        }

        public override void UpdateContainer(GameTime gameTime) {
            UpdateDownloadButtonState();

            base.UpdateContainer(gameTime);
        }

        private void InfoButtonOnClick(object sender, MouseEventArgs e) {
            if (Url.IsValid(_markerPackPkg.Info)) {
                // TODO: Let's do something more to prevent something slipping in to process.start - even if we host the repo.
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
            _downloadButton.Location      = new Point(this.Width           - _downloadButton.Width      - EDGE_PADDING / 2, this.Height - 20 - _downloadButton.Height / 2);
            _infoButton.Location          = new Point(_downloadButton.Left - _infoButton.Width          - 5,                this.Height - 20 - _downloadButton.Height / 2);
            _keepUpdatedCheckbox.Location = new Point(this.Width           - _keepUpdatedCheckbox.Width - EDGE_PADDING,     EDGE_PADDING);
        }

        public override void PaintBeforeChildren(SpriteBatch spriteBatch, Rectangle bounds) {
            // Background
            spriteBatch.DrawOnCtrl(this, _textureHeroBackground, new Rectangle(-9, -13, _textureHeroBackground.Width, _textureHeroBackground.Height), Color.White * GetHoverFade());

            // Name and description
            spriteBatch.DrawStringOnCtrl(this, _markerPackPkg.Name.Replace(" ", "  "), GameService.Content.DefaultFont18, new Rectangle(EDGE_PADDING, EDGE_PADDING / 2, bounds.Width - EDGE_PADDING * 2, 40), ContentService.Colors.Chardonnay);
            spriteBatch.DrawStringOnCtrl(this, _markerPackPkg.Description.Replace(@"\n", "\n"), GameService.Content.DefaultFont14, new Rectangle(EDGE_PADDING, 40 + EDGE_PADDING / 2, bounds.Width - EDGE_PADDING, bounds.Height - 200), StandardColors.Default, true, HorizontalAlignment.Left, VerticalAlignment.Top);

            // Current version
            spriteBatch.DrawStringOnCtrl(this, _lastUpdateStr, GameService.Content.DefaultFont14, new Rectangle(EDGE_PADDING, EDGE_PADDING / 2, _keepUpdatedCheckbox.Left - EDGE_PADDING * 2, EDGE_PADDING * 2 - 5), ContentService.Colors.Chardonnay,
                                         false, HorizontalAlignment.Right);

            // Black bottom bar
            spriteBatch.DrawOnCtrl(this, ContentService.Textures.Pixel, new Rectangle(0, bounds.Height - 40, bounds.Width, 40), Color.Black * 0.8f);

            // Categories
            spriteBatch.DrawStringOnCtrl(this, $"{Strings.Repo_Categories}: {_markerPackPkg.Categories}", GameService.Content.DefaultFont12, new Rectangle(EDGE_PADDING, bounds.Height - 40, _infoButton.Left - EDGE_PADDING / 2, 40), StandardColors.Default);
        }

    }
}
