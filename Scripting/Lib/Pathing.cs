using SemVer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.Scripting.Lib {
    public class Pathing {

        private readonly PathingGlobal _global;

        internal Pathing(PathingGlobal global) {
            _global = global;
        }

        public string Version => _global.ScriptEngine.Module.Version.Clean();

        public bool IsVersionAtLeast(string version) {
            try {
                return _global.ScriptEngine.Module.Version >= new SemVer.Version(version);
            } catch (ArgumentException) {
                // Invalid version string.
                return false;
            }
        }
    }
}
