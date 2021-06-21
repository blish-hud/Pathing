using System;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Controls;
using BhModule.Community.Pathing.UI.Views;
using Blish_HUD;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;
using Flurl.Http;

namespace BhModule.Community.Pathing.UI.Presenter {

    public class PackRepoPresenter : Presenter<PackRepoView, int> {

        public struct MarkerPackPkg {

            public string   Name           { get; set; }
            public string   Description    { get; set; }
            public string   Download       { get; set; }
            public string   Info           { get; set; }
            public string   FileName       { get; set; }
            public string   Categories     { get; set; }
            public string   Version        { get; set; }
            public float    Size           { get; set; }
            public int      TotalDownloads { get; set; }
            public string   AuthorName     { get; set; }
            public string   AuthorUsername { get; set; }
            public DateTime LastUpdate     { get; set; }

        }

        private static readonly Logger Logger = Logger.GetLogger<PackRepoPresenter>();

        private const string REPO_SETTING   = "PackRepoUrl";
        private const string PUBLIC_REPOURL = "https://mp-repo.blishhud.com/repo.json";

        private string _repoUrl;

        private static MarkerPackPkg[] _markerPkgCache;

        public PackRepoPresenter(PackRepoView view, int model) : base(view, model) { }

        protected override async Task<bool> Load(IProgress<string> progress) {
            progress.Report("Loading...");

            DefineRepoSettings();

            _markerPkgCache ??= await LoadMarkerPackPkgs(progress);

            Logger.Info($"Found {_markerPkgCache.Length} marker packs from {_repoUrl}.");

            return _markerPkgCache.Length > 0;
        }

        private void DefineRepoSettings() {
            _repoUrl = PathingModule.Instance.SettingsManager.ModuleSettings.DefineSetting(REPO_SETTING, PUBLIC_REPOURL).Value;
        }

        private async Task<MarkerPackPkg[]> LoadMarkerPackPkgs(IProgress<string> progress) {
            progress.Report("Requesting latest list of marker packs...");

            (MarkerPackPkg[] releases, var exception) = await RequestMarkerPacks();

            if (exception != null) {
                progress.Report($"Failed to get a list of marker packs.\r\n{exception.Message}");
            }

            return releases;
        }

        private async Task<(MarkerPackPkg[] Releases, Exception Exception)> RequestMarkerPacks() {
            try {
                return (await _repoUrl.WithHeader("User-Agent", "Blish-HUD").GetJsonAsync<MarkerPackPkg[]>(), null);
            } catch (FlurlHttpException ex) {
                Logger.Warn(ex, "Failed to get list of marker packs");
                return (Array.Empty<MarkerPackPkg>(), ex);
            }
        }

        protected override void UpdateView() {
            this.View.RepoFlowPanel.ClearChildren();

            foreach (var markerPackPkg in _markerPkgCache) {
                var nHero = new MarkerPackHero(markerPackPkg) {
                    Parent = this.View.RepoFlowPanel,
                    Width  = this.View.RepoFlowPanel.Width - 60
                };
            }
        }

    }
}
