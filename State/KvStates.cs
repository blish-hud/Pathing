using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace BhModule.Community.Pathing.State {
    public class KvStates : ManagedState {

        private static readonly Logger Logger = Logger.GetLogger<KvStates>();

        private const string KVFILE = "kv.json";
        private const double INTERVAL_CHECKPOINT = 5000; // 5 seconds

        private string _kvDir;
        private string _kvFile;

        private bool _dirty = false;

        private double _lastCheckpointCheck = INTERVAL_CHECKPOINT;

        private ConcurrentDictionary<string, string> _kvStore;

        public KvStates(IRootPackState rootPackState) : base(rootPackState) { /* NOOP */ }

        protected async override Task<bool> Initialize() {
            _kvDir = DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_KV);
            _kvFile = Path.Combine(_kvDir, KVFILE);

            LoadKv();

            return true;
        }

        private void LoadKv() {
            try {
                if (File.Exists(_kvFile)) {
                    _kvStore = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(File.ReadAllText(_kvFile)) ?? new();
                } else {
                    _kvStore = new();
                }
            } catch (Exception ex) {
                _kvStore = new();
                Logger.Warn(ex, $"Failed to load {KVFILE}.  Settings will not be restored!");
            }
        }

        private async Task FlushKv(GameTime gameTime) {
            if (_dirty) {
                _dirty = false;

                File.WriteAllText(_kvFile, JsonConvert.SerializeObject(_kvStore, Formatting.Indented));
            }
        }

        public string UpsertValue(string key, string value) {
            bool updated = false;

            _kvStore.AddOrUpdate(key, value, (_, existingVal) => {
                if (existingVal != value) {
                    updated = true;
                    return value;
                }

                return existingVal;
            });

            if (updated) {
                Invalidate();
            }

            return value;
        }

        public string ReadValue(string name) {
            _kvStore.TryGetValue(name, out string value);
            return value;
        }

        public void DeleteValue(string key) {
            if (_kvStore.TryRemove(key, out _)) {
                Invalidate();
            }
        }

        public void Invalidate() {
            _dirty = true;
        }

        public override async Task Reload() {
            await Unload();
            await Initialize();
        }

        public override void Update(GameTime gameTime) {
            UpdateCadenceUtil.UpdateAsyncWithCadence(FlushKv, gameTime, INTERVAL_CHECKPOINT, ref _lastCheckpointCheck);
        }

        public override async Task Unload() {
            await FlushKv(null);
        }

    }
}
