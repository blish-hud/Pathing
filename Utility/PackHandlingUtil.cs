using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.MarkerPackRepo;
using Blish_HUD;

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
            progress.Report($"Downloading pack '{markerPackPkg.Name}'...");
            markerPackPkg.IsDownloading = true;
            markerPackPkg.DownloadError = null;

            string tempPackDownloadDestination = Path.GetTempFileName();

            try {
                // This is stupid, but GetTempFileName actually generates a file for us which causes DownloadFileAsync to fail since the file already exists.
                File.Delete(tempPackDownloadDestination);

                using (var webClient = new WebClient()) {
                    webClient.Headers.Add("user-agent", DOWNLOAD_UA);
                    webClient.DownloadProgressChanged += (s, e) => {
                        markerPackPkg.DownloadProgress = e.ProgressPercentage;
                    };
                    await webClient.DownloadFileTaskAsync(markerPackPkg.Download, tempPackDownloadDestination);
                }
            } catch (Exception ex) {
                markerPackPkg.DownloadError = "Marker pack download failed.";
                Logger.Error(ex, $"Failed to download marker pack {markerPackPkg.Name} from {markerPackPkg.Download} to {tempPackDownloadDestination}.");
                funcOnComplete(markerPackPkg, false);
                return;
            }

            // TODO: Localize 'Finalizing new pack download...'
            progress.Report("Finalizing new pack download...");

            string finalPath = Path.Combine(DataDirUtil.MarkerDir, markerPackPkg.FileName);

            try {
                bool needsInit = true;

                // Test to ensure that the pack is valid.
                using (var packStream = File.Open(tempPackDownloadDestination, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    var packArchive = new ZipArchive(packStream);

                    // Ensure we can see the list of entries, at least.
                    if (!(packArchive.Entries.Count > 0)) {
                        throw new InvalidDataException();
                    }
                }

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
                    await PathingModule.Instance.PackInitiator.LoadPackedPackFiles(
                                                                                   new[] {
                                                                                       finalPath
                                                                                   }
                                                                                  );
                }
            } catch (InvalidDataException ex) {
                markerPackPkg.DownloadError = "Marker pack download is corrupt.";
                Logger.Warn(ex, $"Failed downloading marker pack {markerPackPkg.Name} from {tempPackDownloadDestination} (it appears to be corrupt).");
            } catch (Exception ex) {
                markerPackPkg.DownloadError = "Failed to import the new marker pack.";
                Logger.Warn(ex, $"Failed moving marker pack {markerPackPkg.Name} from {tempPackDownloadDestination} to {finalPath}.");
            }

            progress.Report(string.Empty);
            funcOnComplete(markerPackPkg, markerPackPkg.DownloadError == null);
        }

    }
}
