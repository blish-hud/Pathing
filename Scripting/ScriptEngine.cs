using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Scripting.Extensions;
using Blish_HUD;
using Blish_HUD.Debug;
using Microsoft.Xna.Framework;
using Neo.IronLua;
using TmfLib;

namespace BhModule.Community.Pathing.Scripting; 

public class ScriptEngine {

    public struct ScriptMessage {

        public DateTime Timestamp { get; }
        public string   Message   { get; }

        /// <summary>
        /// The source script that triggered the message.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// 0 = info, 1 = warn, 2 = error
        /// </summary>
        public int LogLevel { get; }

        public ScriptMessage(string message, string source, DateTime timestamp, int logLevel = 0) {
            this.Message   = message;
            this.Source    = source;
            this.Timestamp = timestamp;
            this.LogLevel  = logLevel;
        }

    }

    private static readonly Logger Logger = Logger.GetLogger<ScriptEngine>();

    internal PathingModule Module { get; }

    private Lua _lua;

    public PathingGlobal Global { get; private set; }

    private TraceLineDebugger _stackTraceDebugger;

    public SafeList<ScriptState> Scripts { get; } = new();

    private readonly RingBuffer<TimeSpan> _frameExecutionTime   = new(10);
    private          TimeSpan             _currentFrameDuration = TimeSpan.Zero;

    public TimeSpan FrameExecutionTime {
        get {
            TimeSpan[] frameTimes = _frameExecutionTime.InternalBuffer.ToArray();
            var averageFrameTime = TimeSpan.Zero;

            foreach (var frameTime in frameTimes) {
                averageFrameTime += frameTime;
            }

            return averageFrameTime.Divide(frameTimes.Length);
        }
    }

    public readonly SafeList<ScriptMessage> OutputMessages = new();

    public ScriptEngine(PathingModule module) {
        this.Module = module;

        LuaType.RegisterTypeExtension(typeof(StandardMarkerScriptExtensions));
        LuaType.RegisterTypeExtension(typeof(PathingCategoryScriptExtensions));
        LuaType.RegisterTypeExtension(typeof(GuidExtensions));

        PathingCategoryScriptExtensions.SetPackInitiator(this.Module.PackInitiator);
    }

    private void BuildEnv() {
        _lua?.Dispose();
        _lua = new Lua(LuaIntegerType.Int32, LuaFloatType.Float);

        _stackTraceDebugger = new TraceLineDebugger();
        
        this.Global = _lua.CreateEnvironment<PathingGlobal>();
        this.Global.ScriptEngine = this;

        PushMessage($"Loaded new environment.", -1);
    }

    /// <param name="logLevel">0 = info, 1 = warn, 2 = error</param>
    public void PushMessage(string message, int logLevel = 0, DateTime? timestamp = null, string source = null) {
        timestamp ??= DateTime.UtcNow;

        if (logLevel == -1) {
            source = "system";
        }

        OutputMessages.Add(new ScriptMessage(message, source ?? _stackTraceDebugger.LastFrameSource ?? "unknown", timestamp.Value, logLevel));
    }

    private void PublishException(LuaException ex) {
        int    sourceLine = ex.Line;
        string source     = ex.FileName ?? ex.Source;

        switch (ex.Source) {
            case "Anonymously Hosted DynamicMethods Assembly":
            case "Neo.Lua":
                source     = _stackTraceDebugger.LastFrameSource;
                sourceLine = _stackTraceDebugger.LastFrameLine;
                break;
        }

        string message = string.IsNullOrEmpty(ex.FileName)
                             ? $"{ex.Message.TrimEnd('.')} during execution on or around line {sourceLine} in '{_stackTraceDebugger.LastFrameScope}'."
                             : $"{ex.Message.TrimEnd('.')} on or around line {sourceLine} column {ex.Column}.";

        switch (ex) {
            case LuaParseException lpe:
                PushMessage(message, 2, source: source);
                break;
            case LuaRuntimeException lre:
                PushMessage(message, 2, source: source);
                break;
        }
    }

    public (LuaResult Result, bool Success) WrapScriptCall(Func<LuaResult> scriptCallDelegate) {
        var runClock = Stopwatch.StartNew();

        LuaResult result = null;
        bool success = true;

        try {
            result = scriptCallDelegate();
        } catch (LuaRuntimeException ex) {
            success = false;
            PublishException(ex);
        } catch (TargetInvocationException ex) when (ex.InnerException is LuaRuntimeException lre) {
            success = false;
            PublishException(lre);
        } catch (Exception ex) {
            success = false;
            PushMessage(ex.Message, 2);

            // PublishException(ex); can't do this because it's not a LuaException
        }

        _currentFrameDuration += runClock.Elapsed;

        return (result, success);
    }

    public LuaResult CallFunction(string funcName, params object[] args) {
        return WrapScriptCall(() => this.Global.CallMemberDirect(funcName, args, true, false, true, true)).Result;
    }

    public LuaResult CallFunction(string funcName, IEnumerable<object> args) {
        return CallFunction(funcName, args.ToArray());
    }

    public async Task<LuaChunk> LoadScript(string scriptName, IPackResourceManager resourceManager) {
        if (!resourceManager.ResourceExists(scriptName)) {
            if (scriptName != "pack.lua") {
                Logger.Warn($"Attempted to load script '{scriptName}', but it could not be found.");
            }

            return null;
        }

        try {
            var scriptStream = await resourceManager.LoadResourceStreamAsync(scriptName);

            using (var scriptReader = new StreamReader(scriptStream)) {
                string scriptSource = await scriptReader.ReadToEndAsync();

                var chunk = _lua.CompileChunk(scriptSource, scriptName, new LuaCompileOptions() {
                                                  DebugEngine = _stackTraceDebugger
                                              }, new KeyValuePair<string, Type>("Pack", typeof(PackContext)));

                var newScript = new ScriptState(chunk);
                newScript.Run(this.Global, new PackContext(this, resourceManager));
                this.Scripts.Add(newScript);

                return chunk;
            }
        } catch (LuaException ex) {
            PublishException(ex);
        } catch (Exception ex) {
            Logger.Warn(ex, $"Failed to load script '{scriptName}'.");
        }

        PushMessage($"Failed to load {scriptName}.", 2, source: "system");

        return null;
    }

    internal LuaResult EvalScript(string script) {
        try {
            var chunk = _lua.CompileChunk(script, "eval", new LuaCompileOptions() {
                                              DebugEngine = _stackTraceDebugger
                                          });

            var scriptResult = WrapScriptCall(() => chunk.Run(this.Global));

            if (scriptResult.Success) {
                return scriptResult.Result;
            }
        } catch (LuaException ex) {
            PublishException(ex);
        } catch (Exception ex) {
            Logger.Warn(ex, $"Failed to eval script.");
        }

        return null;
    }

    public void Update(GameTime gameTime) {
        _frameExecutionTime.PushValue(_currentFrameDuration);
        _currentFrameDuration = TimeSpan.Zero;
        this.Global?.Update(gameTime);
    }

    public void Reset() {
        Scripts.Clear();
        BuildEnv();
    }

    public void Unload() {
        Scripts.Clear();
        _lua.Dispose();
        PathingCategoryScriptExtensions.SetPackInitiator(null);
    }

}