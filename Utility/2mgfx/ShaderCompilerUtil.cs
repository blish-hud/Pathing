using System;
using System.IO;
using Blish_HUD;

namespace BhModule.Community.Pathing.Utility.TwoMGFX {

    public static class ShaderCompilerUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(ShaderCompilerUtil));

        public static byte[] CompileShader(string shaderPath) {
            var options = new Options() {
                Debug      = false,
                Defines    = "",
                SourceFile = shaderPath
            };

            ShaderResult shaderResult;

            try {
                shaderResult = ShaderResult.FromFile(options.SourceFile, options, new LoggerEffectCompilerOutput());
            } catch (Exception ex) {
                Logger.Warn(ex, $"Failed to parse '{shaderPath}'!");

                return null;
            }

            // Create the effect object.
            EffectObject effect;
            string shaderErrorsAndWarnings = string.Empty;
            try {
                effect = EffectObject.CompileEffect(shaderResult, out shaderErrorsAndWarnings);

                if (!string.IsNullOrEmpty(shaderErrorsAndWarnings)) {
                    Logger.Warn(shaderErrorsAndWarnings);
                }
            } catch (ShaderCompilerException) {
                // Write the compiler errors and warnings and let the user know what happened.
                Logger.Warn(shaderErrorsAndWarnings);
                Logger.Warn($"Failed to compile '{shaderPath}'!");
                return null;
            } catch (Exception ex) {
                // First write all the compiler errors and warnings.
                if (!string.IsNullOrEmpty(shaderErrorsAndWarnings)) {
                    Logger.Warn(shaderErrorsAndWarnings);
                }

                // If we have an exception message then write that.
                if (!string.IsNullOrEmpty(ex.Message)) {
                    Logger.Warn(ex, $"Unhandled exception when compiling shader {shaderPath}.");
                }

                return null;
            }

            byte[] shaderOut;

            // Write out the effect to a runtime format.
            try {
                using var stream = new MemoryStream();
                using (var writer = new BinaryWriter(stream)) {
                    effect.Write(writer, options);
                }

                shaderOut = stream.ToArray();
            } catch (Exception ex) {
                Logger.Warn(ex, "Failed to write out shader data.");

                return null;
            }


            Logger.Info($"Successfully compiled shader '{shaderPath}' ({shaderOut.Length} bytes).");

            return shaderOut;
        }

    }

}