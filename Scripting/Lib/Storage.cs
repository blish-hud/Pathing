using System;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.Scripting.Lib {
    public class Storage {

        private const string GLOBAL_NAMESPACE = "000";
        private const string KEY_PATTERN      = "{0}\\{1}";

        private readonly PathingGlobal _global;

        internal Storage(PathingGlobal global) {
            _global = global;
        }

        // Validation

        private string ValidateKey(string ns, string name) {
            ns ??= GLOBAL_NAMESPACE;

            if (ns.Length > 64 || string.IsNullOrWhiteSpace(ns)) {
                throw new ArgumentException("Key namespace must be nil or between 1 and 64 characters");
            }

            if (name.Length > 64 || string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("Key name must be between 1 and 64 characters");
            }

            return string.Format(KEY_PATTERN, ns, name);
        }

        // Upsert data

        public string UpsertValue(string ns, string name, string value) {
            string key = ValidateKey(ns, name);

            using var session = _global.ScriptEngine.Module.PackInitiator.PackState.KvStates.GetSession();

            session.Upsert(ref key, ref value);

            _global.ScriptEngine.Module.PackInitiator.PackState.KvStates.Invalidate();

            return value;
        }

        public string UpsertValue(string name, string value) {
            return UpsertValue(null, name, value);
        }

        // Read data

        public string ReadValue(string ns, string name) {
            string key = ValidateKey(ns, name);

            using var session = _global.ScriptEngine.Module.PackInitiator.PackState.KvStates.GetSession();

            string output = null;
            var read = session.Read(ref key, ref output);
            return output;
        }

        public string ReadValue(string name) {
            return ReadValue(null, name);
        }

        // Delete data

        public void DeleteValue(string ns, string name) {
            string key = ValidateKey(ns, name);

            using var session = _global.ScriptEngine.Module.PackInitiator.PackState.KvStates.GetSession();

            session.Delete(ref key);

            _global.ScriptEngine.Module.PackInitiator.PackState.KvStates.Invalidate();
        }

        public void DeleteValue(string name) {
            DeleteValue(null, name);
        }

    }
}
