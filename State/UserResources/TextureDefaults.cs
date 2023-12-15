using System;
using System.IO;
using System.Threading.Tasks;
using BhModule.Community.Pathing.Utility;
using Blish_HUD;
using Blish_HUD.Content;

namespace BhModule.Community.Pathing.State.UserResources {

    /// <summary>
    /// User customizable textures.
    /// </summary>
    public class TextureDefaults {

        private static readonly Logger Logger = Logger.GetLogger<TextureDefaults>();

        private const string TRAIL_NAME  = "defaultTrail.png";
        private const string MARKER_NAME = "defaultMarker.png";

        public AsyncTexture2D DefaultTrailTexture { get; set; }

        public AsyncTexture2D DefaultMarkerTexture { get; set; }

        public TextureDefaults() {
            this.DefaultTrailTexture  = new AsyncTexture2D(ContentService.Textures.Error);
            this.DefaultMarkerTexture = new AsyncTexture2D(ContentService.Textures.Error);
        }

        public async Task Load(string userResourceDir) {
            string trailOut  = Path.Combine(userResourceDir, TRAIL_NAME);
            string markerOut = Path.Combine(userResourceDir, MARKER_NAME);

            if (!File.Exists(trailOut)) {
                try {
                    var trailStream = PathingModule.Instance.ContentsManager.GetFileStream(@"png\defaults\trail.png");
                    await FileUtil.WriteStreamAsync(trailOut, trailStream);
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to write default trail to path {filePath}.", trailOut);
                }
            }

            if (!File.Exists(markerOut)) {
                try {
                    var markerStream = PathingModule.Instance.ContentsManager.GetFileStream(@"png\defaults\marker.png");
                    await FileUtil.WriteStreamAsync(markerOut, markerStream);
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to write default trail to path {filePath}.", markerOut);
                }
            }

            GameService.Graphics.QueueMainThreadRender((graphicsDevice) => {
                try {
                    var rawTrail = File.Open(trailOut, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var markerTrail = File.Open(markerOut, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    this.DefaultTrailTexture.SwapTexture(TextureUtil.FromStreamPremultiplied(graphicsDevice, rawTrail));
                    this.DefaultMarkerTexture.SwapTexture(TextureUtil.FromStreamPremultiplied(graphicsDevice, markerTrail));
                } catch (Exception ex) {
                    Logger.Warn(ex, "Failed to load default textures from file.");
                }
            });
        }

    }
}
