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