using BhModule.Community.Pathing.UI.Controls;
using BhModule.Community.Pathing.UI.Presenter;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Views {
    public class PackRepoView : View {

        public FlowPanel RepoFlowPanel { get; private set; }

        private TextBox _searchBox;

        public PackRepoView(PathingModule module) {
            this.WithPresenter(new PackRepoPresenter(this, module));
        }

        protected override void Build(Container buildPanel) {
            _searchBox = new TextBox() {
                PlaceholderText = "Search marker packs...",
                Parent = buildPanel,
                Location = new Point(20, 10),
                Width = buildPanel.ContentRegion.Width - 40,
            };

            _searchBox.TextChanged += SearchBoxTextChanged;

            this.RepoFlowPanel = new FlowPanel {
                Size                = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _searchBox.Bottom - 12),
                Top                 = _searchBox.Bottom + 12,
                CanScroll           = true,
                ControlPadding      = new Vector2(0,  15),
                OuterControlPadding = new Vector2(20, 5),
                Parent              = buildPanel
            };
        }

        private void SearchBoxTextChanged(object sender, System.EventArgs e) {
            string searchText = _searchBox.Text.ToLowerInvariant();
            this.RepoFlowPanel.FilterChildren<MarkerPackHero>((hero) 
                => hero.MarkerPackPkg.Name.ToLowerInvariant().Contains(searchText)
                || (hero.MarkerPackPkg.Description ?? "").ToLowerInvariant().Contains(searchText)
                || (hero.MarkerPackPkg.Categories ?? "").ToLowerInvariant().Contains(searchText));
        }
    }
}
