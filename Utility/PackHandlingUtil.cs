using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.UI.Presenter;
using Blish_HUD;
using Flurl.Http;

namespace BhModule.Community.Pathing.Utility {
    public static class PackHandlingUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(PackHandlingUtil));
            
        // At least one pack will deny us from downloading it without a more typical UA set.
        private const string DOWNLOAD_UA = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

        public static void DownloadPack(PackRepoPresenter.MarkerPackPkg markerPackPkg) {
            var beginThread = new Thread(async () => await BeginPackDownload(markerPackPkg, PathingModule.Instance.GetModuleProgressHandler())) { IsBackground = true };
            beginThread.Start();
        }

        private static async Task BeginPackDownload(PackRepoPresenter.MarkerPackPkg markerPackPkg, IProgress<string> progress) {
            // TODO: Localize 'Downlaoding pack '{0}'...'
            progress.Report($"Downloading pack '{markerPackPkg.Name}'...");

            string tempPackDownloadDestination = Path.GetTempFileName();

            try {
                // This is stupid, but GetTempFileName actually generates a file for us which causes DownloadFileAsync to fail since the file already exists.  🤦‍
                File.Delete(tempPackDownloadDestination);

                await markerPackPkg.Download.WithHeader("user-agent", DOWNLOAD_UA).DownloadFileAsync(Path.GetDirectoryName(tempPackDownloadDestination), Path.GetFileName(tempPackDownloadDestination));
            } catch (Exception ex) {
                Logger.Error(ex, $"Failed to download marker pack {markerPackPkg.Name} from {markerPackPkg.Download} to {tempPackDownloadDestination}.");
                return;
            }

            // TODO: Localize 'Finalizing new pack download...'
            progress.Report("Finalizing new pack download...");

            string finalPath = Path.Combine(DataDirUtil.MarkerDir, markerPackPkg.FileName);

            try {
                // TODO: We should actually check the data readers and make sure they're unlocked but...probably won't be an issue most of the time... 😅
                if (File.Exists(finalPath)) {
                    File.Delete(finalPath);
                }

                File.Move(tempPackDownloadDestination, finalPath);
            } catch (Exception ex) {
                Logger.Error(ex, $"Failed moving marker pack {markerPackPkg.Name} from {tempPackDownloadDestination} to {finalPath}.");
                return;
            }

            progress.Report(string.Empty);
        }

    }
}
