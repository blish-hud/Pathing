using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        /// <summary>
        /// TacO has a minimum of 30, so we replicate this.
        /// </summary>
        private const float DEFAULT_TRAILRESOLUTION = 30f;
        
        private IEnumerable<Vector3> PostProcessing_SetTrailResolution(IEnumerable<Vector3> points, float resolution = DEFAULT_TRAILRESOLUTION) {
            Vector3[] pointsArr = points as Vector3[] ?? points.ToArray();

            if (1 > pointsArr.Length) yield break;

            Vector3 prevPoint;

            yield return prevPoint = pointsArr[0];

            for (int i = 1; i < pointsArr.Length; i++) {
                var curPoint = pointsArr[i];

                float dist = Vector3.Distance(prevPoint, curPoint);
                float s    = dist / resolution;
                float inc = 1 / s;

                for (float v = inc; v < s - inc; v += inc) {
                    yield return Vector3.Lerp(prevPoint, curPoint, v / s);
                }

                prevPoint = curPoint;

                yield return curPoint;
            }
        }

    }
}
