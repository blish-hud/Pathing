using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.Utility;
using Blish_HUD.Content;
using TmfLib.Prototype;
using Color = Microsoft.Xna.Framework.Color;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_TEXTURE = "texture";

        private AsyncTexture2D _texture;
        public AsyncTexture2D Texture {
            get => _texture;
            set {
                _texture = value;

                if (_texture == null) return;

                FadeIn();
            }
        }

        public Color TrailSampleColor { get; set; } = Color.White;

        /// <summary>
        /// texture
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Texture(AttributeCollection collection, TextureResourceManager resourceManager) {
            {
                if (collection.TryGetAttribute(ATTR_TEXTURE, out var attribute)) {
                    attribute.GetValueAsTextureAsync(resourceManager).ContinueWith((textureTaskResult) => {
                        if (!textureTaskResult.IsFaulted && textureTaskResult.Result.Texture != null) {
                            this.Texture = textureTaskResult.Result.Texture;

                            if (this.TrailSampleColor == Color.White) {
                                this.TrailSampleColor = textureTaskResult.Result.Sample;
                            }
                        } else {
                            Logger.Warn("Trail failed to load texture '{trailTexture}'", attribute);
                        }
                    });
                } else {
                    this.Texture = _packState.UserResourceStates.Textures.DefaultTrailTexture;
                    Logger.Warn($"Trail is missing '{ATTR_TEXTURE}' attribute.");
                }
            }
        }

    }
}
