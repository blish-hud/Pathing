using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Blish_HUD;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;

namespace BhModule.Community.Pathing.Content {
    public class TextureResourceManager : IPackResourceManager {

        private static readonly Texture2D _textureFailedToLoad = PathingModule.Instance.ContentsManager.GetTexture(@"png\missing-texture.png");

        private static readonly ConcurrentDictionary<IPackResourceManager, TextureResourceManager> _textureResourceManagerLookup = new();

        public static TextureResourceManager GetTextureResourceManager(IPackResourceManager referencePackResourceManager) {
            return _textureResourceManagerLookup.GetOrAdd(referencePackResourceManager, (packResourceManager) => new TextureResourceManager(packResourceManager));
        }

        public static async Task UnloadAsync() {
            var managers = _textureResourceManagerLookup.Values;

            foreach (var manager in managers) {
                await manager.ClearCache();
            }
        }

        private readonly ConcurrentDictionary<string, TaskCompletionSource<Texture2D>> _textureCache = new(StringComparer.OrdinalIgnoreCase);

        private readonly IPackResourceManager _packResourceManager;

        private TextureResourceManager(IPackResourceManager packResourceManager) {
            _packResourceManager = packResourceManager;
        }

        public bool ResourceExists(string resourcePath) {
            return _packResourceManager.ResourceExists(resourcePath);
        }

        public async Task<byte[]> LoadResourceAsync(string resourcePath) {
            return await _packResourceManager.LoadResourceAsync(resourcePath);
        }

        public async Task<Stream> LoadResourceStreamAsync(string resourcePath) {
            return await _packResourceManager.LoadResourceStreamAsync(resourcePath);
        }

        private static void LoadTexture(TaskCompletionSource<Texture2D> textureTcs, Stream textureStream) {
            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => {
                if (textureStream == null) {
                    textureTcs.SetResult(_textureFailedToLoad);
                    return;
                }

                try {
                    // TODO: Move the blending to the shader so that we don't have to slow load these.
                    textureTcs.SetResult(TextureUtil.FromStreamPremultiplied(graphicsDevice, textureStream));
                } catch (Exception) {
                    textureTcs.SetResult(_textureFailedToLoad);
                }
            });
        }

        public async Task PreloadTexture(string texturePath) {
            if (!_textureCache.ContainsKey(texturePath)) {
                var textureTcs = new TaskCompletionSource<Texture2D>();

                _textureCache[texturePath] = textureTcs;

                await Task.Yield();
                LoadTexture(textureTcs, await LoadResourceStreamAsync(texturePath));
            }
        }

        public async Task<Texture2D> LoadTextureAsync(string texturePath) {
            return await _textureCache[texturePath].Task;
        }

        public async Task ClearCache() {
            var textureCollection = _textureCache.Values;
            _textureCache.Clear();

            var textures = new List<Texture2D>(textureCollection.Count);

            foreach (var resource in textureCollection) {
                textures.Add(await resource.Task);
            }

            // Ensure we don't dispose textures outside of the main thread.
            GameService.Overlay.QueueMainThreadUpdate((_) => {
                foreach (var texture in textures) {
                    texture.Dispose();
                }
            });
        }

    }
}
