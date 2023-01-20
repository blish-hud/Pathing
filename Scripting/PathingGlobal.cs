using System;
using System.Collections.Generic;
using System.Linq;
using BhModule.Community.Pathing.Scripting.Lib;
using BhModule.Community.Pathing.Scripting.Lib.Std;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Neo.IronLua;
using TmfLib;

namespace BhModule.Community.Pathing.Scripting; 

public class PathingGlobal : LuaTable {

    private readonly Dictionary<int, LuaChunk> _loadedChunks = new();

    private readonly LuaGlobal _sandboxedGlobal;

    internal ScriptEngine ScriptEngine { get; set; }

    public PathingGlobal(Lua lua) {
		_sandboxedGlobal = new LuaGlobal(lua);

        this.Debug = new Debug(this);
        this.Event = new Event(this);
        this.User  = new User(this);

        this.World = new World(this);
        this.I     = new(this);
    }

    [LuaMember(nameof(World))]
    public World World { get; }

    private bool _packsWarning = false;
    [LuaMember("Packs")]
    public World Packs {
        get {
            if (!_packsWarning) {
                this.ScriptEngine.PushMessage("`Packs` is deprecated.  Use `World` instead.", -1);
                _packsWarning = true;
            }

            return this.World;
        }
    }

    [LuaMember(nameof(Mumble))]
    public Gw2MumbleService Mumble => GameService.Gw2Mumble;

    [LuaMember(nameof(Debug))]
    public Debug Debug { get; }

    [LuaMember(nameof(I))]
    public Instance I { get; }

    [LuaMember(nameof(Menu))]
    public Menu Menu { get; } = new("Scripts", null, false, false);
    
    [LuaMember(nameof(Event))]
    public Event Event { get; }

    [LuaMember(nameof(User))]
    public User User { get; }

    [LuaMember("require")]
    public LuaResult Require(string scriptName, IPackResourceManager resourceManager) {
        scriptName = scriptName.ToLowerInvariant();

        if (!scriptName.EndsWith(".lua")) {
            scriptName += ".lua";
        }

        int lookup = HashCode.Combine(scriptName, resourceManager);

        if (!_loadedChunks.TryGetValue(lookup, out var chunk)) {
            var loadScript = this.ScriptEngine.LoadScript(scriptName, resourceManager);
            loadScript.Wait();

            if (loadScript.Result != null) {
                _loadedChunks[lookup] = chunk = loadScript.Result;
            }
        }

        return chunk == null 
                   ? LuaResult.Empty 
                   : new LuaResult(chunk);
    }

    #region Replicated LuaGlobal

    [LuaMember("getmetatable")]
    public static LuaTable LuaGetMetaTable(object obj) => LuaGlobal.LuaGetMetaTable(obj);

	[LuaMember("rawmembers")]
	public static IEnumerable<KeyValuePair<string, object>> LuaRawMembers(LuaTable t) => LuaGlobal.LuaRawMembers(t);

	[LuaMember("rawarray")]
	public static IList<object> LuaRawArray(LuaTable t) => LuaGlobal.LuaRawArray(t);

	[LuaMember("ipairs")]
	public static LuaResult LuaIPairs(LuaTable t) => LuaGlobal.LuaIPairs(t);

	[LuaMember("mpairs")]
	public static LuaResult LuaMPairs(LuaTable t) => LuaGlobal.LuaMPairs(t);

    [LuaMember("pairs")] public static LuaResult LuaPairs(LuaTable t) => LuaGlobal.LuaPairs(t);

    [LuaMember("next")]
    private static LuaResult LuaNext(LuaTable t, object next) {
        if (t == null)
            return null;
        var n = t.NextKey(next);
        return new LuaResult(n, t[n]);
    }

	[LuaMember("nextKey")]
	private static object LuaNextKey(LuaTable t, object next) => t?.NextKey(next);

	[LuaMember("print")]
	private void LuaPrint(params object[] args) {
		if (args == null)
			return;

		this.Debug.Print(string.Join(" ", (from a in args select a == null ? string.Empty : a.ToString())));
	}

	[LuaMember("rawequal")]
	public static bool LuaRawEqual(object a, object b) => LuaGlobal.LuaRawEqual(a, b);

	[LuaMember("rawget")]
	public static object LuaRawGet(LuaTable t, object index) => LuaGlobal.LuaRawGet(t, index);

	[LuaMember("rawlen")]
	public static int LuaRawLen(object v) => LuaGlobal.LuaRawLen(v);

	[LuaMember("rawset")]
	public static LuaTable LuaRawSet(LuaTable t, object index, object value) => LuaGlobal.LuaRawSet(t, index, value);

	[LuaMember("select")]
	public static LuaResult LuaSelect(string index, params object[] values) => LuaGlobal.LuaSelect(index, values);

	[LuaMember("setmetatable")]
	public static LuaTable LuaSetMetaTable(LuaTable t, LuaTable metaTable) => LuaGlobal.LuaSetMetaTable(t, metaTable);

	[LuaMember("tonumber")]
	public object LuaToNumber(object v, int? iBase = null) => _sandboxedGlobal.LuaToNumber(v, iBase);

    /// <summary></summary>
    /// <param name="v"></param>
    /// <returns></returns>
    [LuaMember("tostring")]
    public static string LuaToString(object v) => LuaGlobal.LuaToString(v);

	/// <summary></summary>
	/// <param name="v"></param>
	/// <param name="clr"></param>
	/// <returns></returns>
	[LuaMember("type")]
	public static string LuaTypeTest(object v, bool clr = false) => LuaGlobal.LuaTypeTest(v, clr);

	[LuaMember("_VERSION")]
    public virtual string Version => LuaGlobal.VersionString;

    [LuaMember("coroutine")]
    private static dynamic LuaLibraryCoroutine => LuaType.GetType(typeof(LuaThread));

    [LuaMember("bit32")]
    private static dynamic LuaLibraryBit32 => LuaType.GetType(typeof(LuaLibraryBit32));

    [LuaMember("math")]
    private static dynamic LuaLibraryMath => LuaType.GetType(typeof(LuaLibraryMath));

    [LuaMember("string")]
    private static dynamic LuaLibraryString => LuaType.GetType(typeof(LuaLibraryString));

    [LuaMember("table")]
    private static dynamic LuaLibraryTable => LuaType.GetType(typeof(LuaTable));

    [LuaMember("os")]
    private static dynamic LuaLibraryOS => LuaType.GetType(typeof(LuaLibraryOS));

    #endregion

    internal void Update(GameTime gameTime) {
        this.Event.Update(gameTime);
    }

}