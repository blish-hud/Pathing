using System.ComponentModel;
using System.Runtime.CompilerServices;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using TmfLib;
using AttributeCollection = TmfLib.Prototype.AttributeCollection;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardMarker {

        private const string ATTR_ICONFILE = "iconfile";

        private Texture2D _texture;

        [JsonIgnore]
        public Texture2D Texture {
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
        private void Populate_IconFile(AttributeCollection collection, IPackResourceManager resourceManager) {
            {
                if (collection.TryPopAttribute(ATTR_ICONFILE, out var attribute)) {
                    attribute.GetValueAsTextureAsync(resourceManager).ContinueWith((textureTaskResult) => {
                        if (!textureTaskResult.IsFaulted && textureTaskResult.Result != null) {
                            this.Texture = textureTaskResult.Result;
                        } else {
                            this.Texture = ContentService.Textures.Error;
                            Logger.Warn($"Marker '{this.Guid}' failed to load texture '{attribute.GetValueAsString()}'");
                        }
                    });
                } else {
                    this.Texture = ContentService.Textures.Error;
                    Logger.Warn($"Markers '{this.Guid}' is missing '{ATTR_ICONFILE}' attribute.");
                }
            }
            
            // TODO: Show new missing-texture.png instead of the built-in error texture.
        }

    }
}
