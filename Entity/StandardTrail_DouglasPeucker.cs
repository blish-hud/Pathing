using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        /// <summary>
        /// Downsamples a list of consecutive points.
        /// </summary>
        /// <param name="points">The list of points</param>
        /// <param name="error">The tolerance before detecting a turn</param>
        /// <returns>The downsampled list of points</returns>
        internal IEnumerable<Vector3> PostProcessing_DouglasPeucker(IEnumerable<Vector3> points, float error = 0.2f) {
            Vector3[] vectors = points.ToArray();

            if (vectors.Length < 3) {
                return vectors;
            }

            // indices to points to keep
            var keep = new ConcurrentBag<int> {
                0,
                vectors.Length - 1
            };

            void Recursive(int first, int last) {
                if (last - first + 1 < 3) {
                    return;
                }

                var vFirst = vectors[first];
                var vLast  = vectors[last];

                var   lastToFirst = vLast - vFirst;
                float length      = lastToFirst.Length();
                float maxDist     = error;
                int   split       = 0;

                for (int i = first + 1; i < last; i++) {
                    var v = vectors[i];

                    // distance to line vFirst -> vLast
                    float dist = Vector3.Cross(vFirst - v, lastToFirst).Length() / length;

                    if (dist < maxDist) {
                        continue;
                    }

                    maxDist = dist;
                    split   = i;
                }

                if (split == 0) {
                    return;
                }

                keep.Add(split);
                var tasks = new Task[2];
                tasks[0] = Task.Run(() => Recursive(first, split));
                tasks[1] = Task.Run(() => Recursive(split, last));

                foreach (var task in tasks) {
                    task.Wait();
                }
            }

            Recursive(0, vectors.Length - 1);
            List<int> keepList = keep.ToList();
            keepList.Sort();
            return keepList.Select(i => vectors[i]).ToList();
        }

    }
}
