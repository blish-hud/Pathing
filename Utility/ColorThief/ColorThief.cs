/*
 *  This code is adapted from ColorThief:
 *  https://github.com/KSemenenko/ColorThief/blob/e3d52d1d84944437696d89945ec61bf1b102da2f/ColorThieft.Shared/ColorThief.cs
 *  https://github.com/KSemenenko/ColorThief/blob/e3d52d1d84944437696d89945ec61bf1b102da2f/ColorThief.Desktop.v45/ColorThief.cs
 *
 *  The MIT License (MIT)
 *
 *  Copyright (c) 2015 Kos
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.  
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BhModule.Community.Pathing.Utility.ColorThief {

    public static class ColorThief {

        private const int  DEFAULT_COLOR_COUNT  = 5;
        private const int  DEFAULT_QUALITY      = 10;
        private const bool DEFAULT_IGNORE_WHITE = true;

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors.
        /// </summary>
        /// <param name="sourceImage">The source image.</param>
        /// <param name="colorCount">The color count.</param>
        /// <param name="quality">
        ///     1 is the highest quality settings. 10 is the default. There is
        ///     a trade-off between quality and speed. The bigger the number,
        ///     the faster a color will be returned but the greater the
        ///     likelihood that it will not be the visually most dominant color.
        /// </param>
        /// <param name="ignoreWhite">if set to <c>true</c> [ignore white].</param>
        /// <returns></returns>
        /// <code>true</code>
        public static List<QuantizedColor> GetPalette(Texture2D sourceImage, int colorCount = DEFAULT_COLOR_COUNT, int quality = DEFAULT_QUALITY, bool ignoreWhite = DEFAULT_IGNORE_WHITE) {
            var pixelArray = GetPixelsFast(sourceImage, quality, ignoreWhite);
            var cmap       = GetColorMap(pixelArray, colorCount);

            if (cmap != null) {
                var colors = cmap.GeneratePalette();
                return colors;
            }

            return new List<QuantizedColor>();
        }

        private static byte[][] GetPixelsFast(Texture2D sourceImage, int quality, bool ignoreWhite) {
            if (quality < 1) {
                quality = DEFAULT_QUALITY;
            }

            var pixels     = GetIntFromPixel(sourceImage);
            var pixelCount = sourceImage.Width * sourceImage.Height;

            return ConvertPixels(pixels, pixelCount, quality, ignoreWhite);
        }

        private static Color[] GetIntFromPixel(Texture2D texture) {
            var colors1D = new Color[texture.Width * texture.Height];
            texture.GetData(colors1D);
            return colors1D;
        }

        /// <summary>
        ///     Use the median cut algorithm to cluster similar colors.
        /// </summary>
        /// <param name="pixelArray">Pixel array.</param>
        /// <param name="colorCount">The color count.</param>
        /// <returns></returns>
        private static CMap GetColorMap(byte[][] pixelArray, int colorCount) {
            // Send array to quantize function which clusters values using median
            // cut algorithm

            if (colorCount > 0) {
                --colorCount;
            }

            var cmap = Mmcq.Quantize(pixelArray, colorCount);
            return cmap;
        }

        private static byte[][] ConvertPixels(Color[] pixels, int pixelCount, int quality, bool ignoreWhite) {

            var expectedDataLength = pixelCount;

            if (expectedDataLength != pixels.Length) {
                throw new ArgumentException($"(expectedDataLength = {expectedDataLength}) != (pixels.length = {pixels.Length})");
            }

            // Store the RGB values in an array format suitable for quantize
            // function

            // numRegardedPixels must be rounded up to avoid an
            // ArrayIndexOutOfBoundsException if all pixels are good.

            var numRegardedPixels = (pixelCount + quality - 1) / quality;

            var numUsedPixels = 0;
            var pixelArray    = new byte[numRegardedPixels][];

            for (var i = 0; i < pixelCount; i += quality) {
                var color = pixels[i];
                var b     = color.B;
                var g     = color.G;
                var r     = color.R;
                var a     = color.A;

                // If pixel is mostly opaque and not white
                if (a >= 125 && !(ignoreWhite && r > 250 && g > 250 && b > 250)) {
                    pixelArray[numUsedPixels] = new[] {
                        r, g, b
                    };

                    numUsedPixels++;
                }
            }

            // Remove unused pixels from the array
            var copy = new byte[numUsedPixels][];
            Array.Copy(pixelArray, copy, numUsedPixels);
            return copy;
        }

    }

}