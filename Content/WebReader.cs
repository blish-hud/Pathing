using System;
using System.IO;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using TmfLib.Content;

namespace BhModule.Community.Pathing.Content {
    public class WebReader : IDataReader {

        private readonly string _baseUrl;

        public WebReader(string baseUrl) {
            _baseUrl = baseUrl;
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

        public void LoadOnFileType(Action<Stream, IDataReader> loadFileFunc, string fileExtension = "", IProgress<string> progress = null) {
            throw new InvalidOperationException();
        }

        public bool FileExists(string filePath) {
            throw new InvalidOperationException();
        }

        public Stream GetFileStream(string filePath) {
            throw new InvalidOperationException();
        }

        public byte[] GetFileBytes(string filePath) {
            throw new InvalidOperationException();
        }

        public async Task<Stream> GetFileStreamAsync(string filePath) {
            return await Url.Combine(_baseUrl, filePath).GetStreamAsync();
        }

        public async Task<byte[]> GetFileBytesAsync(string filePath) {
            var stream = await GetFileStreamAsync(filePath);

            using var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);

            return memStream.ToArray();
        }

    }
}
