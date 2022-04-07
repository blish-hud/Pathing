using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Blish_HUD;
using Flurl;
using Flurl.Http;
using TmfLib.Content;

namespace BhModule.Community.Pathing.Content {
    public class WebReader : IDataReader {

        private static readonly Logger Logger = Logger.GetLogger<WebReader>();

        private const string ENTRIES_ENDPOINT = "entries.json";

        private readonly string _baseUrl;

        private HashSet<string> _entries = null;

        public WebReader(string baseUrl) {
            _baseUrl = baseUrl;
        }

        public async Task InitWebReader() {
            _entries = await GetEntries();
        }

        private async Task<HashSet<string>> GetEntries() {
            var entriesUrl = _baseUrl.AppendPathSegment(ENTRIES_ENDPOINT);

            var entrySet = new HashSet<string>();

            try {
                foreach (var entry in await entriesUrl.GetJsonAsync<string[]>()) {
                    entrySet.Add(entry.Replace('\\', '/'));
                }
            } catch (Exception ex) {
                Logger.Warn(ex, $"Failed to load {entriesUrl}.");
                return new HashSet<string>(0);
            }

            return entrySet;
        }

        public void Dispose() {
            // NOOP
        }

        public IDataReader GetSubPath(string subPath) {
            return new WebReader(Url.Combine(_baseUrl, subPath));
        }

        public string GetPathRepresentation(string relativeFilePath = null) {
            return Url.Combine(_baseUrl, relativeFilePath ?? string.Empty);
        }

        private void ThrowIfNoInit() {
            if (_entries == null) {
                throw new InvalidOperationException($"{nameof(InitWebReader)}() must be called before any calls can be made to a {nameof(WebReader)}.");
            }
        }

        private string GetCaseSensitiveEntryUri(string filePath) {
            ThrowIfNoInit();

            foreach (var entry in _entries) {
                if (string.Equals(filePath, entry, StringComparison.InvariantCultureIgnoreCase)) {
                    return entry;
                }
            }

            return filePath;
        }

        public async Task LoadOnFileTypeAsync(Func<Stream, IDataReader, Task> loadFileFunc, string fileExtension = "", IProgress<string> progress = null) {
            ThrowIfNoInit();

            string[] validEntries = _entries.Where(e => e.EndsWith(fileExtension.ToLowerInvariant())).ToArray();

            foreach (string entry in validEntries) {
                progress?.Report($"Loading {entry}...");
                await loadFileFunc.Invoke(await this.GetFileStreamAsync(entry), this);
            }
        }

        public bool FileExists(string filePath) {
            ThrowIfNoInit();

            return true;

            //return _entries.Contains(GetCaseSensitiveEntryUri(filePath));
        }

        public Stream GetFileStream(string filePath) {
            throw new InvalidOperationException();
        }

        public byte[] GetFileBytes(string filePath) {
            throw new InvalidOperationException();
        }

        private async Task<Stream> GetFileStreamAsyncWithRetry(string filePath, int attempts = 3) {
            try {
                return await Url.Combine(_baseUrl, GetCaseSensitiveEntryUri(filePath)).GetStreamAsync();
            } catch (Exception ex) {
                if (attempts > 0) {
                    Logger.Debug(ex, $"Failed to load {filePath}.  Retrying with {attempts} attempts left.");
                    return await GetFileStreamAsyncWithRetry(filePath, --attempts);
                } else {
                    Logger.Warn(ex, $"Failed to load {filePath}");
                }
            }

            return Stream.Null;
        }

        public async Task<Stream> GetFileStreamAsync(string filePath) {
            return await GetFileStreamAsyncWithRetry(filePath);
        }

        public async Task<byte[]> GetFileBytesAsync(string filePath) {
            var stream = await GetFileStreamAsync(GetCaseSensitiveEntryUri(filePath));

            using var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);

            return memStream.ToArray();
        }

        public void AttemptReleaseLocks() { /* NOOP */ }

    }
}
