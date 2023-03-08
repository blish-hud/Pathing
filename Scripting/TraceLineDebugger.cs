using Neo.IronLua;
using System.Linq.Expressions;
using Blish_HUD;

namespace BhModule.Community.Pathing.Scripting {
    public class TraceLineDebugger : LuaTraceLineDebugger {

        public string LastFrameSource { get; private set; }
        public string LastFrameScope  { get; private set; }
        public int    LastFrameLine   { get; private set; }

        protected override LuaTraceChunk CreateChunk(Lua lua, LambdaExpression expr) {
            return base.CreateChunk(lua, expr);
        }

        protected override void OnExceptionUnwind(LuaTraceLineExceptionEventArgs e) {
            this.LastFrameSource = e.SourceName;
            this.LastFrameScope  = e.ScopeName;
            this.LastFrameLine   = e.SourceLine;

            base.OnExceptionUnwind(e);
        }

        protected override void OnFrameEnter(LuaTraceLineEventArgs e) {
            this.LastFrameSource = e.SourceName;
            this.LastFrameScope  = e.ScopeName;
            this.LastFrameLine   = e.SourceLine;

            base.OnFrameEnter(e);
        }

        protected override void OnFrameExit() {
            base.OnFrameExit();
        }

        protected override void OnTracePoint(LuaTraceLineEventArgs e) {
            this.LastFrameSource = e.SourceName;
            this.LastFrameScope  = e.ScopeName;
            this.LastFrameLine   = e.SourceLine;

            base.OnTracePoint(e);
        }

    }
}
