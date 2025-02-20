using System.Linq;
using BhModule.Community.Pathing.UI.Controls.TreeView;
using BhModule.Community.Pathing.UI.Presenter;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.UI.Views {
    public class MarkerTreeView : View {

        public FlowPanel RepoFlowPanel { get; private set; }

        public TreeView TreeView { get; private set; }

        private TextBox _searchBox;

        public MarkerTreeView(PathingModule module) {
            this.WithPresenter(new MarkerTreePresenter(this, module));
        }

        protected override void Build(Container buildPanel) {
            _searchBox = new TextBox {
                PlaceholderText = "Search markers...",
                Parent = buildPanel,
                Location = new Point(0, 10),
                Width = buildPanel.ContentRegion.Width - 30,
            };

            _searchBox.TextChanged += SearchBoxTextChanged;

            this.RepoFlowPanel = new CustomFlowPanel {
                Size                = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _searchBox.Bottom - 12),
                Top                 = _searchBox.Bottom + 5,
                CanScroll           = true,
                Parent              = buildPanel
            };

            this.TreeView = new TreeView {
                HeightSizingMode = SizingMode.AutoSize,
                Size             = new Point(RepoFlowPanel.Width, RepoFlowPanel.Height),
                Parent = RepoFlowPanel
            };
        }

        private void SearchBoxTextChanged(object sender, System.EventArgs e) {
            if (Presenter is MarkerTreePresenter presenter) {
                if (string.IsNullOrWhiteSpace(_searchBox.Text)) {
                    presenter.ResetView();
                    return;
                }

                var results = presenter.Search(_searchBox.Text).ToList();

                presenter.SetSearchResults(results);
            }
        }
    }
}
