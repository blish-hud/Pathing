using Blish_HUD;

namespace BhModule.Community.Pathing.Utility.TwoMGFX {
    public class LoggerEffectCompilerOutput : IEffectCompilerOutput {

        private static readonly Logger Logger = Logger.GetLogger<LoggerEffectCompilerOutput>();

        public void WriteWarning(string file, int line, int column, string message) {
            Logger.Warn("Warning: {0}({1},{2}): {3}", file, line, column, message);
        }

        public void WriteError(string file, int line, int column, string message) {
            Logger.Error($"Error: {file}({line},{column}): {message}");
        }

    }
}
