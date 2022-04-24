using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.Utility;
using BhModule.Community.Pathing.Utility.ColorThief;
using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework.Graphics;
using TmfLib.Prototype;
using Color = Microsoft.Xna.Framework.Color;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_TEXTURE = "texture";

        private AsyncTexture2D _texture;
        public AsyncTexture2D Texture {
            get => _texture;
            set {
                if (_texture != null) {
                    _texture.TextureSwapped -= ResampleTexture;
                }

                _texture = value;

                if (_texture == null) return;

                _texture.TextureSwapped += ResampleTexture;
                ResampleTexture(null, null);

                FadeIn();
            }
        }

        private void ResampleTexture(object sender, ValueChangedEventArgs<Texture2D> e) {
            if (this.Texture != null && this.TrailSampleColor == Color.White) {
                List<QuantizedColor> palette = ColorThief.GetPalette(this.Texture);
                palette.Sort((color, color2) => color2.Population.CompareTo(color.Population));

                Color? dominantColor = palette.FirstOrDefault()?.Color;

                if (dominantColor != null) {
                    this.TrailSampleColor = dominantColor.Value;
                }
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
                        if (!textureTaskResult.IsFaulted && textureTaskResult.Result != null) {
                            this.Texture = textureTaskResult.Result;
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
