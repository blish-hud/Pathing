using System.IO;
using System.Threading.Tasks;

namespace BhModule.Community.Pathing {
    public interface IPackResourceManager {

        bool ResourceExists(string resourcePath);

        Task<byte[]> LoadResourceAsync(string resourcePath);

        Task<Stream> LoadResourceStreamAsync(string resourcePath);

    }
}
