using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.Utility;
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
                _texture = value;

                if (_texture == null) return;

                this.FadeIn();
            }
        }

        /// <summary>
        /// iconfile
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_IconFile(AttributeCollection collection, TextureResourceManager resourceManager) {
            {
                if (collection.TryPopAttribute(ATTR_ICONFILE, out var attribute)) {
                    attribute.GetValueAsTextureAsync(resourceManager).ContinueWith((textureTaskResult) => {
                        if (!textureTaskResult.IsFaulted && textureTaskResult.Result.Texture != null) {
                            this.Texture = textureTaskResult.Result.Texture;
                        } else {
                            Logger.Warn("Marker failed to load texture '{markerTexture}'", attribute);
                        }
                    });
                } else {
                    this.Texture = _packState.UserResourceStates.Textures.DefaultMarkerTexture;
                    Logger.Warn($"Marker '{this.Guid.ToBase64String()}' is missing '{ATTR_ICONFILE}' attribute.");
                }
            }
        }

    }
}
