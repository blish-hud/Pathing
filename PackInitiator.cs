using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using TmfLib;
using TmfLib.Diagnostics;

namespace BhModule.Community.Pathing {
    public class PackInitiator : IUpdatable {

        private static readonly Logger Logger = Logger.GetLogger<PackInitiator>();

        private readonly string            _watchPath;
        private readonly PathingModule     _module;
        private readonly IProgress<string> _loadingIndicator;

        private readonly IRootPackState _packState;

        private readonly SafeList<PackWrapper> _packs = new();

        /// <summary>
        /// The packs that are currently loaded (indexed) along with their merged categories.
        /// </summary>
        private readonly PackSet _packSet = new();

        private SettingCollection _packToggleSettings;

        private CancellationTokenSource _mapLoadCancellation;

        public bool IsLoading { get; private set; }

        public IRootPackState PackState => _packState;

        private int _lastMap = -1;

        public event EventHandler<EventArgs> LoadMapFromEachPackStarted;
        public event EventHandler<EventArgs> LoadMapFromEachPackFinished;

        public PackInitiator(string watchPath, PathingModule module, IProgress<string> loadingIndicator) {
            _watchPath        = watchPath;
            _module           = module;
            _loadingIndicator = loadingIndicator;

            _packToggleSettings = _module.SettingsManager.ModuleSettings.AddSubCollection("PackToggleSettings");

            _packState = new SharedPackState(_module);
        }

        public void ReloadPacks() {
            if (_packState.CurrentMapId < 0 || this.IsLoading) return;

            _lastMap = -1;

            LoadMapFromEachPackInBackground(_packState.CurrentMapId);
        }

        private IEnumerable<ContextMenuStripItem> GetPackLoadSettings(PackWrapper pack) {
            // Always Load
            yield return new SimpleContextMenuStripItem("Always Load", (e) => pack.AlwaysLoad.Value = e, pack.AlwaysLoad.Value);

            if (pack.IsLoaded) {
                yield return new ContextMenuStripItem() {
                    Text = $"Loaded in {pack.LoadTime} milliseconds",
                    BasicTooltipText = $"Categories: {pack.Pack.IndexDuration.TotalMilliseconds:F0} ms\nCurrent map: {pack.MapLoadTime} ms",
                    Enabled = false
                };

                yield return new SimpleContextMenuStripItem("Reload Marker Pack", () => ReloadPack(pack)) {
                    Enabled = !this.IsLoading
                };

                yield return new SimpleContextMenuStripItem("Unload Marker Pack", () => UnloadPack(pack)) {
                    Enabled = !this.IsLoading
                };
            } else {
                yield return new SimpleContextMenuStripItem("Load Marker Pack", () => {
                    pack.ForceLoad = true;
                    ReloadPack(pack);
                });
            }

            // Problems
            /*
            var diagnostics = pack.Pack.Diagnostics;
            if (diagnostics.Count > 0) {
                var problems = diagnostics.Snapshot();
                int errors   = problems.Count(d => d.Severity == DiagnosticSeverity.Error);
                int warnings = problems.Count(d => d.Severity == DiagnosticSeverity.Warning);

                yield return new SimpleContextMenuStripItem($"Copy Problem Report ({errors} errors, {warnings} warnings)", () => {
                    ClipboardHelper.CopyWithNotification(GetProblemReport(pack), "Problem report copied to clipboard.");
                }) {
                    BasicTooltipText = string.Join("\n", problems.Where(d => d.Severity >= DiagnosticSeverity.Warning).Take(10).Select(d => d.ToString()))
                };
            }
            */

            // Delete Pack
            yield return new SimpleContextMenuStripItem("Delete Pack", () => {
                if (pack.Package != null) {
                    Utility.PackHandlingUtil.DeletePack(_module, pack.Package);
                }
            }) {
                Enabled = pack.Package != null
            };
        }

        private static string GetProblemReport(PackWrapper pack) {
            return $"{pack.Pack.Name} ({pack.Pack.Format}){Environment.NewLine}{pack.Pack.Source.Location}{Environment.NewLine}{Environment.NewLine}{pack.Pack.Diagnostics.ToReport()}";
        }

