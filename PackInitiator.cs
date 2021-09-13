using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls;
using Blish_HUD;
using Blish_HUD.Controls;
using Blish_HUD.Graphics;
using Microsoft.Xna.Framework;
using TmfLib;
using TmfLib.Reader;

namespace BhModule.Community.Pathing {
    public class PackInitiator : IUpdatable {

        private static readonly Logger Logger = Logger.GetLogger<PackInitiator>();

        private const int LOAD_RETRY_COUNTS = 3;

        private readonly string            _watchPath;
        private readonly ModuleSettings    _moduleSettings;
        private readonly IProgress<string> _loadingIndicator;

        private readonly IRootPackState _packState;

        private readonly SafeList<Pack> _packs = new();

        private SharedPackCollection _sharedPackCollection;
        
        private readonly PackReaderSettings _packReaderSettings;

        public PackInitiator(string watchPath, ModuleSettings moduleSettings, IProgress<string> loadingIndicator) {
            _watchPath        = watchPath;
            _moduleSettings   = moduleSettings;
            _loadingIndicator = loadingIndicator;

            _packReaderSettings = new PackReaderSettings();
            _packReaderSettings.VenderPrefixes.Add("bh-"); // Support Blish HUD specific categories/markers/trails/attributes.

            _packState = new SharedPackState(moduleSettings);

            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;
        }

        public IEnumerable<ContextMenuStripItem> GetPackMenuItems() {
            // All Markers
            var allMarkers = new ContextMenuStripItem() {
                Text     = "All Markers", // TODO: Localize "All Markers"
                CanCheck = true,
                Checked  = _moduleSettings.GlobalPathablesEnabled.Value,
                // TODO: Don't show menu if we're mid-loading or unloaded.
                Submenu  = new CategoryContextMenuStrip(_packState, _sharedPackCollection.Categories)
            };

            allMarkers.CheckedChanged += (_, e) => {
                _moduleSettings.GlobalPathablesEnabled.Value = e.Checked;
            };

            // Reload Markers
            var reloadMarkers = new ContextMenuStripItem() {
                Text    = "Reload Markers", // TODO: Localize "Reload Markers"
                Enabled = _packState.CurrentMapId > 0
            };

            reloadMarkers.Click += async (_, _) => {
                if (_packState.CurrentMapId < 0) return;

                await LoadMapFromEachPack(_packState.CurrentMapId);
            };

            // Unload Markers
            var unloadMarkers = new ContextMenuStripItem() {
                Text    = "Unload Markers", // TODO: Localize "Unload Markers"
                Enabled = _packState.CurrentMapId > 0
            };

            unloadMarkers.Click += async (_, _) => {
                if (_packState.CurrentMapId < 0) return;

                await UnloadStateAndCollection();
            };
            
            yield return allMarkers;
            yield return reloadMarkers;
            yield return unloadMarkers;
        }

        public async Task Init() {
            await LoadAllPacks();
        }

        private async Task LoadUnpackedPackFiles(string unpackedDir) {
            var newPack = Pack.FromDirectoryMarkerPack(unpackedDir);
            
            _packs.Add(newPack);
        }

        private async Task LoadPackedPackFiles(IEnumerable<string> zipPackFiles) {
            foreach (var newPack in zipPackFiles.Select(Pack.FromArchivedMarkerPack)) {
                _packs.Add(newPack);
            }
        }

        private async Task LoadWebPackFile() {
            var webReader = new WebReader("https://webpacks.blishhud.com/reactif-en/");
            await webReader.InitWebReader();

            _packs.Add(Pack.FromIDataReader(webReader));
        }

        private async Task UnloadStateAndCollection() {
            _sharedPackCollection?.Unload();
            await _packState.Unload();
        }

        private async Task LoadAllPacks() {
            await LoadPackedPackFiles(Directory.GetFiles(_watchPath, "*.zip", SearchOption.AllDirectories));
            await LoadPackedPackFiles(Directory.GetFiles(_watchPath, "*.taco", SearchOption.AllDirectories));
            //await LoadWebPackFile();
            await LoadUnpackedPackFiles(_watchPath);

            // If the module loads at launch, this can end up firing twice.
            // If the module loads after launch (manually enabled), we need this to populate the current map.
            if (GameService.Gw2Mumble.CurrentMap.Id != default) {
                await LoadMapFromEachPack(_packState.CurrentMapId = GameService.Gw2Mumble.CurrentMap.Id);
            }
        }

        private async Task PrepareState(int mapId) {
            await UnloadStateAndCollection();

            _sharedPackCollection = new SharedPackCollection();
        }

        private async Task LoadMapFromEachPack(int mapId, int retry = 3) {
            // TODO: Localize the loading messages.

            _loadingIndicator.Report("Loading marker packs...");

            // We unlock frames to avoid timestep frame limiter from slowing our loading down.
            var currentFrameLimiter = GameService.Graphics.FrameLimiter;
            GameService.Graphics.FrameLimiter = FramerateMethod.Unlimited;

            await PrepareState(mapId);

            try {
                foreach (var pack in _packs.ToArray()) {
                    _loadingIndicator.Report($"Loading {pack.Name}...");
                    await pack.LoadMapAsync(mapId, _sharedPackCollection, _packReaderSettings);
                }
            } catch (Exception e) {
                Logger.Warn(e, "Loading pack failed.");

                if (retry > 0) {
                    await LoadMapFromEachPack(--retry);
                }

                Logger.Error($"Loading pack failed after {LOAD_RETRY_COUNTS} attempts.");
            }

            _loadingIndicator.Report("Finalizing marker collection...");
            await _packState.LoadPackCollection(_sharedPackCollection);

            // We set the frame limiter back.
            GameService.Graphics.FrameLimiter = currentFrameLimiter;

            foreach (var pack in _packs.ToArray()) {
                pack.ReleaseLocks();
            }

            _loadingIndicator.Report("");
        }

        private async void OnMapChanged(object sender, ValueEventArgs<int> e) {
            if (e.Value == _packState.CurrentMapId) return;

            _packState.CurrentMapId = e.Value;

            await LoadMapFromEachPack(e.Value);
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
