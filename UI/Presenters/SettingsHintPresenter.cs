using System;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Views;
using Blish_HUD.Graphics.UI;

namespace BhModule.Community.Pathing.UI.Presenters {
    public class SettingsHintPresenter : Presenter<SettingsHintView, (Action OpenSettings, Action OpenMarkerPacks, PackInitiator packInitiator)> {

        public SettingsHintPresenter(SettingsHintView view, (Action OpenSettings, Action OpenMarkerPacks, PackInitiator packInitiator) model) : base(view, model) { }

        protected override Task<bool> Load(IProgress<string> progress) {
            this.View.OpenSettingsClicked += View_OpenSettingsClicked;
            this.View.OpenMarkerPacksClicked += View_OpenMarkerPacksClicked;

            return base.Load(progress);
        }

        private void View_OpenMarkerPacksClicked(object sender, EventArgs e) {
            this.Model.OpenMarkerPacks.Invoke();
        }

        private void View_OpenSettingsClicked(object sender, EventArgs e) {
            this.Model.OpenSettings.Invoke();
        }

        protected override void Unload() {
            this.View.OpenSettingsClicked -= View_OpenSettingsClicked;
            this.View.OpenMarkerPacksClicked -= View_OpenMarkerPacksClicked;
        }
    }
}
