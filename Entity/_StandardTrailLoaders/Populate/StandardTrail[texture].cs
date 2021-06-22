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

        public Texture2D Texture { get; set; }

        public Color TrailSampleColor { get; set; } = Color.White;

        /// <summary>
        /// texture
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Populate_Texture(AttributeCollection collection, IPackResourceManager resourceManager) {
            {
                if (collection.TryGetAttribute(ATTR_TEXTURE, out var attribute)) {
                    this.Texture = attribute.GetValueAsTexture(resourceManager);

                    if (this.Tint != _packState.UserResourceStates.Population.TrailPopulationDefaults.Tint) {
                        this.TrailSampleColor = this.Tint;
                    } else if (this.Texture != null) {
                        this.TrailSampleColor = this.Texture.Sample24();
                    }
                } else {
                    this.Texture = ContentService.Textures.Error;
                }
            }
        }

    }
}
