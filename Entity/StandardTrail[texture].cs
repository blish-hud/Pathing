using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TmfLib;
using TmfLib.Prototype;

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
                    this.TrailSampleColor = this.Texture.SampleN(_packState.UserResourceStates.Static.MapTrailColorSamples);
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
