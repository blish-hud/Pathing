using BhModule.Community.Pathing.Entity;
using Neo.IronLua;
using TmfLib;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing.Scripting {
    public class PackContext {

        private readonly ScriptEngine _scriptEngine;

        public IPackResourceManager ResourceManager { get; }

        public PackContext(ScriptEngine scriptEngine, IPackResourceManager resourceManager) {
            _scriptEngine = scriptEngine;

            this.ResourceManager = resourceManager;
        }

        public LuaResult Require(string path) 
            => _scriptEngine.Global.Require(path, this.ResourceManager);

        public StandardMarker CreateMarker(LuaTable attributes = null) 
            => _scriptEngine.Global.I.Marker(this.ResourceManager, attributes);

        public StandardTrail CreateTrail(LuaTable attributes = null)
            => _scriptEngine.Global.I.Trail(this.ResourceManager, attributes);

    }
}
