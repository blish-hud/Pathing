using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility.ColorThief;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;

namespace BhModule.Community.Pathing.Content {
    public class TextureResourceManager : IPackResourceManager {

        private static readonly Texture2D _textureFailedToLoad = PathingModule.Instance.ContentsManager.GetTexture(@"png\missing-texture.png");
        private static readonly Color     _defaultSampleColor  = Color.DarkGray;

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

        private readonly ConcurrentDictionary<string, TaskCompletionSource<(Texture2D Texture, Color Sample)>> _textureCache = new(StringComparer.OrdinalIgnoreCase);

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

        public async Task<IEnumerable<string>> GetResources(string extension = "") {
            return await _packResourceManager.GetResources(extension);
        }

        private static void LoadTexture(TaskCompletionSource<(Texture2D Texture, Color Sample)> textureTcs, Stream textureStream, bool shouldSample) {
            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => {
                if (textureStream == null) {
                    textureTcs.SetResult((_textureFailedToLoad, Color.DarkGray));
                    return;
                }

                try {
                    var texture = TextureUtil.FromStreamPremultiplied(graphicsDevice, textureStream);
                    var sample  = shouldSample ? SampleColor(texture) : _defaultSampleColor;

                    // TODO: Move the blending to the shader so that we don't have to slow load these.
                    textureTcs.SetResult((texture, sample));
                } catch (Exception) {
                    textureTcs.SetResult((_textureFailedToLoad, _defaultSampleColor));
                }
            });
        }

        private static Color SampleColor(Texture2D texture) {
            List<QuantizedColor> palette = ColorThief.GetPalette(texture);
            palette.Sort((color, color2) => color2.Population.CompareTo(color.Population));

            Color? dominantColor = palette.FirstOrDefault()?.Color;

            return dominantColor ?? _defaultSampleColor;

        }

        public async Task PreloadTexture(string texturePath, bool shouldSample) {
            if (!_textureCache.ContainsKey(texturePath)) {
                var textureTcs = new TaskCompletionSource<(Texture2D Texture, Color Sample)>();

                _textureCache[texturePath] = textureTcs;

                LoadTexture(textureTcs, await LoadResourceStreamAsync(texturePath), shouldSample);
            }
        }

        public async Task<(Texture2D Texture, Color Sample)> LoadTextureAsync(string texturePath) {
            if (_textureCache.TryGetValue(texturePath, out var texture)) {
                return await texture.Task;
            }

            return (_textureFailedToLoad, _defaultSampleColor);
        }

        public async Task ClearCache() {
            var textureCollection = _textureCache.Values;
            _textureCache.Clear();

            var texturePairs = new List<(Texture2D Texture, Color Sample)>(textureCollection.Count);

            foreach (var resource in textureCollection) {
                texturePairs.Add(await resource.Task);
            }

            // Ensure we don't dispose textures outside of the main thread.
            GameService.Overlay.QueueMainThreadUpdate((_) => {
                foreach (var texturePair in texturePairs) {
                    texturePair.Texture.Dispose();
                }
            });
        }

    }
}
