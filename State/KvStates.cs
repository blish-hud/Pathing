using System;
using System.IO;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility;
using FASTER.core;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.State {
    public class KvStates : ManagedState {

        private const double INTERVAL_CHECKPOINT = 5000; // 5 seconds

        private string _kvDir;

        private FasterKV<string, string> _kvStore;

        private bool _dirty = false;

        private double _lastCheckpointCheck = INTERVAL_CHECKPOINT;

        public KvStates(IRootPackState rootPackState) : base(rootPackState) { /* NOOP */ }

        protected async override Task<bool> Initialize() {
            _kvDir = DataDirUtil.GetSafeDataDir(DataDirUtil.COMMON_KV);

            var defLog = Devices.CreateLogDevice(Path.Combine(_kvDir, "bkv.db"));
            var objLog = Devices.CreateLogDevice(Path.Combine(_kvDir, "okv.db"));

            _kvStore = new FasterKV<string, string>(new FasterKVSettings<string, string>(_kvDir) {
                LogDevice       = defLog,
                ObjectLogDevice = objLog
            });

            try {
                await _kvStore.RecoverAsync();
            } catch (Exception ex) {

            }

            return true;
        }

        public ClientSession<string, string, string, string, Empty, IFunctions<string, string, string, string, Empty>> GetSession() {
            return _kvStore.NewSession(new SimpleFunctions<string, string>());
        }

        public void Invalidate() {
            _dirty = true;
        }

        public override async Task Reload() {
            await Unload();
            await Initialize();
        }

        private async Task FlushCheckpoint(GameTime gameTime) {
            if (_dirty) {
                _dirty = false;
                await _kvStore.TakeHybridLogCheckpointAsync(CheckpointType.FoldOver, true);
            }
        }

        public override void Update(GameTime gameTime) {
            UpdateCadenceUtil.UpdateAsyncWithCadence(FlushCheckpoint, gameTime, INTERVAL_CHECKPOINT, ref _lastCheckpointCheck);
        }

        public override async Task Unload() {
            await _kvStore.CompleteCheckpointAsync();
            _kvStore.Dispose();
        }

    }
}
