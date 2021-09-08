using BhModule.Community.Pathing.UI.Presenter;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Views {
    public class PackRepoView : View {

        public FlowPanel RepoFlowPanel { get; private set; }

        public PackRepoView() {
            this.WithPresenter(new PackRepoPresenter(this, 0));
        }

        protected override void Build(Container buildPanel) {
            this.RepoFlowPanel = new FlowPanel {
                Size                = buildPanel.ContentRegion.Size,
                Top                 = 0,
                CanScroll           = true,
                ControlPadding      = new Vector2(0,  15),
                OuterControlPadding = new Vector2(20, 5),
                Parent              = buildPanel
            };
        }

    }
}
