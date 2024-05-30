using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Behavior.Modifier;
using BhModule.Community.Pathing.MarkerPackRepo;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;
using TmfLib;
using TmfLib.Reader;

namespace BhModule.Community.Pathing {
    public class PackInitiator : IUpdatable {

        private static readonly Logger Logger = Logger.GetLogger<PackInitiator>();

        private readonly string            _watchPath;
        private readonly PathingModule     _module;
        private readonly IProgress<string> _loadingIndicator;

        private readonly IRootPackState _packState;

        private readonly SafeList<PackWrapper> _packs = new();

        private SharedPackCollection _sharedPackCollection;
        
        private readonly PackReaderSettings _packReaderSettings;

        private SettingCollection _packToggleSettings;

        public bool IsLoading { get; private set; }

        public IRootPackState PackState => _packState;

        private int _lastMap = -1;

        public PackInitiator(string watchPath, PathingModule module, IProgress<string> loadingIndicator) {
            _watchPath        = watchPath;
            _module           = module;
            _loadingIndicator = loadingIndicator;

            _packToggleSettings = _module.SettingsManager.ModuleSettings.AddSubCollection("PackToggleSettings");

            _packReaderSettings = new PackReaderSettings();
            _packReaderSettings.VenderPrefixes.Add("bh-"); // Support Blish HUD specific categories/markers/trails/attributes.

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

            // Load Manually
            if (pack.IsLoaded) {
                yield return new ContextMenuStripItem() {
                    Text = $"Loaded in {pack.LoadTime} milliseconds",
                    Enabled = false
                };
            } else {
                yield return new SimpleContextMenuStripItem("Load Marker Pack", () => {
                    pack.ForceLoad = true;
                    ReloadPacks();
                });
            }

            // Delete Pack
            yield return new SimpleContextMenuStripItem("Delete Pack", () => {
                if (pack.Package != null) {
                    Utility.PackHandlingUtil.DeletePack(_module, pack.Package);
                }
            }) {
                Enabled = pack.Package != null
            };
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
            yield return new SimpleContextMenuStripItem("Reload Marker Packs", ReloadPacks) {
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
            }); ;
        }

        public IEnumerable<ContextMenuStripItem> GetPackMenuItems() {
            // All Markers
            bool isAnyMarkers = !this.IsLoading
                             && _sharedPackCollection != null
                             && _sharedPackCollection.Categories != null
                             && _sharedPackCollection.Categories.Any(category => !string.IsNullOrWhiteSpace(category.DisplayName));

            var allMarkers = new ContextMenuStripItem() {
                Text = "All Markers", // TODO: Localize "All Markers"
                CanCheck = true,
                Checked = _module.Settings.GlobalPathablesEnabled.Value,
                Submenu = isAnyMarkers
                    ? new CategoryContextMenuStrip(_packState, _sharedPackCollection.Categories, false)
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
                var newPack = Pack.FromDirectoryMarkerPack(unpackedDir);
                _packs.Add(GetPackWrapper(newPack));
            } catch (Exception ex) {
                Logger.Warn(ex, $"Unpacked markers failed to load.");
            }
        }

        public async Task LoadPackedPackFiles(IEnumerable<string> zipPackFiles) {
            foreach (string packArchive in zipPackFiles) {
                try {
                    var newPack = Pack.FromArchivedMarkerPack(packArchive);
                    _packs.Add(GetPackWrapper(newPack));
                } catch (InvalidDataException) {
                    Logger.Warn($"Pack {packArchive} appears to be corrupt.  Please remove it and download it again.");
                } catch (Exception ex) {
                    Logger.Warn(ex, $"Pack {packArchive} failed to load.");
                }
            }
        }

        public async Task LoadPack(Pack pack) {
            if (pack != null) {
                _packs.Add(GetPackWrapper(pack));
            }
        }

        public void UnloadPackByName(string packName) {
            foreach (var pack in _packs.ToArray()) {
                if (string.Equals(packName, pack.Pack.Name, StringComparison.OrdinalIgnoreCase)) {
                    _packs.Remove(pack);
                    break;
                }
            }
        }

        private async Task UnloadStateAndCollection() {
            _sharedPackCollection?.Unload();
            await _packState.Unload();
        }

