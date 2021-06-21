using System.Collections.Generic;
using System.Linq;
using TmfLib;
using TmfLib.Pathable;

namespace BhModule.Community.Pathing {
    public class SharedPackCollection : IPackCollection {

        public PathingCategory Categories { get; private set; }

        public IList<PointOfInterest> PointsOfInterest { get; private set; }

        internal SharedPackCollection(PathingCategory categories = null, IEnumerable<PointOfInterest> pointsOfInterest = null) {
            this.Categories       = categories                 ?? new PathingCategory(true);
            this.PointsOfInterest = pointsOfInterest?.ToList() ?? new List<PointOfInterest>();
        }

        public void Unload() {
            // TODO: Prevent us from unloading while loading.
            //this.Categories = null;
            //this.PointsOfInterest.Clear();
        }

    }
}
