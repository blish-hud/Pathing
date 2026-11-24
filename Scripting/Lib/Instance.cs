using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BhModule.Community.Pathing.Content;
using BhModule.Community.Pathing.Entity;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;
using Microsoft.Xna.Framework;
using Neo.IronLua;
using TmfLib.Attributes;
using TmfLib.Model;
using TmfLib.Parsing;

namespace BhModule.Community.Pathing.Scripting.Lib {
    public class Instance {

        private readonly PathingGlobal _global;

        internal Instance(PathingGlobal global) {
            _global = global;
        }

        // Vector3

        public Vector3 Vector3(float x, float y, float z) {
            return new Vector3(x, y, z);
        }

        // Color

        public Color Color(int r, int g, int b, int a = 255) {
            return new Color(r, g, b, a);
        }

        // Pathables

        private PathableSource PathableSourceFromLuaTable(PathableKind kind, IPackResourceManager resourceManager, LuaTable luaTable) {
            var attributes = luaTable != null
                                 ? AttributeSet.From(luaTable.Members.Select(member => new PackAttribute(AttrKeys.Get(member.Key), string.Format(CultureInfo.InvariantCulture, "{0}", member.Value))).ToList())
                                 : AttributeSet.Empty;

            var rootCategory = _global.ScriptEngine.Module.PackInitiator.PackState.RootCategory;

            PathingCategory category = null;
            if (rootCategory != null && attributes.TryGetValue(AttrKeys.Type, out string type)) {
                category = rootCategory.GetOrAdd(type);
            }

            int mapId = -1;
            string mapIdValue = attributes.GetValueOrDefault(AttrKeys.MapId) ?? category?.ResolvedAttributes.GetValueOrDefault(AttrKeys.MapId);
            if (mapIdValue != null) ValueParser.TryParseInt(mapIdValue, out mapId);

            var reader = new AttributeReader();
            reader.Reset(attributes.Without(AttrKeys.Type), category);

            return new PathableSource(kind, category, mapId, resourceManager, reader);
        }

        // Marker

        public StandardMarker Marker(IPackResourceManager resourceManager, LuaTable attributes = null) {
            var marker = _global.ScriptEngine.Module.PackInitiator.PackState.InitPathable(PathableSourceFromLuaTable(PathableKind.Marker, resourceManager, attributes)) as StandardMarker;

            if (marker.MapId < 0) {
                // Map ID can be assumed since we only show current map icons anyways, but UI filtering doesn't know that.
                marker.MapId = GameService.Gw2Mumble.CurrentMap.Id;
            }

            return marker;
        }

        // Trail

        public StandardTrail Trail(IPackResourceManager resourceManager, LuaTable attributes = null) {
            var trail = _global.ScriptEngine.Module.PackInitiator.PackState.InitPathable(PathableSourceFromLuaTable(PathableKind.Trail, resourceManager, attributes)) as StandardTrail;

            if (trail.MapId < 0) {
                // Map ID can be assumed since we only show current map icons anyways, but UI filtering doesn't know that.
                trail.MapId = GameService.Gw2Mumble.CurrentMap.Id;
            }

            return trail;
        }

        // Guid

        public Guid Guid(string base64) {
            return AttributeParsingUtil.InternalGetValueAsGuid(base64);
        }

        // Texture

        internal static AsyncTexture2D Texture(TextureResourceManager textureManager, string texturePath) {
            var outTexture = new AsyncTexture2D();
            // If it's a texture from a different map (or no map at all), we must ensure it's preloaded.
            // We don't need to await this method because the texture dictionary is updated before the first yield.
            textureManager.PreloadTexture(texturePath, false);
            textureManager.LoadTextureAsync(texturePath).ContinueWith(textureTaskResult => {
                if (!textureTaskResult.IsFaulted && textureTaskResult.Result.Texture != null) {
                    outTexture.SwapTexture(textureTaskResult.Result.Texture);
                } else {
                    // TODO: Probably should report this with a logger.
                }
            });

            return outTexture;
        }

        public AsyncTexture2D Texture(int textureId) {
            return AsyncTexture2D.FromAssetId(textureId) ?? ContentService.Textures.Error;
        }

        public AsyncTexture2D Texture(PackContext pack, string texturePath) {
            var textureManager = TextureResourceManager.GetTextureResourceManager(pack.ResourceManager);

            return Texture(textureManager, texturePath);
        }

    }
}
