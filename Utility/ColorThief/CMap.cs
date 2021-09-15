/*
 *  This code is adapted from ColorThief (https://github.com/KSemenenko/ColorThief/blob/e3d52d1d84944437696d89945ec61bf1b102da2f/ColorThieft.Shared/CMap.cs)
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Utility.ColorThief {

    /// <summary>
    ///     Color map
    /// </summary>
    internal class CMap {

        private readonly List<VBox>           vboxes = new();
        private          List<QuantizedColor> palette;

        public void Push(VBox box) {
            palette = null;
            vboxes.Add(box);
        }

        public List<QuantizedColor> GeneratePalette() {
            if (palette == null) {
                palette = (from vBox in vboxes
                           let rgb = vBox.Avg(false)
                           let color = FromRgb(rgb[0], rgb[1], rgb[2])
                           select new QuantizedColor(color, vBox.Count(false))).ToList();
            }

            return palette;
        }

        public Color FromRgb(int red, int green, int blue) {
            var color = new Color {
                A = 255,
                R = (byte) red,
                G = (byte) green,
                B = (byte) blue
            };

            return color;
        }

    }

}