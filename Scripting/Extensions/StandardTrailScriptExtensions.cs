using BhModule.Community.Pathing.Entity;
using Blish_HUD.Content;
using Blish_HUD;
using System;
using BhModule.Community.Pathing.Behavior;

namespace BhModule.Community.Pathing.Scripting.Extensions {
    internal static class StandardTrailScriptExtensions {

        private static PackInitiator _packInitiator;

        internal static void SetPackInitiator(PackInitiator packInitiator) {
            _packInitiator = packInitiator;
        }

        // Remove

        public static void Remove(this StandardTrail trail) {
            trail.Unload();
            _packInitiator.PackState.RemovePathingEntity(trail);
        }
        
        // Texture

        public static void SetTexture(this StandardTrail trail, string texturePath) {
            throw new NotImplementedException("This method has not been implemented yet.");
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
