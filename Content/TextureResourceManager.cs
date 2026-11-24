using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility.ColorThief;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib.Content;

namespace BhModule.Community.Pathing.Content {
    public class TextureResourceManager : IPackResourceManager {

        private static readonly Texture2D _textureFailedToLoad = PathingModule.Instance.ContentsManager.GetTexture(@"png\missing-texture.png");
        private static readonly Color     _defaultSampleColor  = Color.DarkGray;

        private static readonly ConcurrentDictionary<IPackResourceManager, TextureResourceManager> _textureResourceManagerLookup = new();

        private static int _generation;

        public static TextureResourceManager GetTextureResourceManager(IPackResourceManager referencePackResourceManager) {
            return _textureResourceManagerLookup.GetOrAdd(referencePackResourceManager, (packResourceManager) => new TextureResourceManager(packResourceManager));
        }

        public static async Task UnloadAsync() {
            var managers = _textureResourceManagerLookup.Values;

            foreach (var manager in managers) {
                await manager.ClearCache();
            }
        }

        /// <summary>
        /// Starts tracking which textures are used so that those which aren't can be released with <see cref="EvictUnused"/>.
        /// </summary>
        public static void BeginGeneration() {
            Interlocked.Increment(ref _generation);
        }

        /// <summary>
        /// Releases textures that have not been used since <see cref="BeginGeneration"/> was last called.
        /// </summary>
        public static void EvictUnused() {
            int generation = Volatile.Read(ref _generation);

            foreach (var manager in _textureResourceManagerLookup.Values) {
                manager.Evict(generation);
            }
        }

        /// <summary>
        /// Releases all textures of a pack that is no longer loaded.
        /// </summary>
        public static async Task ReleaseAsync(IPackResourceManager packResourceManager) {
            if (_textureResourceManagerLookup.TryRemove(packResourceManager, out var manager)) {
                await manager.ClearCache();
            }
        }

        private readonly ConcurrentDictionary<string, TaskCompletionSource<(Texture2D Texture, Color Sample)>> _textureCache = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, int>                                                     _lastUsed     = new(StringComparer.Ordinal);

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
                } finally {
                    textureStream.Dispose();
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
            string key = PackPath.Normalize(texturePath);

            _lastUsed[key] = Volatile.Read(ref _generation);

            var textureTcs = new TaskCompletionSource<(Texture2D Texture, Color Sample)>();

            if (_textureCache.TryAdd(key, textureTcs)) {
                LoadTexture(textureTcs, await LoadResourceStreamAsync(texturePath), shouldSample);
            }
        }

        public async Task<(Texture2D Texture, Color Sample)> LoadTextureAsync(string texturePath) {
            string key = PackPath.Normalize(texturePath);

            if (_textureCache.TryGetValue(key, out var texture)) {
                _lastUsed[key] = Volatile.Read(ref _generation);
                return await texture.Task;
            }

            return (_textureFailedToLoad, _defaultSampleColor);
        }

        private void Evict(int generation) {
            var evicted = new List<TaskCompletionSource<(Texture2D Texture, Color Sample)>>();

            foreach (var entry in _lastUsed) {
                if (entry.Value >= generation) continue;

                if (_textureCache.TryRemove(entry.Key, out var texture)) {
                    evicted.Add(texture);
                }

                _lastUsed.TryRemove(entry.Key, out _);
            }

            if (evicted.Count > 0) {
                _ = DisposeAsync(evicted);
            }
        }

        public async Task ClearCache() {
            var textureCollection = _textureCache.Values.ToList();
            _textureCache.Clear();
            _lastUsed.Clear();

            await DisposeAsync(textureCollection);
        }

        private static async Task DisposeAsync(ICollection<TaskCompletionSource<(Texture2D Texture, Color Sample)>> textureCollection) {
            var texturePairs = new List<(Texture2D Texture, Color Sample)>(textureCollection.Count);

            foreach (var resource in textureCollection) {
                texturePairs.Add(await resource.Task);
            }

            // Ensure we don't dispose textures outside of the main thread.
            GameService.Overlay.QueueMainThreadUpdate((_) => {
                foreach (var texturePair in texturePairs) {
                    // The fallback texture is shared.
                    if (texturePair.Texture != _textureFailedToLoad) {
                        texturePair.Texture.Dispose();
                    }
                }
            });
        }

    }
}
