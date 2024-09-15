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

        private Color _trailSampleColor = Color.White;
        private Color _glowBeadColor    = Color.Black;
        public Color TrailSampleColor {
            get => _trailSampleColor;
            set {
                _trailSampleColor = value;
                // A way to theoretically change it to white or black based off of color brightness. Didn't get it to work well so I switched to color inversion.
                // Leaving this here in case someone wants to fix it up since color inversion isn't the best.
                // Note you'll also have to uncomment GlowTrailColorBiasPercentage in AdvancedDefaults too
                //float luma        = 0.2126f * value.R + 0.7152f * value.G + 0.0722f * value.B; // result is 0-255
                //var bias     = luma > 128 ? Color.Black : Color.White;
                //_glowBeadColor    = Color.Lerp(value, bias, _packState.UserResourceStates.Advanced.GlowTrailColorBiasPercent);
                _glowBeadColor = new Color(255 - value.R, 255 - value.G, 255 - value.B);
            }
        }

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
