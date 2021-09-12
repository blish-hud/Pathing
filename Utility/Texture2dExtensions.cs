using System.Drawing;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Utility {

    /// <remarks>
    /// Modified from https://stackoverflow.com/a/12495674
    /// </remarks>
    public static class Texture2dExtensions {

        public static Bitmap TextureToBitmap(this Texture2D texture) {
            using var memoryStream = new MemoryStream();

            texture.SaveAsPng(memoryStream, texture.Width, texture.Height);

            // Go to the beginning of the stream.
            memoryStream.Seek(0, SeekOrigin.Begin);

            // Create the image based on the stream.
            return new Bitmap(memoryStream);
        }

    }

}