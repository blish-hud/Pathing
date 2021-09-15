/*
 *  This code is adapted from ColorThief (https://github.com/KSemenenko/ColorThief/blob/e3d52d1d84944437696d89945ec61bf1b102da2f/ColorThieft.Shared/QuantizedColor.cs)
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
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Utility.ColorThief {

    public class QuantizedColor {

        public QuantizedColor(Color color, int population) {
            Color      = color;
            Population = population;
            IsDark     = CalculateYiqLuma(color) < 128;
        }

        public Color Color      { get; private set; }
        public int   Population { get; private set; }
        public bool  IsDark     { get; private set; }

        public int CalculateYiqLuma(Color color) {
            return Convert.ToInt32(Math.Round((299 * color.R + 587 * color.G + 114 * color.B) / 1000f));
        }

    }

}