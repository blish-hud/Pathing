using BhModule.Community.Pathing.Entity;
using Blish_HUD.Content;
using Blish_HUD;
using System;
using BhModule.Community.Pathing.Behavior;
using Neo.IronLua;
using BhModule.Community.Pathing.State;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using BhModule.Community.Pathing.Scripting.Lib;

namespace BhModule.Community.Pathing.Scripting.Extensions {
    internal static class StandardTrailScriptExtensions {

        private static PackInitiator _packInitiator;

        internal static void SetPackInitiator(PackInitiator packInitiator) {
            _packInitiator = packInitiator;
        }

        // Points

        public static void SetPoints(this StandardTrail trail, LuaTable points) {
            var vpoints = points.ArrayList.Select(a => (Vector3)a).ToArray();

            var trailSections = new List<Vector3[]>(1) {
                trail.PostProcessing_DouglasPeucker(vpoints, _packInitiator.PackState.UserResourceStates.Advanced.MapTrailDouglasPeuckerError).ToArray()
            };

            trail._sectionPoints = trailSections.ToArray();

            trail.BuildBuffers(vpoints);
        }

        // Remove

        public static void Remove(this StandardTrail trail) {
            trail.Unload();
            _packInitiator.PackState.RemovePathingEntity(trail);
        }
        
        // Texture

        public static void SetTexture(this StandardTrail trail, string texturePath) {
            trail.Texture = Instance.Texture(trail.TextureResourceManager, texturePath);
        }

        public static void SetTexture(this StandardTrail trail, int textureId) {
            // Should match what is in Instance.cs > Texture
            trail.Texture = AsyncTexture2D.FromAssetId(textureId) ?? ContentService.Textures.Error;
        }

        // Behavior

        public static IBehavior GetBehavior(this StandardTrail trail, string behaviorName) {
            foreach (var behavior in trail.Behaviors) {
                if (string.Equals(behavior.GetType().Name, behaviorName, StringComparison.InvariantCultureIgnoreCase)) {
                    return behavior;
                }
            }

            return null;
        }

    }
}
