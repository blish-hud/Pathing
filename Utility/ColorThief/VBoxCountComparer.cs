using System.Collections.Generic;

namespace BhModule.Community.Pathing.Utility.ColorThief {

    internal class VBoxCountComparer : IComparer<VBox> {

        public int Compare(VBox x, VBox y) {
            var a = x.Count(false);
            var b = y.Count(false);
            return a < b ? -1 : (a > b ? 1 : 0);
        }

    }

}