        private async Task LoadAllPacks() {
            // Load from base and advanced markers paths
            foreach (string markerDir in (_packState.UserResourceStates.Advanced.MarkerLoadPaths ?? Array.Empty<string>()).Concat(new [] {_watchPath})) {
                await LoadPackedPackFiles(Directory.GetFiles(markerDir, "*.zip",  SearchOption.AllDirectories));
                await LoadPackedPackFiles(Directory.GetFiles(markerDir, "*.taco", SearchOption.AllDirectories));
                await LoadUnpackedPackFiles(markerDir);
            }

            // If the module loads at launch, this can end up firing twice.
            // If the module loads after launch (manually enabled), we need this to populate the current map.
            if (GameService.Gw2Mumble.CurrentMap.Id != default) {
                LoadMapFromEachPackInBackground(_packState.CurrentMapId = GameService.Gw2Mumble.CurrentMap.Id);
            }

            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
        }

        private async Task PrepareState(int mapId) {
            await UnloadStateAndCollection();

            _sharedPackCollection = new SharedPackCollection();
        }

        private void LoadMapFromEachPackInBackground(int mapId) {
            lock (this) {
                if (mapId == _lastMap) {
                    return;
                };
                _lastMap = mapId;
            }

            var thread = new Thread(async () => await LoadMapFromEachPack(mapId)) {
                IsBackground = true
            };

            thread.Start();
        }

        private async Task LoadMapFromEachPack(int mapId) {
            this.IsLoading = true;

            var loadTimer = Stopwatch.StartNew();

            _packState.Module.ScriptEngine.Reset();

            var packTimings = new List<(Pack Pack, long LoadDuration)>();

            // TODO: Localize the loading messages.
            _loadingIndicator.Report("Loading marker packs...");

            await PrepareState(mapId);

            var packs = _packs.ToArray();

            foreach (var pack in packs) {
                pack.IsLoaded = false;

                if (!pack.AlwaysLoad.Value && !pack.ForceLoad) {
                    // Skip loading.
                    continue;
                }

                try {
                    var packTimer = Stopwatch.StartNew();

                    _loadingIndicator.Report($"Loading {pack.Pack.Name}...");
                    await pack.Pack.LoadMapAsync(mapId, _sharedPackCollection, _packReaderSettings);
                    pack.IsLoaded = true;

                    pack.LoadTime = packTimer.ElapsedMilliseconds;
                    packTimings.Add((pack.Pack, pack.LoadTime));
                } catch (FileNotFoundException e) {
                    Logger.Warn("Pack file '{packPath}' failed to load because it could not be found.", e.FileName);
                    _packs.Remove(pack);
                } catch (Exception e) {
                    Logger.Warn(e, $"Loading pack '{pack.Pack.Name}' failed.");
                }
            }

            _loadingIndicator.Report("Finalizing marker collection...");

            try {
                await _packState.LoadPackCollection(_sharedPackCollection);
            } catch (Exception e) {
                Logger.Warn(e, $"Finalizing packs failed.");
                _packState?.Unload();
            }

            // We only load scripts if they're enabled.
            if (_packState.Module.Settings.ScriptsEnabled.Value) {
                _loadingIndicator.Report("Loading scripts...");

                foreach (var pack in packs) {
                    if (!pack.AlwaysLoad.Value && !pack.ForceLoad) {
                        // Skip loading.
                        continue;
                    }

                    var scriptTimer = Stopwatch.StartNew();
                    await _packState.Module.ScriptEngine.LoadScript("pack.lua", pack.Pack.ResourceManager, pack.Pack.Name);
                    pack.LoadTime += scriptTimer.ElapsedMilliseconds;
                }
            }

            foreach (var pack in packs) {
                pack.Pack.ReleaseLocks();
            }
            
            _loadingIndicator.Report(null);

            this.IsLoading = false;

            Logger.Info($"Finished loading packs {string.Join(", ", packTimings.Select(p => $"{(p.Pack.ManifestedPack ? "+" : "-")}{p.Pack.Name}[{p.LoadDuration}ms]"))} in {loadTimer.ElapsedMilliseconds}ms for map {mapId}.");
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

            _packState.Unload();
        }

    }
}
