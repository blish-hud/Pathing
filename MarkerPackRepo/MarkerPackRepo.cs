using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Flurl.Http;

namespace BhModule.Community.Pathing.MarkerPackRepo {
    public class MarkerPackRepo {

        private static readonly Logger Logger = Logger.GetLogger<MarkerPackRepo>();

        private const string REPO_SETTING   = "PackRepoUrl";
        private const string PUBLIC_REPOURL = "https://mp-repo.blishhud.com/repo.json";

        private string _repoUrl;

        public MarkerPackPkg[] MarkerPackages { get; private set; } = Array.Empty<MarkerPackPkg>();

        public void Init() {
            var beginThread = new Thread(async () => await Load(PathingModule.Instance.GetModuleProgressHandler())) { IsBackground = true };
            beginThread.Start();
        }

        private async Task Load(IProgress<string> progress) {
            DefineRepoSettings();

            MarkerPackages = await LoadMarkerPackPkgs(progress);

            Logger.Info($"Found {MarkerPackages.Length} marker packs from {_repoUrl}.");

            LoadLocalPackInfo();

            progress.Report(null);
        }

        private void DefineRepoSettings() {
            _repoUrl = PathingModule.Instance.SettingsManager.ModuleSettings.DefineSetting(REPO_SETTING, PUBLIC_REPOURL).Value;
        }

        private void LoadLocalPackInfo() {
            string directory = PathingModule.Instance.DirectoriesManager.GetFullDirectoryPath("markers");

            string[] existingTacoPacks = Directory.GetFiles(directory, "*.taco", SearchOption.AllDirectories);
            string[] existingZipPacks  = Directory.GetFiles(directory, "*.zip",  SearchOption.AllDirectories);

            string[] existingPacks = existingTacoPacks.Concat(existingZipPacks).ToArray();

            foreach (var pack in MarkerPackages) {
                string basePackName = Path.GetFileNameWithoutExtension(pack.FileName).ToLowerInvariant();

                string associatedLocalPack = null;

                foreach (var existingFile in existingPacks) {
                    if (existingFile.ToLowerInvariant().Contains(basePackName)) {
                        associatedLocalPack = existingFile;
                        break;
                    }
                }

                if (associatedLocalPack != null) {
                    pack.CurrentDownloadDate = File.GetLastWriteTimeUtc(associatedLocalPack);

                    if (pack.CurrentDownloadDate != default && pack.LastUpdate > pack.CurrentDownloadDate) {
                        PackHandlingUtil.DownloadPack(pack, OnUpdateComplete);
                    }
                }
            }
        }

        private static void OnUpdateComplete(MarkerPackPkg markerPackPkg, bool success) {
            markerPackPkg.IsDownloading = false;

            if (success) {
                markerPackPkg.CurrentDownloadDate = DateTime.UtcNow;
            }
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

    }
}
