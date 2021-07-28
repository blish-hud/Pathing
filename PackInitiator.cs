using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.State;
using BhModule.Community.Pathing.UI.Controls;
using Blish_HUD;
using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using TmfLib;
using TmfLib.Reader;

namespace BhModule.Community.Pathing {
    public class PackInitiator : IUpdatable {

        private static readonly Logger Logger = Logger.GetLogger<PackInitiator>();

        private const int LOAD_RETRY_COUNTS = 3;

        private readonly string _watchPath;

        private readonly IRootPackState _packState;

        private readonly List<Pack> _packs = new();

        private SharedPackCollection _sharedPackCollection;

        private readonly SemaphoreSlim _packMutex = new(1, 1);

        private ContextMenuStripItem _allMarkers;
        
        private readonly PackReaderSettings _packReaderSettings;

        public PackInitiator(string watchPath, ModuleSettings moduleSettings) {
            _watchPath = watchPath;

            _packReaderSettings = new PackReaderSettings();
            _packReaderSettings.VenderPrefixes.Add("bh-"); // Support Blish HUD specific categories/markers/trails/attributes.

            _packState = new SharedPackState(moduleSettings);

            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChanged;

            // TODO: Ensure this is all cleaned up in Unload() and move this to a better spot.

            _allMarkers          = PathingModule.Instance._pathingContextMenuStrip.AddMenuItem("All Markers");
            _allMarkers.CanCheck = true;
            _allMarkers.Checked  = moduleSettings.GlobalPathablesEnabled.Value;

            _allMarkers.CheckedChanged += delegate(object sender, CheckChangedEvent e) {
                moduleSettings.GlobalPathablesEnabled.Value = e.Checked;
            };

            moduleSettings.GlobalPathablesEnabled.SettingChanged += delegate(object sender, ValueChangedEventArgs<bool> args) {
                _allMarkers.Checked = args.NewValue;
            };

            PathingModule.Instance._pathingContextMenuStrip.AddMenuItem("Reload Markers").Click += async delegate {
                if (_packState.CurrentMapId < 0) return;
                
                await LoadMapFromEachPack(_packState.CurrentMapId);
            };

            PathingModule.Instance._pathingContextMenuStrip.AddMenuItem("Unload Markers").Click += delegate {
                if (_packState.CurrentMapId < 0) return;

                UnloadStateAndCollection();
            };

            //PathingModule.Instance._pathingContextMenuStrip.AddMenuItem("Export Marker State").Click += delegate {
            //    if (_packState.CurrentMapId < 0) return;

            //    string exportPath = Utility.DataDirUtil.GetSafeDataDir("export");

            //    foreach (var pathable in GameService.Graphics.World.Entities) {
            //        if (pathable is StandardMarker sm) {
            //            string output = JsonConvert.SerializeObject(sm, Formatting.Indented);
            //            File.WriteAllText(Path.Combine(exportPath, $"{sm.Guid}.txt"), output);
            //        }
            //    }
            //};
        }

        public async Task Init() {
            await LoadAllPacks();
        }

        private async Task LoadUnpackedPackFiles(string unpackedDir) {
            var newPack = Pack.FromDirectoryMarkerPack(unpackedDir);

            await _packMutex.WaitAsync();
            _packs.Add(newPack);
            _packMutex.Release();
        }

        private async Task LoadPackedPackFiles(IEnumerable<string> zipPackFiles) {
            foreach (var newPack in zipPackFiles.Select(Pack.FromArchivedMarkerPack)) {
                await _packMutex.WaitAsync();
                _packs.Add(newPack);
                _packMutex.Release();
            }
        }

        private void UnloadStateAndCollection() {
            _sharedPackCollection?.Unload();
            _allMarkers.Submenu = null;
            _packState.UnloadPacks();
        }

        private async Task LoadAllPacks() {
            await LoadPackedPackFiles(Directory.GetFiles(_watchPath, "*.zip", SearchOption.AllDirectories));
            await LoadPackedPackFiles(Directory.GetFiles(_watchPath, "*.taco", SearchOption.AllDirectories));
            await LoadUnpackedPackFiles(_watchPath);

            await LoadMapFromEachPack(_packState.CurrentMapId = GameService.Gw2Mumble.CurrentMap.Id);
        }

        private void PrepareState(int mapId) {
            UnloadStateAndCollection();

            _sharedPackCollection = new SharedPackCollection();
        }

        private async Task LoadMapFromEachPack(int mapId, int retry = 3) {
            PrepareState(mapId);

            await _packMutex.WaitAsync();

            try {
                foreach (var pack in _packs) {
                    await pack.LoadMapAsync(mapId, _sharedPackCollection, _packReaderSettings);
                }
            } catch (Exception e) {
                Logger.Warn(e, "Loading pack failed.");

                _packMutex.Release();

                if (retry > 0) {
                    await LoadMapFromEachPack(--retry);
                }

                Logger.Error($"Loading pack failed after {LOAD_RETRY_COUNTS} attempts.");
            }

            _packMutex.Release();

            await _packState.LoadPackCollection(_sharedPackCollection);

            // TODO: Check Map and Compass individually (currently map values are ignored and are assumed to match).
            _allMarkers.Submenu = new CategoryContextMenuStrip(_packState, _sharedPackCollection.Categories);
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

            _packState.UnloadPacks();
        }

    }
}
