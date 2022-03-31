using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing.Utility {
    public static class FileUtil {

        // Avoids a subtle bug if we ever support other platforms natively.
        private const string NEWLINE = "\r\n";

        public static async Task<byte[]> ReadAsync(string path) {
            using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            byte[] result    = new byte[(int)stream.Length];
            int    readIndex = 0;

            while (readIndex < result.Length) {
                readIndex += await stream.ReadAsync(result, readIndex, result.Length - readIndex);
            }

            return result;
        }

        public static async Task<string> ReadTextAsync(string path, Encoding encoding = null) {
            encoding ??= Encoding.UTF8;

            return encoding.GetString(await ReadAsync(path));
        }

        public static async Task<string[]> ReadLinesAsync(string path, Encoding encoding = null) {
            return (await ReadTextAsync(path, encoding)).Split(new[] { NEWLINE }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static async Task WriteBytesAsync(string path, byte[] data) {
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, FileOptions.Asynchronous);
            await fileStream.WriteAsync(data, 0, data.Length);
        }

        public static async Task WriteStreamAsync(string path, Stream stream) {
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write, 4096, FileOptions.Asynchronous);
            await stream.CopyToAsync(fileStream);
        }

        public static async Task WriteTextAsync(string path, string text, Encoding encoding = null) {
            encoding ??= Encoding.UTF8;

            await WriteBytesAsync(path, encoding.GetBytes(text));
        }

        public static async Task WriteLinesAsync(string path, IEnumerable<string> lines, Encoding encoding = null) {
            await WriteTextAsync(path, string.Join(NEWLINE, lines), encoding);
        }

    }
}