        private IEnumerable<ContextMenuStripItem> GetPackToggles() {
            var packs = _packs.ToArray();

            foreach (var pack in packs.OrderBy(p => p.Package?.Name ?? p.Pack.Name)) {
                if (pack.Pack.Name == "markers") {
                    // Skip unpacked.
                    continue;
                }

                string packName = pack.Package?.Name ?? pack.Pack.Name;

                yield return new ContextMenuStripItem() {
                    Text = packName,
                    Submenu = new ContextMenuStrip(() => GetPackLoadSettings(pack))
                };
            }

            yield return new ContextMenuStripDivider();

            // Reload Markers
            yield return new SimpleContextMenuStripItem("Reload Marker Packs", ReloadAllPacks) {
                Enabled = !this.IsLoading && _packState.CurrentMapId > 0,
                BasicTooltipText = _module.Settings.KeyBindReloadMarkerPacks.Value.PrimaryKey != Microsoft.Xna.Framework.Input.Keys.None ? $"Keybind: {_module.Settings.KeyBindReloadMarkerPacks.Value.GetBindingDisplayText()}" : null
            };

            // Unload Markers
            yield return new SimpleContextMenuStripItem("Unload Marker Packs", async () => {
                if (_packState.CurrentMapId < 0) return;
                await UnloadStateAndCollection();
            }) {
                Enabled = !this.IsLoading && _packState.CurrentMapId > 0
            };

            // Download Marker Packs
            yield return new SimpleContextMenuStripItem("Download Marker Packs", () => {
                _module.SettingsWindow.SelectedTab = _module.MarkerRepoTab;
                _module.SettingsWindow.Show();
            });
        }

        public PathingCategory GetAllMarkersCategories() {
            return _packState.RootCategory;
        }

        public IEnumerable<ContextMenuStripItem> GetPackMenuItems() {
            var rootCategory = _packState.RootCategory;

            // All Markers
            bool isAnyMarkers = !this.IsLoading
                             && rootCategory != null
                             && rootCategory.Any(category => !string.IsNullOrWhiteSpace(category.DisplayName));

            var allMarkers = new ContextMenuStripItem() {
                Text = "All Markers", // TODO: Localize "All Markers"
                CanCheck = true,
                Checked = _module.Settings.GlobalPathablesEnabled.Value,
                Submenu = isAnyMarkers
                    ? new CategoryContextMenuStrip(_packState, rootCategory, false)
                    : null
            };

            allMarkers.CheckedChanged += (_, e) => {
                _module.Settings.GlobalPathablesEnabled.Value = e.Checked;
            };

            // Manage Packs
            var packs = new ContextMenuStripItem() {
                Text = "Manage Marker Packs",
                Submenu = new ContextMenuStrip(GetPackToggles)
            };

            // Scripts
            if (_module.Settings.ScriptsEnabled.Value && _module.ScriptEngine.Global != null && _module.ScriptEngine.Global.Menu.Menus.Any()) {
                yield return _module.ScriptEngine.Global.Menu.BuildMenu();
            }

            yield return allMarkers;
            yield return packs;
        }

        public async Task Init() {
            await _packState.Load();
            await LoadAllPacks();
        }

        private PackWrapper GetPackWrapper(Pack pack) {
            var alwaysLoad = _packToggleSettings.DefineSetting($"{pack.Name}_AlwaysLoad", true);
            return new PackWrapper(_module, pack, alwaysLoad);
        }

        public async Task LoadUnpackedPackFiles(string unpackedDir) {
            try {
                var newPack = Pack.FromDirectory(unpackedDir);
                _packs.Add(GetPackWrapper(newPack));
            } catch (Exception ex) {
                Logger.Warn(ex, $"Unpacked markers failed to load.");
            }

            await Task.CompletedTask;
        }

        public async Task LoadPackedPackFiles(IEnumerable<string> zipPackFiles) {
            foreach (string packArchive in zipPackFiles) {
                try {
                    var newPack = Pack.FromArchive(packArchive);
                    _packs.Add(GetPackWrapper(newPack));
                } catch (InvalidDataException) {
                    Logger.Warn($"Pack {packArchive} appears to be corrupt.  Please remove it and download it again.");
                } catch (Exception ex) {
                    Logger.Warn(ex, $"Pack {packArchive} failed to load.");
                }
            }

            await Task.CompletedTask;
        }

