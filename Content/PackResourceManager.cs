using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TmfLib;
using TmfLib.Content;

namespace BhModule.Community.Pathing.Content {
    public sealed class PackResourceManager : IPackResourceManager {

        private static readonly ConditionalWeakTable<Pack, PackResourceManager> _managers = new();

        public Pack Pack { get; }

        private PackResourceManager(Pack pack) {
            this.Pack = pack;
        }

        public static PackResourceManager For(Pack pack) {
            return _managers.GetValue(pack, p => new PackResourceManager(p));
        }

        public bool ResourceExists(string resourcePath) {
            return this.Pack.Source.Exists(resourcePath);
        }

        public Task<byte[]> LoadResourceAsync(string resourcePath) {
            return Task.Run(() => {
                using (var buffer = this.Pack.Source.ReadAll(resourcePath)) {
                    return buffer?.ToArray();
                }
            });
        }

        public Task<Stream> LoadResourceStreamAsync(string resourcePath) {
            return Task.Run(() => this.Pack.Source.OpenRead(resourcePath));
        }

    }
}
