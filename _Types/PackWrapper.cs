using BhModule.Community.Pathing.MarkerPackRepo;
using Blish_HUD.Settings;
using System.Linq;
using TmfLib;

namespace BhModule.Community.Pathing {
    internal class PackWrapper {

        private readonly PathingModule _module;

        public Pack Pack { get; private set; }

        private MarkerPackPkg _package = null;
        public MarkerPackPkg Package { get {
                if (_package == null) {
                    _package = _module.MarkerPackRepo.MarkerPackages.FirstOrDefault(p => p.FileName.ToLowerInvariant().StartsWith(this.Pack.Name.ToLowerInvariant()));
                }

                return _package;
            }
        }

        public SettingEntry<bool> AlwaysLoad { get; private set; }

        public bool ForceLoad { get; set; } = false;

        public bool IsLoaded { get; set; } = false;

        public long LoadTime { get; set; }

        public PackWrapper(PathingModule module, Pack pack, SettingEntry<bool> alwaysLoad) {
            _module = module;

            this.Pack = pack;
            this.AlwaysLoad = alwaysLoad;

        }

    }
}
