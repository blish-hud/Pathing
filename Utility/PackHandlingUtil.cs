using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.MarkerPackRepo;
using Blish_HUD;
using TmfLib;
using TmfLib.Writer;

namespace BhModule.Community.Pathing.Utility {
    public static class PackHandlingUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(PackHandlingUtil));
        
        // At least one pack will deny us from downloading it without a more typical UA set.
        private const string DOWNLOAD_UA = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";

        public static void DownloadPack(PathingModule module, MarkerPackPkg markerPackPkg, Action<MarkerPackPkg, bool> funcOnComplete, bool skipReload = false) {
            var beginThread = new Thread(async () => await BeginPackDownload(module, markerPackPkg, module.GetModuleProgressHandler(), funcOnComplete, skipReload)) { IsBackground = true };
            beginThread.Start();
        }

        public static void DeletePack(PathingModule module, MarkerPackPkg markerPackPkg) {
            markerPackPkg.IsDownloading = true;
            markerPackPkg.DownloadError = null;

            string mpPath = Path.Combine(DataDirUtil.MarkerDir, markerPackPkg.FileName);

            try {
                if (File.Exists(mpPath)) {
                    while (module.PackInitiator.IsLoading) {
                        // We're currently loading the packs.  Wait a second and check again.
                        Thread.Sleep(1000);
                    }

                    File.Delete(mpPath);
                    Logger.Info("Deleted marker pack '{packPath}'.", mpPath);
                } else {
                    Logger.Warn("Attempted to delete pack '{packPath}' that doesn't exist.", mpPath);
                }

                module.PackInitiator.UnloadPackByName(Path.GetFileNameWithoutExtension(markerPackPkg.FileName));

                module.PackInitiator.ReloadPacks();

                markerPackPkg.CurrentDownloadDate = default;
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to delete marker pack '{packPath}'", mpPath);
                markerPackPkg.DownloadError = "Failed to delete marker pack.";
            }

            markerPackPkg.IsDownloading = false;
        }

        private static async Task<string> OptimizePack(string downloadedPack) {
            string dir  = Path.GetDirectoryName(downloadedPack);
            string file = $"optimized-{Path.GetFileName(downloadedPack)}";

            try {
                var pack = Pack.FromArchivedMarkerPack(downloadedPack);

                if (pack.ManifestedPack) {
                    // Pack is already optimized.  We can skip doing it again.
                    pack.ReleaseLocks();
                    return downloadedPack;
                }

                var packCollection = await pack.LoadAllAsync();

                var packWriter = new PackWriter(new PackWriterSettings() {
                                                    PackOutputMethod = PackWriterSettings.OutputMethod.Archive
                                                });

                await packWriter.WriteAsync(pack, packCollection, dir, file);

                pack.ReleaseLocks();
            } catch (Exception e) {
                // Optimization failed - lets not interrupt the process.
                Logger.Error(e, "Failed to optimize marker pack.");
                return downloadedPack;
            }

            File.Delete(downloadedPack);

            return Path.Combine(dir, file);
        }

        private static async Task BeginPackDownload(PathingModule module, MarkerPackPkg markerPackPkg, IProgress<string> progress, Action<MarkerPackPkg, bool> funcOnComplete, bool skipReload = false) {
            // TODO: Localize 'Downloading pack '{0}'...'
            Logger.Info($"Downloading pack '{markerPackPkg.Name}'...");
            progress.Report($"Downloading pack '{markerPackPkg.Name}'...");
            markerPackPkg.IsDownloading    = true;
            markerPackPkg.DownloadError    = null;
            markerPackPkg.DownloadProgress = 0;

            string finalPath                   = Path.Combine(DataDirUtil.MarkerDir, markerPackPkg.FileName);
            string tempPackDownloadDestination = Path.Combine(Path.GetTempPath(),    Guid.NewGuid().ToString());

            try {
                using (var webClient = new WebClient()) {
                    webClient.Headers.Add("user-agent", DOWNLOAD_UA);
                    webClient.DownloadProgressChanged += (s, e) => { markerPackPkg.DownloadProgress = e.ProgressPercentage; };
                    await webClient.DownloadFileTaskAsync(markerPackPkg.Download, tempPackDownloadDestination);
                }
            } catch (Exception ex) {
                markerPackPkg.DownloadError = "Marker pack download failed.";
                if (ex is WebException we) {
                    Logger.Warn(ex, $"Failed to download marker pack {markerPackPkg.Name} from {markerPackPkg.Download} to {tempPackDownloadDestination}.");
                } else {
                    Logger.Error(ex, $"Failed to download marker pack {markerPackPkg.Name} from {markerPackPkg.Download} to {tempPackDownloadDestination}.");
                }
                progress.Report(null);
                funcOnComplete(markerPackPkg, false);
                return;
            }

            // TODO: Localize 'Finalizing new pack download...'
            progress.Report("Finalizing new pack download...");

            try {
                bool needsInit = true;

                // Test to ensure that the pack is valid.
                using (var packStream = File.Open(tempPackDownloadDestination, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (var packArchive = new ZipArchive(packStream)) {
                        // Ensure we can see the list of entries, at least.
                        if (!(packArchive.Entries.Count > 0)) {
                            throw new InvalidDataException();
                        }
                    }
                }

                if (module.RunState == Blish_HUD.Modules.ModuleRunState.Loaded && module.PackInitiator.PackState.UserResourceStates.Advanced.OptimizeMarkerPacks) {
                    // TODO: Localize 'Optimizing the pack...'
                    progress.Report("Optimizing the pack...");
                    tempPackDownloadDestination = await OptimizePack(tempPackDownloadDestination);
                } else {
                    Logger.Info("Skipping pack optimization - it's disabled or instance is null.");
                }

                if (File.Exists(finalPath)) {
                    // The pack was already downloaded - make sure we're not currently loading!
                    needsInit = false;

                    while (module.PackInitiator.IsLoading) {
                        // We're currently loading the packs.  Wait a second and check again.
                        await Task.Delay(1000);
                    }

                    for (int i = 5; i>= 0; i--) {
                        try {
                            File.Delete(finalPath);
                            break;
                        } catch (IOException) when (i > 0) {
                            // File is probably temporarily locked.
                            await Task.Delay(1500);
                        }
                    }

                }

                try {
                    File.Move(tempPackDownloadDestination, finalPath);
                } catch (IOException) {
                    Logger.Warn("Failed to move temp marker pack from {startPath} to {endPath} so instead we'll attempt to copy it.", tempPackDownloadDestination, finalPath);
                    File.Copy(tempPackDownloadDestination, finalPath);
                }

                var newPack = Pack.FromArchivedMarkerPack(finalPath);

                if (!needsInit) {
                    module.PackInitiator.UnloadPackByName(newPack.Name);
                }

                await module.PackInitiator.LoadPack(newPack);
                newPack.ReleaseLocks();

                if (!skipReload) {
                    // We skip refreshing when mass-updating packs.
                    module.PackInitiator.ReloadPacks();
                }
            } catch (InvalidDataException ex) {
                markerPackPkg.DownloadError = "Marker pack download is corrupt.";
                Logger.Warn(ex, $"Failed downloading marker pack {markerPackPkg.Name} from {tempPackDownloadDestination} (it appears to be corrupt).");
            } catch (Exception ex) {
                markerPackPkg.DownloadError = "Failed to import the new marker pack.";
                Logger.Warn(ex, $"Failed moving marker pack {markerPackPkg.Name} from {tempPackDownloadDestination} to {finalPath}.");
            }

            progress.Report(null);
            funcOnComplete(markerPackPkg, markerPackPkg.DownloadError == null);
        }

    }
}
