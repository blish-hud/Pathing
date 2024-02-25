using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Settings;
using Flurl.Http;

namespace BhModule.Community.Pathing.MarkerPackRepo {
    public class MarkerPackRepo {

        private static readonly Logger Logger = Logger.GetLogger<MarkerPackRepo>();

        private const string REPO_SETTING    = "PackRepoUrl";
        private const string PUBLIC_REPOURL  = "https://mp-repo.blishhud.com/repo.json";
        private const string MPREPO_SETTINGS = "MarkerRepoSettings";

        private string _repoUrl;

        private readonly PathingModule _module;

        private SettingCollection _markerPackSettings;

        public MarkerPackPkg[] MarkerPackages { get; private set; } = Array.Empty<MarkerPackPkg>();

        public MarkerPackRepo(PathingModule module) {
            _module = module;
        }

        public void Init() {
            var beginThread = new Thread(async () => {
                // Wait 5 seconds before we start downloading marker packs.
                await Task.Delay(5000);

                // Only update marker packs if we are currently loaded.
                if (_module.RunState != Blish_HUD.Modules.ModuleRunState.Unloading && _module.RunState != Blish_HUD.Modules.ModuleRunState.Unloaded) {
                    await Load(_module.GetModuleProgressHandler());
                }
            }) { IsBackground = true };
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
            _repoUrl            = _module.SettingsManager.ModuleSettings.DefineSetting(REPO_SETTING, PUBLIC_REPOURL).Value;
            _markerPackSettings = _module.SettingsManager.ModuleSettings.AddSubCollection(MPREPO_SETTINGS);
        }

        private void LoadLocalPackInfo() {
            string directory = _module.DirectoriesManager.GetFullDirectoryPath("markers");

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

                    if (pack.AutoUpdate.Value && pack.CurrentDownloadDate != default && pack.LastUpdate > pack.CurrentDownloadDate) {
                        PackHandlingUtil.DownloadPack(_module, pack, OnUpdateComplete, true);
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

            foreach (var pack in releases) {
                pack.AutoUpdate = _markerPackSettings.DefineSetting(pack.Name + "_AutoUpdate", true);
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
