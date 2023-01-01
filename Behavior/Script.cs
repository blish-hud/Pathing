using System;
using System.Linq;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Neo.IronLua;
using TmfLib.Prototype;

namespace BhModule.Community.Pathing.Behavior {
    internal class Script : Behavior<IPathingEntity>, ICanFocus, ICanInteract, ICanFilter {

        private static readonly Logger Logger = Logger.GetLogger<Script>();

        public const  string PRIMARY_ATTR_NAME = "script";
        private const string ATTR_TICK         = PRIMARY_ATTR_NAME + "-tick";
        private const string ATTR_FOCUS        = PRIMARY_ATTR_NAME + "-focus";
        private const string ATTR_TRIGGER      = PRIMARY_ATTR_NAME + "-trigger";
        private const string ATTR_FILTER       = PRIMARY_ATTR_NAME + "-filter";
        private const string ATTR_ONCE         = PRIMARY_ATTR_NAME + "-once";

        private const int TICK_FREQUENCY = 0 /*500*/;

        public (string Name, object[] Args) TickFunc    { get; set; }
        public (string Name, object[] Args) FocusFunc   { get; set; }
        public (string Name, object[] Args) TriggerFunc { get; set; }
        public (string Name, object[] Args) FilterFunc  { get; set; }
        public (string Name, object[] Args) OnceFunc    { get; set; }

        private double _nextTick = 0;

        public Script(string tickFunc, string focusFunc, string triggerFunc, string filterFunc, string onceFunc, IPathingEntity entity) : base(entity) {
            this.TickFunc    = SplitFunc(tickFunc);
            this.FocusFunc   = SplitFunc(focusFunc);
            this.TriggerFunc = SplitFunc(triggerFunc);
            this.FilterFunc  = SplitFunc(filterFunc);
            this.OnceFunc    = SplitFunc(onceFunc);
        }

        public static IBehavior BuildFromAttributes(AttributeCollection attributes, IPathingEntity entity) {
            return new Script(attributes.TryGetAttribute(ATTR_TICK,    out var tickAttr) ? tickAttr.GetValueAsString() : null,
                              attributes.TryGetAttribute(ATTR_FOCUS,   out var focusAttr) ? focusAttr.GetValueAsString() : null,
                              attributes.TryGetAttribute(ATTR_TRIGGER, out var triggerAttr) ? triggerAttr.GetValueAsString() : null,
                              attributes.TryGetAttribute(ATTR_FILTER,  out var filterAttr) ? filterAttr.GetValueAsString() : null,
                              attributes.TryGetAttribute(ATTR_ONCE,    out var onceAttr) ? onceAttr.GetValueAsString() : null,
                              entity);
        }

        public void Focus() {
            if (this.FocusFunc.Name != null) {
                PathingModule.Instance.ScriptEngine.CallFunction(this.FocusFunc.Name, new object[] { _pathingEntity, true }.Concat(this.FocusFunc.Args));
            }
        }

        public void Unfocus() {
            if (this.FocusFunc.Name != null) {
                PathingModule.Instance.ScriptEngine.CallFunction(this.FocusFunc.Name, new object[] { _pathingEntity, false }.Concat(this.FocusFunc.Args));
            }
        }

        public void Interact(bool autoTriggered) {
            if (this.TriggerFunc.Name != null) {
                PathingModule.Instance.ScriptEngine.CallFunction(this.TriggerFunc.Name, new object[] { _pathingEntity, autoTriggered }.Concat(this.TriggerFunc.Args));
            }
        }

        public bool IsFiltered() {
            if (this.FilterFunc.Name != null) {
                var filterCall = PathingModule.Instance.ScriptEngine.CallFunction(this.FilterFunc.Name, new object[] { _pathingEntity }.Concat(this.FilterFunc.Args));

                if (filterCall != LuaResult.Empty) {
                    return filterCall.ToBoolean();
                }
            }

            return false;
        }

        private object GetValueFromString(string value) {
            if (value[0] != '\'' && value[0] != '"') {
                // nil
                if (value.Equals("nil")) {
                    return null;
                }

                // Float
                if (value.Contains(".")) {
                    if (float.TryParse(value, out float floatResult)) {
                        return floatResult;
                    }

                    return value;
                }

                // Int32
                if (int.TryParse(value, out int intResult)) {
                    return intResult;
                }

                // bool true
                if (string.Equals(value, "true", StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }

                // bool false
                if (string.Equals(value, "false", StringComparison.InvariantCultureIgnoreCase)) {
                    return false;
                }
            }

            // We'll default to a string.
            return value.Trim('\'', '"');
        }

        private (string Name, object[] Args) SplitFunc(string func) {
            if (!string.IsNullOrWhiteSpace(func)) {
                try {
                    if (func.Contains("(")) {
                        // This technically could allow for some pretty jank, but valid, syntax.  For the sake of simplicity, though...
                        string[] parts = func.Split('(');

                        func = parts[0];

                        string argStr = parts[1].Trim(' ', ')');

                        if (argStr.Length > 0) {
                            object[] args = argStr.Split(',').Select(GetValueFromString).ToArray(); // This is WRONG: a string could contain a comma.

                            return (func, args);
                        }
                    }

                    // Only func is provided.  No args.
                    return (func, Array.Empty<object>());
                } catch (Exception ex) {
                    Logger.Warn($"Pathable '{_pathingEntity.Guid.ToBase64String()}' has an invalid script attribute value of '{func}'.");
                }
            }

            return (null, null);
        }

        public string FilterReason() {
            return $"Hidden by a script.";
        }

        public override void Update(GameTime gameTime) {
            if (PathingModule.Instance.PackInitiator.IsLoading) {
                // Avoid calling Lua scripts until all packs are done loading.
                return;
            }

            if (this.OnceFunc.Name != null) {
                PathingModule.Instance.ScriptEngine.CallFunction(this.OnceFunc.Name, new object[] { _pathingEntity }.Concat(this.OnceFunc.Args));
                this.OnceFunc = (null, null);
            }

            if (this.TickFunc.Name != null && gameTime.TotalGameTime.TotalMilliseconds > _nextTick) {
                _nextTick = gameTime.TotalGameTime.TotalMilliseconds + TICK_FREQUENCY;

                PathingModule.Instance.ScriptEngine.CallFunction(this.TickFunc.Name, new object[] { _pathingEntity, gameTime }.Concat(this.TickFunc.Args));
            }
        }

    }
}