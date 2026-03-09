using BhModule.Community.Pathing.UI.Controls;
using BhModule.Community.Pathing.UI.Presenter;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.UI.Views {
    public class PackRepoView : View {

        public FlowPanel RepoFlowPanel { get; private set; }

        private TextBox _searchBox;

        private Checkbox _onlyShowCurrentMap;

        private int _currentMap = -1;

        public PackRepoView(PathingModule module) {
            this.WithPresenter(new PackRepoPresenter(this, module));
        }

        protected override Task<bool> Load(IProgress<string> progress) {
            _currentMap = GameService.Gw2Mumble.CurrentMap.Id;
            GameService.Gw2Mumble.CurrentMap.MapChanged += CurrentMapMapChanged;

            return base.Load(progress);
        }

        protected override void Build(Container buildPanel) {
            _searchBox = new TextBox() {
                PlaceholderText = "Search marker packs...",
                Parent = buildPanel,
                Location = new Point(20, 10),
                Width = buildPanel.ContentRegion.Width - 40,
            };

            _searchBox.TextChanged += SearchBoxTextChanged;

            _onlyShowCurrentMap = new Checkbox() {
                Text = "Current Map Only",
                BasicTooltipText = "If checked, only marker packs with marker or trails for the current map will be shown.",
                Parent = buildPanel,
                Location = new Point(_searchBox.Left + 8, _searchBox.Bottom + 8)
            };

            _onlyShowCurrentMap.CheckedChanged += OnlyShowCurrentMapCheckedChanged;

            this.RepoFlowPanel = new FlowPanel {
                Size                = new Point(buildPanel.ContentRegion.Width, buildPanel.ContentRegion.Height - _onlyShowCurrentMap.Bottom - 12),
                Top                 = _onlyShowCurrentMap.Bottom + 12,
                CanScroll           = true,
                ControlPadding      = new Vector2(0,  15),
                OuterControlPadding = new Vector2(20, 5),
                Parent              = buildPanel
            };
        }

        private void CurrentMapMapChanged(object sender, ValueEventArgs<int> e) {
            _currentMap = e.Value;
            UpdateFilter();
        }

        private void OnlyShowCurrentMapCheckedChanged(object sender, CheckChangedEvent e) {
            UpdateFilter();
        }

        private void SearchBoxTextChanged(object sender, System.EventArgs e) {
            UpdateFilter();
        }

        private void UpdateFilter() {
            string searchText = _searchBox.Text.ToLowerInvariant();
            this.RepoFlowPanel.FilterChildren<MarkerPackHero>((hero)
                => (hero.MarkerPackPkg.Name.ToLowerInvariant().Contains(searchText)
                || (hero.MarkerPackPkg.Description ?? "").ToLowerInvariant().Contains(searchText)
                || (hero.MarkerPackPkg.Categories ?? "").ToLowerInvariant().Contains(searchText))
                && (!_onlyShowCurrentMap.Checked || hero.MarkerPackPkg.MapIds.Contains(_currentMap)));
        }

        protected override void Unload() {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= CurrentMapMapChanged;

            base.Unload();
        }
    }
}
