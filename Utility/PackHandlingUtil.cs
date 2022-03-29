using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.MarkerPackRepo;
using Blish_HUD;
using Flurl.Http;

namespace BhModule.Community.Pathing.Utility {
    public static class PackHandlingUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(PackHandlingUtil));
        
        // At least one pack will deny us from downloading it without a more typical UA set.
        private const string DOWNLOAD_UA = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

        public static void DownloadPack(MarkerPackPkg markerPackPkg, Action<MarkerPackPkg, bool> funcOnComplete) {
            var beginThread = new Thread(async () => await BeginPackDownload(markerPackPkg, PathingModule.Instance.GetModuleProgressHandler(), funcOnComplete)) { IsBackground = true };
            beginThread.Start();
        }

        private static async Task BeginPackDownload(MarkerPackPkg markerPackPkg, IProgress<string> progress, Action<MarkerPackPkg, bool> funcOnComplete) {
            // TODO: Localize 'Updating pack '{0}'...'
            Logger.Info($"Updating pack '{markerPackPkg.Name}'...");
            progress.Report($"Updating pack '{markerPackPkg.Name}'...");
            markerPackPkg.IsDownloading = true;

            string tempPackDownloadDestination = Path.GetTempFileName();

            try {
                // This is stupid, but GetTempFileName actually generates a file for us which causes DownloadFileAsync to fail since the file already exists.  🤦‍
                File.Delete(tempPackDownloadDestination);

                await markerPackPkg.Download.WithHeader("user-agent", DOWNLOAD_UA).DownloadFileAsync(Path.GetDirectoryName(tempPackDownloadDestination), Path.GetFileName(tempPackDownloadDestination));
            } catch (Exception ex) {
                Logger.Error(ex, $"Failed to update marker pack {markerPackPkg.Name} from {markerPackPkg.Download} to {tempPackDownloadDestination}.");
                funcOnComplete(markerPackPkg, false);
                return;
            }

            // TODO: Localize 'Finalizing new pack download...'
            progress.Report("Finalizing new pack download...");

            string finalPath = Path.Combine(DataDirUtil.MarkerDir, markerPackPkg.FileName);

            try {
                bool needsInit = true;

                if (File.Exists(finalPath)) {
                    // The pack was already downloaded - make sure we're not currently loading!
                    needsInit = false;
                    while (PathingModule.Instance.PackInitiator.IsLoading) {
                        // We're currently loading the packs.  Wait a second and check again.
                        Thread.Sleep(1000);
                    }

                    File.Delete(finalPath);
                }

                File.Move(tempPackDownloadDestination, finalPath);

                if (needsInit) {
                    await PathingModule.Instance.PackInitiator.LoadPackedPackFiles(new[] { finalPath });
                }
            } catch (Exception ex) {
                Logger.Warn(ex, $"Failed moving marker pack {markerPackPkg.Name} from {tempPackDownloadDestination} to {finalPath}.");
                funcOnComplete(markerPackPkg, false);
                return;
            }

            progress.Report(string.Empty);
            funcOnComplete(markerPackPkg, true);
        }

    }
}
