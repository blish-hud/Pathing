using System;
using System.Diagnostics;
using BhModule.Community.Pathing.MarkerPackRepo;
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

        // TODO: MarkerPackHero really probably should be a view but control is good for performance.

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

        private readonly PathingModule _module;
        private readonly MarkerPackPkg _markerPackPkg;

        private readonly BlueButton _downloadButton;
        private readonly BlueButton _infoButton;
        private readonly BlueButton _deleteButton;

        private readonly Checkbox _keepUpdatedCheckbox;

        private readonly string _lastUpdateStr = "";

        private double _hoverTick;
        private bool   _isUpToDate;

        public MarkerPackHero(PathingModule module, MarkerPackPkg markerPackPkg) {
            _module        = module;
            _markerPackPkg = markerPackPkg;

            if (markerPackPkg.LastUpdate != default) {
                _lastUpdateStr = $"Last update {markerPackPkg.LastUpdate.Humanize()}";
            }

            this.SuspendLayout();

            _keepUpdatedCheckbox = new Checkbox() {
                Text             = "Keep Updated",
                BasicTooltipText = "If checked, new pack versions will be automatically downloaded on launch.",
                Parent           = this,
                Checked          = markerPackPkg.AutoUpdate.Value,
                Enabled          = markerPackPkg.CurrentDownloadDate != default
            };

            _downloadButton = new BlueButton() {
                Text             = Strings.Repo_Download,
                Width            = 90,
                Parent           = this
            };

            if (_markerPackPkg.Size > 0) {
                // We don't know the size of all packs
                _downloadButton.BasicTooltipText = Math.Round(_markerPackPkg.Size, 2).Megabytes().Humanize();
            }

            _infoButton = new BlueButton() {
                Text             = Strings.Repo_Info,
                Width            = 90,
                Visible          = _markerPackPkg.Info != null,
                BasicTooltipText = _markerPackPkg.Info,
                Parent           = this
            };

            _deleteButton = new BlueButton() {
                Text   = "Delete",
                Width  = 90,
                Parent = this
            };

            if (_markerPackPkg.TotalDownloads > 0) {
                this.BasicTooltipText = $"Approx. {_markerPackPkg.TotalDownloads:n0} Downloads";
            }

            _downloadButton.Click                   += DownloadButtonOnClick;
            _infoButton.Click                       += InfoButtonOnClick;
            _deleteButton.Click                     += DeleteButtonOnClick;
            _keepUpdatedCheckbox.CheckedChanged     += KeepUpdatedCheckboxOnChecked;
            markerPackPkg.AutoUpdate.SettingChanged += AutoUpdateOnSettingChanged;

            this.Size    = new Point(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            this.Padding = new Thickness(13, 0, 0, 9);

            this.ResumeLayout(true);
        }

        private void AutoUpdateOnSettingChanged(object sender, ValueChangedEventArgs<bool> e) {
            _keepUpdatedCheckbox.Checked = e.NewValue;
        }

        private void DeleteButtonOnClick(object sender, MouseEventArgs e) {
            Utility.PackHandlingUtil.DeletePack(_module, _markerPackPkg);
        }

        private void KeepUpdatedCheckboxOnChecked(object sender, CheckChangedEvent e) {
            _markerPackPkg.AutoUpdate.Value = e.Checked;
        }

        private void DownloadButtonOnClick(object sender, MouseEventArgs e) {
            _downloadButton.Enabled = false;
            Utility.PackHandlingUtil.DownloadPack(_module, _markerPackPkg, OnComplete);
        }

        private static void OnComplete(MarkerPackPkg markerPackPkg, bool success) {
            markerPackPkg.IsDownloading = false;

            if (success) {
                markerPackPkg.CurrentDownloadDate = DateTime.UtcNow;
            }
        }

        private void UpdateControlStates() {
            string downloadText    = "Download";
            bool   downloadEnabled = true;

            _isUpToDate = false;

            if (_markerPackPkg.CurrentDownloadDate != default) {
                downloadEnabled = false;

                _deleteButton.Visible = true;
                _deleteButton.Enabled = true;

                if (_module.PackInitiator.IsLoading) {
                    downloadText    = "Loading...";
                } else if (_markerPackPkg.LastUpdate > _markerPackPkg.CurrentDownloadDate) {
                    downloadText    = "Update";
                    downloadEnabled = true;
                } else {
                    downloadText = "Up to Date";
                    _isUpToDate  = true;
                }
            } else {
                _deleteButton.Visible = false;
            }

            if (_markerPackPkg.IsDownloading) {
                downloadText    = $"Downloading...";
                downloadEnabled = false;

                _deleteButton.Enabled = false;
            }

            _downloadButton.Text    = downloadText;
            _downloadButton.Enabled = downloadEnabled;
            _downloadButton.Visible = !_markerPackPkg.IsDownloading && !_isUpToDate;
        }

        public override void UpdateContainer(GameTime gameTime) {
            UpdateControlStates();

            base.UpdateContainer(gameTime);
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
            _downloadButton.Location      = new Point(this.Width           - _downloadButton.Width      - EDGE_PADDING / 2, this.Height - 20 - _downloadButton.Height / 2);
            _infoButton.Location          = new Point(_downloadButton.Left - _infoButton.Width          - 5,                this.Height - 20 - _downloadButton.Height / 2);
            _deleteButton.Location        = new Point(_infoButton.Left     - _deleteButton.Width        - 5,                this.Height - 20 - _downloadButton.Height / 2);
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

            // Categories OR download error message
            if (_markerPackPkg.DownloadError == null) {
                spriteBatch.DrawStringOnCtrl(this, $"{_markerPackPkg.Categories}", GameService.Content.DefaultFont12, new Rectangle(EDGE_PADDING, bounds.Height - 40, _infoButton.Left - EDGE_PADDING / 2, 40), StandardColors.Default);
            } else {
                spriteBatch.DrawStringOnCtrl(this, $"Error: {_markerPackPkg.DownloadError}", GameService.Content.DefaultFont12, new Rectangle(EDGE_PADDING, bounds.Height - 40, _infoButton.Left - EDGE_PADDING / 2, 40), StandardColors.Red);
            }
            
            // Download % / Up to Date
            if (_markerPackPkg.IsDownloading) {
                spriteBatch.DrawStringOnCtrl(this, $"{Math.Min(_markerPackPkg.DownloadProgress, 99)}%", GameService.Content.DefaultFont14, _downloadButton.LocalBounds, StandardColors.Default, false, HorizontalAlignment.Center);
            } else if (_isUpToDate) {
                spriteBatch.DrawStringOnCtrl(this, "Up to Date", GameService.Content.DefaultFont14, _downloadButton.LocalBounds, StandardColors.Default, false, HorizontalAlignment.Center);
            }
        }

        protected override void DisposeControl() {
            _markerPackPkg.AutoUpdate.SettingChanged -= AutoUpdateOnSettingChanged;

            base.DisposeControl();
        }

    }
}
