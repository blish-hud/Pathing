using BhModule.Community.Pathing.Behavior;
using BhModule.Community.Pathing.Entity;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Scripting.Extensions {
    internal static class StandardMarkerScriptExtensions {

        private static PackInitiator _packInitiator;

        internal static void SetPackInitiator(PackInitiator packInitiator) {
            _packInitiator = packInitiator;
        }

        // Position

        public static void SetPos(this StandardMarker marker, float x, float y, float z) {
            marker.Position = new Vector3(x, y, z);
        }

        public static void SetPos(this StandardMarker marker, Vector3 position) {
            marker.Position = position;
        }

        public static void SetPosX(this StandardMarker marker, float x) {
            SetPos(marker, x, marker.Position.Y, marker.Position.Z);
        }

        public static void SetPosY(this StandardMarker marker, float y) {
            SetPos(marker, marker.Position.X, y, marker.Position.Z);
        }

        public static void SetPosZ(this StandardMarker marker, float z) {
            SetPos(marker, marker.Position.X, marker.Position.Y, z);
        }

        // Rotation

        public static void SetRot(this StandardMarker marker, float x, float y, float z) {
            marker.RotationXyz = new Vector3(x, y, z);
        }

        public static void SetRot(this StandardMarker marker, Vector3 rotation) {
            marker.RotationXyz = rotation;
        }

        public static void SetRotX(this StandardMarker marker, float x) {
            SetRot(marker, x, marker.RotationXyz.GetValueOrDefault(Vector3.Zero).Y, marker.RotationXyz.GetValueOrDefault(Vector3.Zero).Z);
        }

        public static void SetRotY(this StandardMarker marker, float y) {
            SetRot(marker, marker.RotationXyz.GetValueOrDefault(Vector3.Zero).X, y, marker.RotationXyz.GetValueOrDefault(Vector3.Zero).Z);
        }

        public static void SetRotZ(this StandardMarker marker, float z) {
            SetRot(marker, marker.RotationXyz.GetValueOrDefault(Vector3.Zero).X, marker.RotationXyz.GetValueOrDefault(Vector3.Zero).Y, z);
        }

        // Remove

        public static void Remove(this StandardMarker marker) {
            marker.Unload();
            _packInitiator.PackState.RemovePathingEntity(marker);
        }

        // Texture

        public static void SetTexture(this StandardMarker marker, string texturePath) {
            // If it's a texture from a different map (or no map at all), we must ensure it's preloaded.
            // We don't need to await this method because the texture dictionary is updated before the first yield.
            marker.TextureResourceManager.PreloadTexture(texturePath, false);
            marker.TextureResourceManager.LoadTextureAsync(texturePath).ContinueWith(textureTaskResult => {
                if (!textureTaskResult.IsFaulted && textureTaskResult.Result.Texture != null) {
                    marker.Texture = textureTaskResult.Result.Texture;
                } else {
                    // TODO: Probably should report this with a logger.
                }
            });
        }

        // Behavior

        public static IBehavior GetBehavior(this StandardMarker marker, string behaviorName) {
            foreach (var behavior in marker.Behaviors) {
                if (string.Equals(behavior.GetType().Name, behaviorName, System.StringComparison.InvariantCultureIgnoreCase)) {
                    return behavior;
                }
            }

            return null;
        }

    }
}
