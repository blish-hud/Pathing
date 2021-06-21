using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.State;
using Blish_HUD;
using TmfLib;
using TmfLib.Reader;

namespace BhModule.Community.Pathing {
    public class PackLoader {

        private static readonly Logger Logger = Logger.GetLogger<PackLoader>();

        private const int LOAD_RETRY_COUNTS = 3;

        private readonly int                _mapId;
        private readonly IEnumerable<Pack>  _packs;
        private readonly IPackCollection    _packCollection;
        private readonly PackReaderSettings _packReaderSettings;
        private readonly IRootPackState     _packState;

        public bool Running { get; private set; }

        private Thread _loadThread;

        public PackLoader(int mapId, IEnumerable<Pack> packs, IPackCollection packCollection, PackReaderSettings packReaderSettings, IRootPackState packState) {
            _mapId              = mapId;
            _packs              = packs;
            _packCollection     = packCollection;
            _packReaderSettings = packReaderSettings;
            _packState          = packState;
        }

        public void Start() {
            if (this.Running) return;

            _loadThread = new Thread(this.Run);
            _loadThread.Start();
        }

        private async void Run() {
            this.Running = true;

            await LoadMapFromEachPack();

            this.Running = false;
        }

        private async Task LoadMapFromEachPack(int retry = LOAD_RETRY_COUNTS) {
            try {
                foreach (var pack in _packs) {
                    await pack.LoadMapAsync(_mapId, _packCollection, _packReaderSettings);
                }
            } catch (Exception e) {
                Logger.Warn(e, "Loading pack failed.");

                if (retry > 0) {
                    await LoadMapFromEachPack(--retry);
                }

                Logger.Error($"Loading pack failed after {LOAD_RETRY_COUNTS} attempts.");
            }

            await _packState.LoadPackCollection(_packCollection);
        }

    }
}
