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

                this.Size = new Vector2(WorldUtil.GameToWorldCoord(_texture.Width  / 2f),
                                        WorldUtil.GameToWorldCoord(_texture.Height / 2f));

                this.VerticalConstraint = _texture.Height == _texture.Width
                                              ? BillboardVerticalConstraint.CameraPosition
                                              : BillboardVerticalConstraint.PlayerPosition;

                this.FadeIn();
            }
        }

        [Browsable(false)]
        public BillboardVerticalConstraint VerticalConstraint { get; set; }

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

                    //var texture = attribute.GetValueAsTexture(resourceManager);

                    //if (texture != null) {
                    //    this.Texture = texture;
                    //} else {
                    //    Logger.Warn($"Marker '{this.Guid}' missing texture '{attribute.GetValueAsString()}'");
                    //}
                } else {
                    Logger.Warn($"Markers '{this.Guid}' is missing {ATTR_ICONFILE} attribute.");
                }
            }
        }

    }
}
