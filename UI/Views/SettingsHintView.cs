using System;
using System.Diagnostics;
using System.Runtime.Remoting.Channels;
using BhModule.Community.Pathing.UI.Controls;
using BhModule.Community.Pathing.UI.Presenters;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Input;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Views {
    public class SettingsHintView : View {

        public event EventHandler<EventArgs> OpenSettingsClicked;

        public event EventHandler<EventArgs> OpenMarkerPacksClicked;

        private StandardButton _bttnOpenSettings;
        private StandardButton _bttnOpenSetupGuide;

        public SettingsHintView() { /* NOOP */  }

        public SettingsHintView((Action OpenSettings, Action OpenMarkerPacks, PackInitiator packInitiator) model) {
            this.WithPresenter(new SettingsHintPresenter(this, model));
        }

        protected override void Build(Container buildPanel) {
            var settingsHero = new SettingHero() {
                Icon = AsyncTexture2D.FromAssetId(156027),
                Text = "Open  Settings",
                Size = new Point(buildPanel.Width / 3, buildPanel.Height - 48),
                Parent = buildPanel
            };

            settingsHero.Click += delegate (object sender, MouseEventArgs e) {
                this.OpenSettingsClicked?.Invoke(this, e);
            };

            var downloadMpHero = new SettingHero() {
                Icon = AsyncTexture2D.FromAssetId(543438),
                Text = "Download  Marker  Packs",
                Size = new Point(buildPanel.Width / 3, buildPanel.Height - 48),
                Left = buildPanel.Width / 3,
                Parent = buildPanel
            };

            downloadMpHero.Click += delegate (object sender, MouseEventArgs e) {
                this.OpenMarkerPacksClicked?.Invoke(this, e);
            };

            var guideHero = new SettingHero() {
                Icon = AsyncTexture2D.FromAssetId(2604584),
                Text = "Open  Setup  Guide",
                Size = new Point(buildPanel.Width / 3, buildPanel.Height - 48),
                Left = buildPanel.Width / 3 * 2,
                Parent = buildPanel
            };

            guideHero.Click += delegate {
                Process.Start("https://link.blishhud.com/pathingsetup");
            };

            var donateHero = new DonateHero() {
                Size = new Point(buildPanel.Width, 48),
                Bottom = buildPanel.Height,
                Parent = buildPanel
            };
        }
    }
}
