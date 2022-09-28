using System;
using System.Diagnostics;
using Neo.IronLua;

namespace BhModule.Community.Pathing.Scripting {
    public class ScriptState {

        private readonly LuaChunk _luaChunk;

        public string Name => _luaChunk.ChunkName;

        public TimeSpan LoadTime { get; private set; }

        public ScriptState(LuaChunk luaChunk) {
            _luaChunk = luaChunk;
        }

        public void Run(LuaTable env, params object[] args) {
            var runClock = Stopwatch.StartNew();
            _luaChunk.Run(env, args);
            LoadTime = runClock.Elapsed;
        }

    }
}