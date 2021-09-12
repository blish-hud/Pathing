using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using ColorThiefDotNet;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;
using TmfLib.Prototype;
using Color = Microsoft.Xna.Framework.Color;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        private const           string     ATTR_TEXTURE  = "texture";
        private static readonly ColorThief _colorThief   = new();

        private Texture2D _texture;
        public Texture2D Texture {
            get => _texture;
            set {
                _texture = value;

                if (_texture == null) return;

                if (this.Texture != null && this.TrailSampleColor == Color.White) {
                    List<QuantizedColor> palette = _colorThief.GetPalette(this.Texture.TextureToBitmap());
                    palette.Sort((color, color2) => color2.Population.CompareTo(color.Population));
                    
                    ColorThiefDotNet.Color? dominantColor = palette.FirstOrDefault()?.Color;

                    if (dominantColor != null) {
                        this.TrailSampleColor = new Color(dominantColor.Value.R, dominantColor.Value.G, dominantColor.Value.B, dominantColor.Value.A);
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