        public async Task LoadPack(Pack pack) {
            if (pack != null) {
                var wrapper = GetPackWrapper(pack);
                _packs.Add(wrapper);

                if (ShouldLoad(wrapper)) {
                    await Task.Run(() => IndexPacks(new[] { wrapper }));
                }
            }
        }

        public void UnloadPackByName(string packName) {
            foreach (var pack in _packs.ToArray()) {
                if (string.Equals(packName, pack.Pack.Name, StringComparison.OrdinalIgnoreCase)) {
                    _packs.Remove(pack);
                    RemoveFromSet(pack);
                    break;
                }
            }
        }

        private static bool ShouldLoad(PackWrapper pack) => pack.AlwaysLoad.Value || pack.ForceLoad;

        /// <summary>
        /// Reads the categories of packs and adds them to the loaded set.
        /// </summary>
        private void IndexPacks(IReadOnlyList<PackWrapper> packs) {
            if (packs.Count == 0) return;

            var timer = Stopwatch.StartNew();

            _packSet.AddRange(packs.Select(p => p.Pack));

            foreach (var pack in packs) {
                if (pack.Pack.State == TmfLib.PackState.Failed) {
                    Logger.Warn(pack.Pack.Error, $"Pack '{pack.Pack.Name}' could not be loaded.");
                    _packSet.Remove(pack.Pack);
                } else if (pack.Pack.Diagnostics.HasErrors) {
                    Logger.Warn($"Pack '{pack.Pack.Name}' loaded with problems:{Environment.NewLine}{pack.Pack.Diagnostics.ToReport()}");
                }
            }

            _packState.CategoryStates?.Invalidate();

            Logger.Info($"Indexed {packs.Count} pack(s) in {timer.ElapsedMilliseconds} ms: {string.Join(", ", packs.Select(p => $"{p.Pack.Name}[{p.Pack.IndexDuration.TotalMilliseconds:F0}ms]"))}");
        }

        private void RemoveFromSet(PackWrapper pack) {
            pack.IsLoaded = false;

            if (_packSet.Remove(pack.Pack)) {
                _ = TextureResourceManager.ReleaseAsync(PackResourceManager.For(pack.Pack));
            }
        }

        private void ReloadPack(PackWrapper pack) {
            if (this.IsLoading) return;

            Task.Run(() => {
                if (_packSet.Packs.Contains(pack.Pack)) {
                    _packSet.Reload(pack.Pack);
                } else {
                    IndexPacks(new[] { pack });
                }

                ReloadPacks();
            });
        }

        private void UnloadPack(PackWrapper pack) {
            if (this.IsLoading) return;

            pack.ForceLoad = false;
            RemoveFromSet(pack);
            ReloadPacks();
        }

        /// <summary>
        /// Re-reads every pack from disk.
        /// </summary>
        private void ReloadAllPacks() {
            if (this.IsLoading) return;

            Task.Run(() => {
                foreach (var pack in _packSet.Packs) {
                    pack.LoadIndex();
                }

                _packSet.Refresh();
                ReloadPacks();
            });
        }

        private async Task UnloadStateAndCollection() {
            await _packState.Unload();
        }

        private async Task LoadAllPacks() {
            // Load from base and advanced markers paths
            foreach (string markerDir in (_packState.UserResourceStates.Advanced.MarkerLoadPaths ?? Array.Empty<string>()).Concat(new [] {_watchPath})) {
                await LoadPackedPackFiles(Directory.GetFiles(markerDir, "*.zip",  SearchOption.AllDirectories));
                await LoadPackedPackFiles(Directory.GetFiles(markerDir, "*.taco", SearchOption.AllDirectories));
                await LoadUnpackedPackFiles(markerDir);
            }

            _loadingIndicator.Report("Loading marker categories...");
            await Task.Run(() => IndexPacks(_packs.ToArray().Where(ShouldLoad).ToArray()));
            _loadingIndicator.Report(null);

            // If the module loads at launch, this can end up firing twice.
            // If the module loads after launch (manually enabled), we need this to populate the current map.
            if (GameService.Gw2Mumble.CurrentMap.Id != default) {
                LoadMapFromEachPackInBackground(_packState.CurrentMapId = GameService.Gw2Mumble.CurrentMap.Id);
            }

            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
        }

