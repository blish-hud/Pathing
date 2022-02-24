using System;
using System.Diagnostics;
using BhModule.Community.Pathing.UI.Presenters;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Views {
    public class SettingsHintView : View {

        /* Basic hint view for now.
         * Plan to update the visuals here to potentially include a list of loaded packs, etc.
         */

        public event EventHandler<EventArgs> OpenSettingsClicked;

        private StandardButton _bttnOpenSettings;
        private StandardButton _bttnOpenSetupGuide;

        public SettingsHintView() { /* NOOP */  }

        public SettingsHintView((Action OpenSettings, PackInitiator packInitiator) model) {
            this.WithPresenter(new SettingsHintPresenter(this, model));
        }

        protected override void Build(Container buildPanel) {
            _bttnOpenSettings = new StandardButton() {
                Text   = "Open Settings",
                Width  = 192,
                Parent = buildPanel,
            };

            _bttnOpenSetupGuide = new StandardButton() {
                Text   = "Open Setup Guide",
                Width  = _bttnOpenSettings.Width,
                Parent = buildPanel,
            };

            _bttnOpenSettings.Location   = new Point(Math.Max(buildPanel.Width / 2 - _bttnOpenSettings.Width / 2, 20), Math.Max(buildPanel.Height / 2 - _bttnOpenSettings.Height, 20) - _bttnOpenSettings.Height - 10);
            _bttnOpenSetupGuide.Location = new Point(_bttnOpenSettings.Left,                                           _bttnOpenSettings.Bottom                                       + 10);

            _bttnOpenSettings.Click   += _bttnOpenSettings_Click;
            _bttnOpenSetupGuide.Click += BttnOpenSetupGuideClick;
        }

        private void _bttnOpenSettings_Click(object sender, Blish_HUD.Input.MouseEventArgs e) {
            this.OpenSettingsClicked?.Invoke(this, e);
        }

        private void BttnOpenSetupGuideClick(object sender, Blish_HUD.Input.MouseEventArgs e) {
            Process.Start("https://link.blishhud.com/pathingsetup");
        }

        protected override void Unload() {
            if (_bttnOpenSettings != null)
                _bttnOpenSettings.Click -= _bttnOpenSettings_Click;

            if (_bttnOpenSetupGuide != null)
                _bttnOpenSetupGuide.Click -= BttnOpenSetupGuideClick;
        }
    }
}
