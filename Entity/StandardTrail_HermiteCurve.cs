using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Entity {
    public partial class StandardTrail {

        /// <summary>
        /// Creates a list of points sampled semi-equidistant along the Cubic-Hermite interpolated curve from a list of points.
        /// </summary>
        /// <param name="points">The list of points the curve is generated from.</param>
        /// <param name="resolution">Distance between each sampled point</param>
        /// <param name="tension">Length of the tangents. 0 gives no overshoot, 1 gives a lot of overshoot.</param>
        /// <param name="smartSampling">Whether or not the curve should sample based on the curvature of the curve at each sampling point.</param>
        /// <param name="curvatureLowerBound">If <paramref name="smartSampling"/> is true, only sample points with a higher curvature than this parameter.</param>
        /// <param name="curvatureUpperBound">If <paramref name="smartSampling"/>  is true, sample <paramref name="upsampleCount"/> points between points with a curvature higher than this parameter.</param>
        /// <param name="upsampleCount">If <paramref name="smartSampling"/>  is true, the amount of points to upsample, between points with a curvature higher than <paramref name="curvatureUpperBound"/>.</param>
        private IEnumerable<Vector3> PostProcessing_HermiteCurve(IEnumerable<Vector3> points,
                                                                 float resolution = 0.15f,
                                                                 float tension = 0.5f,
                                                                 bool smartSampling = true,
                                                                 float curvatureLowerBound = 0.05f,
                                                                 float curvatureUpperBound = 2f,
                                                                 uint upsampleCount = 10) {
            tension = MathHelper.Clamp(tension, 0f, 1.0f);

            //Hermite basis functions
            float H00(float t) => (1 + 2 * t) * (float) Math.Pow(1 - t, 2.0f);
            float H10(float t) => t           * (float) Math.Pow(1 - t, 2.0f);
            float H01(float t) => (float) Math.Pow(t,                   2.0f) * (3 - 2 * t);
            float H11(float t) => (float) Math.Pow(t,                   2.0f) * (t - 1);

            Vector3 p0, p1, m0, m1;

            float SplineLength() {
                var c0 = m0;
                var c1 = 6f * (p1 - p0) - 4f * m0 - 2f * m1;
                var c2 = 6f * (p0 - p1) + 3f * (m1 + m0);

                Vector3 Derivative(float t) => c0 + t * (c1 + t * c2);

                var gaussLegendreCoefficients = new List<Vector2>() {
                    new (0.0f,         0.5688889f),
                    new (-0.5384693f,  0.47862867f),
                    new (0.5384693f,   0.47862867f),
                    new (-0.90617985f, 0.23692688f),
                    new (0.90617985f,  0.23692688f)
                };

                float length = 0.0f;

                foreach (var coeff in gaussLegendreCoefficients) {
                    float t = 0.5f * (1.0f + coeff.X);
                    length += Derivative(t).Length() * coeff.Y;
                }
                return 0.5f * length;

            }

            float GetCurvature(float t0) {
                //First derivative
                float H00dt(float t) => 6 * t * t         - 6 * t;
                float H10dt(float t) => 3 * t * t - 4 * t + 1;
                float H01dt(float t) => -6 * t * t        + 6 * t;
                float H11dt(float t) => 3  * t * t        - 2 * t;

                //Second derivative
                float H00dt2(float t) => 12  * t - 6;
                float H10dt2(float t) => 6   * t - 4;
                float H01dt2(float t) => -12 * t + 6;
                float H11dt2(float t) => 6   * t - 2;

                float curvature = (float) (Vector3.Cross(H00dt(t0)  * p0 + H10dt(t0)  * m0 + H01dt(t0)  * p1 + H11dt(t0)  * m1,
                                                         H00dt2(t0) * p0 + H10dt2(t0) * m0 + H01dt2(t0) * p1 + H11dt2(t0) * m1)
                                                  .Length()
                                         / Math.Pow((H00dt(t0) * p0 + H10dt(t0) * m0 + H01dt(t0) * p1 + H11dt(t0) * m1).Length(), 3));
                return curvature;
            }

            Vector3[] pointsArr = points.ToArray();

            Vector3 prevPoint;

            yield return prevPoint = pointsArr[0];

            for (int k = 0; k < pointsArr.Length - 1; k++) {

                p0 = pointsArr[k];
                p1 = pointsArr[k + 1];

                if (k > 0)
                    m0 = tension * (p1 - pointsArr[k - 1]);
                else
                    m0 = p1 - p0;

                if (k < pointsArr.Length - 2)
                    m1 = tension * (pointsArr[k + 2] - p0);
                else
                    m1 = p1 - p0;

                uint numPoints = (uint)(SplineLength() / resolution);
                float kappa = 0.0f;

                for (int i = 0; i < numPoints; i++) {
                    float t = i * (1.0f / numPoints);

                    if (smartSampling) {
                        kappa = GetCurvature(t);
                    }

                    var sampledPoint = H00(t) * p0 + H10(t) * m0 + H01(t) * p1 + H11(t) * m1;

                    if (smartSampling && kappa < curvatureLowerBound && (prevPoint - sampledPoint).Length() < 10) continue;

                    prevPoint = sampledPoint;

                    yield return sampledPoint;

                    if (smartSampling && kappa > curvatureUpperBound) {
                        float t1 = (i + 1) * (1.0f / numPoints);
                        float delta = 1.0f / upsampleCount;

                        for (float n = delta; n < 1; n += delta) {
                            float dt = (t1 - t) * n;
                            yield return H00(t + dt) * p0 + H10(t + dt) * m0 + H01(t + dt) * p1 + H11(t + dt) * m1;
                        }
                    }
                }
            }

            yield return pointsArr.Last();
        }

    }
}
