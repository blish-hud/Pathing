using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using BhModule.Community.Pathing.Utility.ColorThief;
using Blish_HUD;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;
using TmfLib.Prototype;
using Color = Microsoft.Xna.Framework.Color;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const string ATTR_TEXTURE = "texture";

        private Texture2D _texture;
        public Texture2D Texture {
            get => _texture;
            set {
                _texture = value;

                if (_texture == null) return;

                if (this.Texture != null && this.TrailSampleColor == Color.White) {
                    List<QuantizedColor> palette = ColorThief.GetPalette(this.Texture);
                    palette.Sort((color, color2) => color2.Population.CompareTo(color.Population));
                    
                    Color? dominantColor = palette.FirstOrDefault()?.Color;

                    if (dominantColor != null) {
                        this.TrailSampleColor = dominantColor.Value;
                    }
                }

                FadeIn();
            }
        }

        public Color TrailSampleColor { get; set; } = Color.White;

        /// <summary>
        /// texture
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Texture(AttributeCollection collection, IPackResourceManager resourceManager) {
            {
                if (collection.TryGetAttribute(ATTR_TEXTURE, out var attribute)) {
                    attribute.GetValueAsTextureAsync(resourceManager).ContinueWith((textureTaskResult) => {
                        if (!textureTaskResult.IsFaulted && textureTaskResult.Result != null) {
                            this.Texture = textureTaskResult.Result;
                        } else {
                            this.Texture = ContentService.Textures.Error;
                            Logger.Warn($"Trail failed to load texture '{attribute.GetValueAsString()}'");
                        }
                    });
                } else {
                    this.Texture = ContentService.Textures.Error;
                    Logger.Warn($"Trail is missing '{ATTR_TEXTURE}' attribute.");
                }
            }
        }

    }
}
