using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;
using Newtonsoft.Json;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_ICONFILE = "iconfile";

        private AsyncTexture2D _texture;

        [JsonIgnore]
        public AsyncTexture2D Texture {
            get => _texture;
            set {
                if (_texture == null) this.FadeIn();

                _texture = value;
            }
        }

        /// <summary>
        /// iconfile
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_IconFile(AttributeCollection collection, TextureResourceManager resourceManager) {
            {
                if (collection.TryPopAttribute(ATTR_ICONFILE, out var attribute)) {
                    attribute.GetValueAsTextureAsync(resourceManager).ContinueWith(textureTaskResult => {
                        // We check that Texture is null in case a script has already executed and updated the Texture before this managed to finish.
                        if (!textureTaskResult.IsFaulted && textureTaskResult.Result.Texture != null && this.Texture == null) {
                            this.Texture = textureTaskResult.Result.Texture;
                        } else {
                            Logger.Warn("Marker failed to load texture '{markerTexture}'", attribute);
                        }
                    });
                } else if (this.Occlude) {
                    // An expected scenario where no texture is expected.
                    this.Texture = ContentService.Textures.TransparentPixel;
                } else {
                    this.Texture = _packState.UserResourceStates.Textures.DefaultMarkerTexture;
                    Logger.Debug($"Marker '{this.Guid.ToBase64String()}' is missing '{ATTR_ICONFILE}' attribute.");
                }
            }
        }

    }
}
