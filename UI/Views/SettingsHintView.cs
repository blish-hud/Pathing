using System;
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

        public SettingsHintView() { /* NOOP */  }

        public SettingsHintView((Action OpenSettings, PackInitiator packInitiator) model) {
            this.WithPresenter(new SettingsHintPresenter(this, model));
        }

        protected override void Build(Container buildPanel) {
            _bttnOpenSettings = new StandardButton() {
                Text = "Open Settings",
                Parent = buildPanel,
            };

            _bttnOpenSettings.Location = new Point(Math.Min(buildPanel.Width / 2 - _bttnOpenSettings.Width / 2, 20), Math.Min(buildPanel.Height / 2 - _bttnOpenSettings.Height, 20));

            _bttnOpenSettings.Click += _bttnOpenSettings_Click;
        }

        private void _bttnOpenSettings_Click(object sender, Blish_HUD.Input.MouseEventArgs e) {
            this.OpenSettingsClicked?.Invoke(this, e);
        }

        protected override void Unload() {
            _bttnOpenSettings.Click -= _bttnOpenSettings_Click;
        }
    }
}
