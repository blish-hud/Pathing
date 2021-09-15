/*
 *  This code is adapted from ColorThief (https://github.com/KSemenenko/ColorThief/blob/e3d52d1d84944437696d89945ec61bf1b102da2f/ColorThieft.Shared/VBox.cs)
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

namespace BhModule.Community.Pathing.Utility.ColorThief {

    /// <summary>
    ///     3D color space box.
    /// </summary>
    internal class VBox {

        private readonly int[] histo;
        private          int[] avg;
        public           int   B1;
        public           int   B2;
        private          int?  count;
        public           int   G1;
        public           int   G2;
        public           int   R1;
        public           int   R2;
        private          int?  volume;

        public VBox(int r1, int r2, int g1, int g2, int b1, int b2, int[] histo) {
            R1 = r1;
            R2 = r2;
            G1 = g1;
            G2 = g2;
            B1 = b1;
            B2 = b2;

            this.histo = histo;
        }

        public int Volume(bool force) {
            if (volume == null || force) {
                volume = (R2 - R1 + 1) * (G2 - G1 + 1) * (B2 - B1 + 1);
            }

            return volume.Value;
        }

        public int Count(bool force) {
            if (count == null || force) {
                var npix = 0;
                int i;

                for (i = R1; i <= R2; i++) {
                    int j;

                    for (j = G1; j <= G2; j++) {
                        int k;

                        for (k = B1; k <= B2; k++) {
                            var index = Mmcq.GetColorIndex(i, j, k);
                            npix += histo[index];
                        }
                    }
                }

                count = npix;
            }

            return count.Value;
        }

        public VBox Clone() {
            return new VBox(
                            R1, R2, G1, G2, B1,
                            B2, histo
                           );
        }

        public int[] Avg(bool force) {
            if (avg == null || force) {
                var ntot = 0;

                var rsum = 0;
                var gsum = 0;
                var bsum = 0;

                int i;

                for (i = R1; i <= R2; i++) {
                    int j;

                    for (j = G1; j <= G2; j++) {
                        int k;

                        for (k = B1; k <= B2; k++) {
                            var histoindex = Mmcq.GetColorIndex(i, j, k);
                            var hval       = histo[histoindex];
                            ntot += hval;
                            rsum += Convert.ToInt32((hval * (i + 0.5) * Mmcq.Mult));
                            gsum += Convert.ToInt32((hval * (j + 0.5) * Mmcq.Mult));
                            bsum += Convert.ToInt32((hval * (k + 0.5) * Mmcq.Mult));
                        }
                    }
                }

                if (ntot > 0) {
                    avg = new[] {
                        Math.Abs(rsum / ntot), Math.Abs(gsum / ntot), Math.Abs(bsum / ntot)
                    };
                } else {
                    avg = new[] {
                        Math.Abs(Mmcq.Mult * (R1 + R2 + 1) / 2), Math.Abs(Mmcq.Mult * (G1 + G2 + 1) / 2), Math.Abs(Mmcq.Mult * (B1 + B2 + 1) / 2)
                    };
                }
            }

            return avg;
        }

    }

}