        private void LoadMapFromEachPackInBackground(int mapId) {
            lock (this) {
                if (mapId == _lastMap) {
                    return;
                };
                _lastMap = mapId;
            }

            Task.Run(() => LoadMapFromEachPack(mapId));
        }

        private async Task LoadMapFromEachPack(int mapId) {
            this.IsLoading = true;
            this.LoadMapFromEachPackStarted?.Invoke(this, EventArgs.Empty);

            var loadTimer = Stopwatch.StartNew();

            _packState.Module.ScriptEngine.Reset();

            // TODO: Localize the loading messages.
            _loadingIndicator.Report("Loading marker packs...");

            await _packState.UnloadEntities();

            var packs = _packs.ToArray();

            // Packs enabled since the last load (e.g. "Always Load" toggled) need their categories first.
            IndexPacks(packs.Where(p => ShouldLoad(p) && p.Pack.State != TmfLib.PackState.Indexed && p.Pack.State != TmfLib.PackState.Failed).ToArray());

            foreach (var pack in packs.Where(p => !ShouldLoad(p) && _packSet.Packs.Contains(p.Pack))) {
                RemoveFromSet(pack);
            }

            var loaded = new HashSet<Pack>(_packSet.Packs);

            var mapTimer = Stopwatch.StartNew();

            _mapLoadCancellation?.Cancel();
            _mapLoadCancellation = new CancellationTokenSource();

            IReadOnlyList<TmfLib.Model.MapSlice> slices;

            try {
                slices = await _packSet.LoadMapAsync(mapId, _mapLoadCancellation.Token);
            } catch (Exception e) {
                Logger.Warn(e, $"Loading map {mapId} from marker packs failed.");
                slices = Array.Empty<TmfLib.Model.MapSlice>();
            }

            long mapMs = mapTimer.ElapsedMilliseconds;

            foreach (var pack in packs) {
                pack.IsLoaded    = loaded.Contains(pack.Pack);
                pack.MapLoadTime = slices.FirstOrDefault(s => s.Pack == pack.Pack) != null ? mapMs : 0;
                pack.LoadTime    = (long)pack.Pack.IndexDuration.TotalMilliseconds + pack.MapLoadTime;
            }

            _loadingIndicator.Report("Finalizing marker collection...");

            try {
                await _packState.LoadMapSlices(_packSet.Categories.Root, slices);
            } catch (Exception e) {
                Logger.Warn(e, $"Finalizing packs failed.");
                await _packState.Unload();
            }

            long entityMs = mapTimer.ElapsedMilliseconds - mapMs;

            // We only load scripts if they're enabled.
            if (_packState.Module.Settings.ScriptsEnabled.Value) {
                _loadingIndicator.Report("Loading scripts...");

                foreach (var pack in packs) {
                    if (!pack.IsLoaded) {
                        // Skip loading.
                        continue;
                    }

                    var scriptTimer = Stopwatch.StartNew();
                    await _packState.Module.ScriptEngine.LoadScript("pack.lua", PackResourceManager.For(pack.Pack), pack.Pack.Name);
                    pack.LoadTime += scriptTimer.ElapsedMilliseconds;
                }
            }

            foreach (var pack in packs) {
                pack.Pack.Source.ReleaseLocks();
            }

            _loadingIndicator.Report(null);

            this.IsLoading = false;

            this.LoadMapFromEachPackFinished?.Invoke(this, EventArgs.Empty);

            Logger.Info($"Finished loading {slices.Count} packs ({slices.Sum(s => s.Count)} pathables) in {loadTimer.ElapsedMilliseconds}ms for map {mapId} (read {mapMs}ms, entities {entityMs}ms).");
        }

        private void OnMapChanged(object sender, ValueEventArgs<int> e) {
            if (e.Value == _packState.CurrentMapId) return;

            _packState.CurrentMapId = e.Value;

            foreach (var pack in _packs) {
                // Reset the force load after map change.
                pack.ForceLoad = false;
            }

            LoadMapFromEachPackInBackground(e.Value);
        }

        public void Update(GameTime gameTime) {
            _packState.Update(gameTime);
        }

        public void Unload() {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChanged;

            _mapLoadCancellation?.Cancel();

            _packState.Unload();

            foreach (var pack in _packs) {
                pack.Pack.Source.Dispose();
            }
        }

    }
}
