using System;
using System.Drawing.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Utility {

    /// <remarks>
    /// Modified from https://stackoverflow.com/a/24101321
    /// </remarks>
    public static class Texture2dExtensions {

        public static Color GetPixel(this Texture2D texture, int x, int y) {
            return GetPixels(texture)[x + (y * texture.Width)];
        }

        public static Color SampleN(this Texture2D texture, int n) {
            int samples = Math.Min(n, texture.Width - 1);

            Color[] pixels = GetPixels(texture);

            int inc = texture.Width / (samples + 1);

            var winner = Color.White;
            int score  = int.MinValue;

            for (int i = 1; i <= samples; i++) {
                var pixel = pixels[(i * inc) + (texture.Height / 2 * texture.Width)];

                int sampleScore = GetSampleScore(pixel);

                if (sampleScore > score) {
                    score  = sampleScore;
                    winner = pixel;
                }
            }

            return winner;
        }

        private static int GetSampleScore(Color sample) {
            if (sample.R == 254 && sample.G == 248) {
                // I hate this stupid yellow color.
                return int.MinValue + 1;
            }

            int scoreR = 128 - Math.Abs(sample.R - 127);
            int scoreG = 128 - Math.Abs(sample.G - 127);
            int scoreB = 128 - Math.Abs(sample.B - 127);

            return scoreR + scoreG + scoreB + sample.A;
        }

        private static Color[] GetPixels(Texture2D texture) {
            var colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);
            return colors1D;
        }

    }

}
