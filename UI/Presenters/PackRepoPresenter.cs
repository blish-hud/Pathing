using System;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Controls;
using BhModule.Community.Pathing.UI.Views;
using Blish_HUD.Graphics.UI;

namespace BhModule.Community.Pathing.UI.Presenter {

    public class PackRepoPresenter : Presenter<PackRepoView, MarkerPackRepo.MarkerPackRepo> {

        private readonly PathingModule _module;

        public PackRepoPresenter(PackRepoView view, PathingModule module) : base(view, module.MarkerPackRepo) {
            _module = module;
        }

        protected override Task<bool> Load(IProgress<string> progress) {
            return Task.FromResult(true);
        }

        protected override void UpdateView() {
            this.View.RepoFlowPanel.ClearChildren();

            foreach (var markerPackPkg in this.Model.MarkerPackages.OrderByDescending(markerPkg => markerPkg.LastUpdate)) {
                var nHero = new MarkerPackHero(_module, markerPackPkg) {
                    Parent = this.View.RepoFlowPanel,
                    Width  = this.View.RepoFlowPanel.Width - 60
                };
            }
        }

    }
}
