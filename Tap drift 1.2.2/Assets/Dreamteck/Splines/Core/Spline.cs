using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dreamteck;

namespace Dreamteck.Splines {
    //The Spline class defines a spline with world coordinates. It comes with various sampling methods
    [System.Serializable]
    public class Spline {
        public enum Direction { Forward = 1, Backward = -1 }
        public enum Type { Hermite, BSpline, Bezier, Linear};
        public SplinePoint[] points = new SplinePoint[0];
        [SerializeField]
        private bool closed = false;
        public Type type = Type.Bezier;
        public AnimationCurve customValueInterpolation = null;
        public AnimationCurve customNormalInterpolation = null;
        [Range(0f, 0.9999f)]
        public double precision = 0.9f;
        private Vector3[] hermitePoints = new Vector3[4];
        /// <summary>
        /// Returns true if the spline is closed
        /// </summary>
        public bool isClosed
        {
            get
            {
                return closed && points.Length >= 4;
            }
            set { }
        }
        /// <summary>
        /// The step size of the percent incrementation when evaluating a spline (based on percision)
        /// </summary>
        public double moveStep
        {
            get {
                if (type == Type.Linear) return 1f / (points.Length-1);
                return 1f / (iterations-1);
            }
            set { }
        }
       /// <summary>
        /// The total count of samples for the spline (based on the precision)
       /// </summary>
       public int iterations
        {
            get {
                if (type == Type.Linear) return points.Length;
                return DMath.CeilInt(1.0 / ((1.0 - precision) / (points.Length - 1)))+1;
            }
            set { }
        }


		public Spline(Type t){
			type = t;
			points = new SplinePoint[0];
		}

        public Spline(Type t, double p)
        {
            type = t;
            precision = p;
            points = new SplinePoint[0];
        }

        /// <summary>
        /// Calculate the length of the spline
        /// </summary>
        /// <param name="from">Calculate from [0-1] default: 0f</param>
        /// <param name="to">Calculate to [0-1] default: 1f</param>
        /// <param name="resolution">Resolution multiplier for precision [0-1] default: 1f</param>
        /// <returns></returns>
        public float CalculateLength(double from = 0.0, double to = 1.0, double resolution = 1.0)
        {
            if (points.Length == 0) return 0f;
            resolution = DMath.Clamp01(resolution);
            if (resolution == 0.0) return 0f;
            from = DMath.Clamp01(from);
            to = DMath.Clamp01(to);
            if (to < from) to = from;
            double percent = from;
            Vector3 lastPos = EvaluatePosition(percent);
            float sum = 0f;
            while (true)
            {
                percent = DMath.Move(percent, to, moveStep / resolution);
                Vector3 pos = EvaluatePosition(percent);
                sum += (pos - lastPos).magnitude;
                lastPos = pos;
                if (percent == to) break;
            }
            return sum;
        }

        /// <summary>
        /// Project point on the spline. Returns evaluation percent.
        /// </summary>
        /// <param name="point">3D Point</param>
        /// <param name="subdivide">Subdivisions default: 4</param>
        /// <param name="from">Sample from [0-1] default: 0f</param>
        /// <param name="to">Sample to [0-1] default: 1f</param>
        /// <returns></returns>
        public double Project(Vector3 point, int subdivide = 3, double from = 0.0, double to = 1.0)
        {
            if (points.Length == 0) return 0.0;
            if (closed && from == 0.0 && to == 1.0) //Handle looped splines
            {
                double closest = GetClosestPoint(subdivide, point, from, to, Mathf.RoundToInt(Mathf.Max(iterations / points.Length, 10)) * 5);
                if (closest < moveStep)
                {
                    double nextClosest = GetClosestPoint(subdivide, point, 0.5, to, Mathf.RoundToInt(Mathf.Max(iterations / points.Length, 10)) * 5);
                    if (Vector3.Distance(point, EvaluatePosition(nextClosest)) < Vector3.Distance(point, EvaluatePosition(closest))) return nextClosest;
                }
                return closest;
            }
            return GetClosestPoint(subdivide, point, from, to, Mathf.RoundToInt(Mathf.Max(iterations / points.Length, 10)) * 5);
        }

        /// <summary>
        /// Casts rays along the spline against all colliders in the scene
        /// </summary>
        /// <param name="hit">Hit information</param>
        /// <param name="hitPercent">The percent of evaluation where the hit occured</param>
        /// <param name="layerMask">Layer mask for the raycast</param>
        /// <param name="resolution">Resolution multiplier for precision [0-1] default: 1f</param>
        /// <param name="from">Raycast from [0-1] default: 0f</param>
        /// <param name="to">Raycast to [0-1] default: 1f</param>
        /// <param name="hitTriggers">Should hit triggers? (not supported in 5.1)</param>
        /// <returns></returns>
        public bool Raycast(out RaycastHit hit, out double hitPercent, LayerMask layerMask, double resolution = 1.0, double from = 0.0, double to = 1.0
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
        , QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal
#endif
        )
        {
            resolution = DMath.Clamp01(resolution);
            from = DMath.Clamp01(from);
            to = DMath.Clamp01(to);
            double percent = from;
            Vector3 fromPos = EvaluatePosition(percent);
            hitPercent = 0f;
            if (resolution == 0f)
            {
                hit = new RaycastHit();
                hitPercent = 0f;
                return false;
            }
            while (true)
            {
                double prevPercent = percent;
                percent = DMath.Move(percent, to, moveStep / resolution);
                Vector3 toPos = EvaluatePosition(percent);
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
                if (Physics.Linecast(fromPos, toPos, out hit, layerMask, hitTriggers))
#else 
                if (Physics.Linecast(fromPos, toPos, out hit, layerMask))
#endif
                {
                    double segmentPercent = (hit.point - fromPos).sqrMagnitude / (toPos - fromPos).sqrMagnitude;
                    hitPercent = DMath.Lerp(prevPercent, percent, segmentPercent);
                    return true;
                }
                fromPos = toPos;
                if (percent == to) break;
            }
            return false;
        }


        /// <summary>
        /// Casts rays along the spline against all colliders in the scene and returns all hits. Order is not guaranteed.
        /// </summary>
        /// <param name="hits">Hit information</param>
        /// <param name="hitPercents">The percents of evaluation where each hit occured</param>
        /// <param name="layerMask">Layer mask for the raycast</param>
        /// <param name="resolution">Resolution multiplier for precision [0-1] default: 1f</param>
        /// <param name="from">Raycast from [0-1] default: 0f</param>
        /// <param name="to">Raycast to [0-1] default: 1f</param>
        /// <param name="hitTriggers">Should hit triggers? (not supported in 5.1)</param>
        /// <returns></returns>
        public bool RaycastAll(out RaycastHit[] hits, out double[] hitPercents, LayerMask layerMask, double resolution = 1.0, double from = 0.0, double to = 1.0
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
            , QueryTriggerInteraction hitTriggers = QueryTriggerInteraction.UseGlobal
#endif
            )
        {
            resolution = DMath.Clamp01(resolution);
            from = DMath.Clamp01(from);
            to = DMath.Clamp01(to);
            double percent = from;
            Vector3 fromPos = EvaluatePosition(percent);
            List<RaycastHit> hitList = new List<RaycastHit>();
            List<double> percentList = new List<double>();
            if (resolution == 0f)
            {
                hits = new RaycastHit[0];
                hitPercents = new double[0];
                return false;
            }
            bool hasHit = false;
            while (true)
            {
                double prevPercent = percent;
                percent = DMath.Move(percent, to, moveStep / resolution);
                Vector3 toPos = EvaluatePosition(percent);
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
                RaycastHit[] h = Physics.RaycastAll(fromPos, toPos - fromPos, Vector3.Distance(fromPos, toPos), layerMask, hitTriggers);
#else 
                RaycastHit[] h = Physics.RaycastAll(fromPos, toPos - fromPos, Vector3.Distance(fromPos, toPos), layerMask);
#endif
                for (int i = 0; i < h.Length; i++)
                {
                    hasHit = true;
                    double segmentPercent = (h[i].point - fromPos).sqrMagnitude / (toPos - fromPos).sqrMagnitude;
                    percentList.Add(DMath.Lerp(prevPercent, percent, segmentPercent));
                    hitList.Add(h[i]);
                }
                fromPos = toPos;
                if (percent == to) break;
            }
            hits = hitList.ToArray();
            hitPercents = percentList.ToArray();
            return hasHit;
        } 

        /// <summary>
        /// Converts a point index to spline percent
        /// </summary>
        /// <param name="pointIndex">The point index</param>
        /// <returns></returns>
        public double GetPointPercent(int pointIndex)
        {
            return DMath.Clamp01((double)pointIndex / (points.Length - 1));
        }

        /// <summary>
        /// Evaluate the spline and return position. This is simpler and faster than Evaluate.
        /// </summary>
        /// <param name="percent">Percent of evaluation [0-1]</param>
        public Vector3 EvaluatePosition(double percent)
        {
            if (points.Length == 0) return Vector3.zero;
            Vector3 point = new Vector3();
            EvaluatePosition(ref point, percent);
            return point;
        }

        /// <summary>
        /// Evaluate the spline at the given time and return a SplineResult
        /// </summary>
        /// <param name="percent">Percent of evaluation [0-1]</param>
        public SplineResult Evaluate(double percent)
        {
            SplineResult result = new SplineResult();
            Evaluate(result, percent);
            return result;
		}

        /// <summary>
        /// Evaluate the spline at the position of a given point and return a SplineResult
        /// </summary>
        /// <param name="pointIndex">Point index</param>
        public SplineResult Evaluate(int pointIndex)
        {
            SplineResult result = new SplineResult();
            Evaluate(result, GetPointPercent(pointIndex));
            return result;
        }

        /// <summary>
        /// Evaluate the splien at the given point and write the result to the "result" object
        /// </summary>
        /// <param name="result">The result output</param>
        /// <param name="pointIndex">Point index</param>
        public void Evaluate(SplineResult result, int pointIndex)
        {
            Evaluate(result, GetPointPercent(pointIndex));
        }

        /// <summary>
        /// Evaluate the splien at the given time and write the result to the "result" object
        /// </summary>
        /// <param name="result">The result output</param>
        /// <param name="percent">Percent of evaluation [0-1]</param>
        public void Evaluate(SplineResult result, double percent)
        {
            if (points.Length == 0)
            {
                result = new SplineResult();
                return;
            }
            percent = DMath.Clamp01(percent);
            if (closed && points.Length <= 2) closed = false;
            if (points.Length == 1)
            {
                result.position = points[0].position;
                result.normal = points[0].normal;
                result.direction = Vector3.forward;
                result.size = points[0].size;
                result.color = points[0].color;
                result.percent = percent;
                return;
            }

            double doubleIndex = (points.Length - 1) * percent;
            int pointIndex = Mathf.Clamp(DMath.FloorInt(doubleIndex), 0, points.Length - 2);
            double getPercent = doubleIndex - pointIndex;
            Vector3 point = EvaluatePosition(percent);
            result.position = point;
            result.percent = percent;
            if (pointIndex <= points.Length - 2)
            {
                SplinePoint nextPoint = points[pointIndex + 1];
                if (pointIndex == points.Length - 2 && closed) nextPoint = points[0];
                float valueInterpolation = (float)getPercent;
                if (customValueInterpolation != null)
                {
                    if (customValueInterpolation.length > 0) valueInterpolation = customValueInterpolation.Evaluate(valueInterpolation);
                }
                float normalInterpolation = (float)getPercent;
                if (customNormalInterpolation != null)
                {
                    if (customNormalInterpolation.length > 0) normalInterpolation = customNormalInterpolation.Evaluate(normalInterpolation);
                }
                result.size = Mathf.Lerp(points[pointIndex].size, nextPoint.size, valueInterpolation);
                result.color = Color.Lerp(points[pointIndex].color, nextPoint.color, valueInterpolation);
                result.normal = Vector3.Slerp(points[pointIndex].normal, nextPoint.normal, normalInterpolation);
            }
            else
            {
                if (closed)
                {
                    result.size = points[0].size;
                    result.color = points[0].color;
                    result.normal = points[0].normal;
                }
                else
                {
                    result.size = points[pointIndex].size;
                    result.color = points[pointIndex].color;
                    result.normal = points[pointIndex].normal;
                }
            }
            if (type == Type.BSpline)
            {
                double step = (1.0 - precision) / points.Length;
                if (percent <= 1.0 - step && percent >= step) result.direction = EvaluatePosition(percent + step) - EvaluatePosition(percent - step);
                else
                {
                    Vector3 back = Vector3.zero, front = Vector3.zero;
                    if (closed)
                    {
                        if (percent < step) back = EvaluatePosition(1.0 - (step - percent));
                        else back = EvaluatePosition(percent - step);
                        if (percent > 1.0 - step) front = EvaluatePosition(step - (1.0 - percent));
                        else front = EvaluatePosition(percent + step);
                        result.direction = front - back;
                    }
                    else
                    {
                        back = result.position - EvaluatePosition(percent - step);
                        front = EvaluatePosition(percent + step) - result.position;
                        result.direction = Vector3.Slerp(front, back, back.magnitude / front.magnitude);
                    }
                }
            } else EvaluateTangent(ref result.direction, percent);
            result.direction.Normalize();
        }

        /// <summary>
        /// Evaluates the spline segment based on the spline's precision. 
        /// </summary>
        /// <param name="from">Start position [0-1]</param>
        /// <param name="to">Target position [from-1]</param>
        /// <returns></returns>
        public void Evaluate(ref SplineResult[] samples, double from = 0.0, double to = 1.0)
        {
            if (points.Length == 0) {
                samples = new SplineResult[0];
                return;
            }
            from = DMath.Clamp01(from);
            to = DMath.Clamp(to, from, 1.0);
            double fromValue = from * (iterations - 1);
            double toValue = to * (iterations - 1);
            int clippedIterations = DMath.CeilInt(toValue) - DMath.FloorInt(fromValue) + 1;
            if (samples == null) samples = new SplineResult[clippedIterations];
            if (samples.Length != clippedIterations) samples = new SplineResult[clippedIterations];
            double percent = from;
            double ms = moveStep;
            int index = 0;
            while (true)
            {
                samples[index] = Evaluate(percent);
                index++;
                if (index >= samples.Length) break;
                percent = DMath.Move(percent, to, ms);
            }
        }

        /// <summary>
        /// Evaluates the spline segment based on the spline's precision and returns only the position. 
        /// </summary>
        /// <param name="positions">The position buffer</param>
        /// <param name="from">Start position [0-1]</param>
        /// <param name="to">Target position [from-1]</param>
        /// <returns></returns>
        public void EvaluatePositions(ref Vector3[] positions, double from = 0.0, double to = 1.0)
        {
            if (points.Length == 0) {
                positions = new Vector3[0];
                return;
            }
            from = DMath.Clamp01(from);
            to = DMath.Clamp(to, from, 1.0);
            double fromValue = from * (iterations - 1);
            double toValue = to * (iterations - 1);
            int clippedIterations = DMath.CeilInt(toValue) - DMath.FloorInt(fromValue) + 1;
            if (positions.Length != clippedIterations) positions = new Vector3[clippedIterations];
            double percent = from;
            double ms = moveStep;
            int index = 0;
            while (true)
            {
                positions[index] = EvaluatePosition(percent);
                index++;
                if (index >= positions.Length) break;
                percent = DMath.Move(percent, to, ms);
            }
        }

        /// <summary>
        /// Returns the percent from the spline at a given distance from the start point
        /// </summary>
        /// <param name="start">The start point</param>
        /// /// <param name="distance">The distance to travel</param>
        /// <param name="direction">The direction towards which to move</param>
        /// <returns></returns>
        public double Travel(double start, float distance, Direction direction)
        {
            if (points.Length <= 1) return 0.0;
            if (direction == Spline.Direction.Forward && start >= 1.0) return 1.0;
            else if (direction == Spline.Direction.Backward && start <= 0.0) return 0.0; ;
            if (distance == 0f) return DMath.Clamp01(start);
            float moved = 0f;
            Vector3 pos = Vector3.zero;
            EvaluatePosition(ref pos, start);
            Vector3 lastPosition = pos;
            double lastPercent = start;
            int i = iterations - 1;
            int nextSampleIndex = direction == Spline.Direction.Forward ? DMath.CeilInt(start * i) : DMath.FloorInt(start * i);
            float lastDistance = 0f;
            double percent = start;
            while (true)
            {
                percent = (double)nextSampleIndex / i;
                pos = EvaluatePosition(percent);
                lastDistance = Vector3.Distance(pos, lastPosition);
                lastPosition = pos;
                moved += lastDistance;
                if (moved >= distance) break;
                lastPercent = percent;
                if (direction == Spline.Direction.Forward)
                {
                    if (nextSampleIndex == i) break;
                    nextSampleIndex++;
                }
                else
                {
                    if (nextSampleIndex == 0) break;
                    nextSampleIndex--;
                }
            }
            return DMath.Lerp(lastPercent, percent, 1f - (moved - distance) / lastDistance);
        }

        private void EvaluatePosition(ref Vector3 point, double percent)
        {
            percent = DMath.Clamp01(percent);
            double doubleIndex = (points.Length - 1) * percent;
            int pointIndex = Mathf.Clamp(DMath.FloorInt(doubleIndex), 0, Mathf.Max(points.Length - 2, 0));
            GetPoint(ref point, doubleIndex - pointIndex, pointIndex);
        }

        private void EvaluateTangent(ref Vector3 tangent, double percent)
        {
            percent = DMath.Clamp01(percent);
            double doubleIndex = (points.Length - 1) * percent;
            int pointIndex = Mathf.Clamp(DMath.FloorInt(doubleIndex), 0, Mathf.Max(points.Length - 2, 0));
            GetTangent(ref tangent, doubleIndex - pointIndex, pointIndex);
        }

        //Get closest point in spline segment. Used for projection
        private double GetClosestPoint(int iterations, Vector3 point, double start, double end, int slices)
        {
            if (iterations <= 0)
            {
                float startDist = (point - EvaluatePosition(start)).sqrMagnitude;
                float endDist = (point - EvaluatePosition(end)).sqrMagnitude;
                if (startDist < endDist) return start;
                else if (endDist < startDist) return end;
                else return (start + end) / 2;
            }
            double closestPercent = 0.0;
            float closestDistance = Mathf.Infinity;
            double tick = (end - start) / slices;
            double t = start;
            Vector3 pos = Vector3.zero;
            while (true)
            {
                EvaluatePosition(ref pos, t);
                float dist = (point - pos).sqrMagnitude;
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestPercent = t;
                }
                if (t == end) break;
                t = DMath.Move(t, end, tick);
            }
            double newStart = closestPercent - tick;
            if (newStart < start) newStart = start;
            double newEnd = closestPercent + tick;
            if (newEnd > end) newEnd = end;
            return GetClosestPoint(--iterations, point, newStart, newEnd, slices);
        }

        /// <summary>
        /// Break the closed spline
        /// </summary>
        public void Break()
        {
            Break(0);
        }

        /// <summary>
        /// Break the closed spline at given point
        /// </summary>
        /// <param name="at"></param>
        public void Break(int at)
        {
            if (!closed) return;
            if (at >= points.Length) return;
            SplinePoint[] prev = new SplinePoint[at];
            for (int i = 0; i < prev.Length; i++) prev[i] = points[i];
            for (int i = at; i < points.Length - 1; i++) points[i - at] = points[i];
            for (int i = 0; i < prev.Length; i++) points[points.Length - at + i - 1] = prev[i];
            points[points.Length - 1] = points[0];
            closed = false;
        }

        /// <summary>
        /// Close the spline. This will cause the first and last points of the spline to merge
        /// </summary>
        public void Close()
        {
            if (points.Length < 4)
            {
                Debug.LogError("Points need to be at least 4 to close the spline");
                return;
            }
            closed = true;
        }

        /// <summary>
        /// Convert the spline to a Bezier path
        /// </summary>
        public void ConvertToBezier()
        {
            switch (type)
            {
                case Type.Linear:
                    for (int i = 0; i < points.Length; i++)
                    {
                        points[i].type = SplinePoint.Type.Broken;
                        points[i].SetTangentPosition(points[i].position);
                        points[i].SetTangent2Position(points[i].position);
                    }
                    break;
                case Type.Hermite:
                    for (int i = 0; i < points.Length; i++)
                    {
                        GetHermitePoints(i);
                        points[i].type = SplinePoint.Type.SmoothMirrored;
                        if (i == 0)
                        {
                            Vector3 direction = hermitePoints[1] - hermitePoints[2];
                            if (closed)
                            {
                                direction = points[points.Length - 2].position - points[i + 1].position;
                                points[i].SetTangentPosition(points[i].position + direction / 6f);
                            } else points[i].SetTangentPosition(points[i].position + direction / 3f);
                        }
                        else if (i == points.Length - 1)
                        {
                            Vector3 direction = hermitePoints[2] - hermitePoints[3];
                            points[i].SetTangentPosition(points[i].position + direction / 3f);
                        }
                        else
                        {
                            Vector3 direction = hermitePoints[0] - hermitePoints[2];
                            points[i].SetTangentPosition(points[i].position + direction / 6f);
                        }
                    }
                    break;
                case Type.BSpline:
                    //No BSPline support yet
                    break;
            }
            type = Type.Bezier;
        }


        private void GetPoint(ref Vector3 point, double percent, int pointIndex)
        {
            //Handle closed paths
            if (closed && points.Length > 3)
            {
                if (pointIndex == points.Length - 2)
                {
                    points[0].SetTangentPosition(points[points.Length - 1].tangent);
                    points[points.Length - 1] = points[0];
                }
            } else closed = false;
            switch (type)
            {
                case Type.Hermite: HermiteGetPoint(ref point, percent, pointIndex); break;
                case Type.Bezier: BezierGetPoint(ref point, percent, pointIndex); break;
                case Type.BSpline: BSPGetPoint(ref point, percent, pointIndex); break;
                case Type.Linear: LinearGetPoint(ref point, percent, pointIndex); break;
            }
        }

        private void GetTangent(ref Vector3 tangent, double percent, int pointIndex)
        {
            switch (type)
            {
                case Type.Hermite: GetHermiteTangent(ref tangent, percent, pointIndex); break;
                case Type.Bezier: BezierGetTangent(ref tangent, percent, pointIndex); break;
                //case Type.BSpline: BSPGetTangent(ref tangent, percent, pointIndex);  break;
                case Type.Linear: LinearGetTangent(ref tangent, percent, pointIndex); break;
            }
        }

        private void LinearGetPoint(ref Vector3 point, double t, int i)
        {
            if (points.Length == 0)
            {
                point = Vector3.zero;
                return;
            }
            if (i < points.Length - 1)
            {
                t = DMath.Clamp01(t);
                i = Mathf.Clamp(i, 0, points.Length - 2);
                point = Vector3.Lerp(points[i].position, points[i + 1].position, (float)t);
            } else point = points[i].position;
        }

        private void LinearGetTangent(ref Vector3 tangent, double t, int i)
        {
            if (points.Length == 0)
            {
                tangent = Vector3.forward;
                return;
            }
            GetHermitePoints(i);
            tangent = hermitePoints[2] - hermitePoints[0];
        }

        private void BSPGetPoint(ref Vector3 point, double time, int i)
        {
            //Used for getting a point on a B-spline
            if (points.Length > 0) point = points[0].position;
            if (points.Length > 1)
            {
                float t1 = (float)DMath.Clamp01(time);
                GetHermitePoints(i);
                point = ((-3f * hermitePoints[0] + 3f * hermitePoints[2]) / 6f 
                + t1 * ((3f * hermitePoints[0] - 6f * hermitePoints[1] + 3 * hermitePoints[2]) / 6f 
                + t1 * (-hermitePoints[0] + 3f * hermitePoints[1] - 3f * hermitePoints[2] + hermitePoints[3]) / 6f)) * t1 
                + (hermitePoints[0] + 4f * hermitePoints[1] + hermitePoints[2]) / 6f;
            }
        }

        private void BezierGetPoint(ref Vector3 point, double t, int i)
        {
            //Used for getting a point on a Bezier spline
            if (points.Length > 0) point = points[0].position;
            else return;
            if (points.Length == 1) return;
            if (i < points.Length - 1)
            {
                t = DMath.Clamp01(t);
                i = Mathf.Clamp(i, 0, points.Length - 2);
                float ft = (float)t;
                float nt = 1f - ft;
                point = nt * nt * nt * points[i].position + 
                    3f * nt * nt * ft * points[i].tangent2 + 
                    3f * nt * ft * ft * points[i + 1].tangent + 
                    ft * ft * ft * points[i + 1].position;
            }
        }

        private void BezierGetTangent(ref Vector3 tangent, double t, int i)
        {
            if (points.Length > 0) tangent = points[0].tangent2;
            else return;
            if (points.Length == 1) return;
            if (i < points.Length - 1)
            {
                t = DMath.Clamp01(t);
                i = Mathf.Clamp(i, 0, points.Length - 2);
                float ft = (float)t;
                float nt = 1f - ft;
                tangent = -3 * nt * nt * points[i].position + 
                    3 * nt * nt * points[i].tangent2 - 
                    6 * ft * nt * points[i].tangent2 - 
                    3 * ft * ft * points[i + 1].tangent + 
                    6 * ft * nt * points[i + 1].tangent + 
                    3 * ft * ft * points[i + 1].position;
            }
        }

        private void HermiteGetPoint(ref Vector3 point, double t, int i)
        {
            float t1 = (float)t;
            float t2 = t1 * t1;
            float t3 = t2 * t1;
            if (points.Length > 0) point = points[0].position;
            if (i >= points.Length) return;
            if (points.Length > 1)
            {
                GetHermitePoints(i);
                point = 0.5f * ((2f * hermitePoints[1]) + (-hermitePoints[0] + hermitePoints[2]) * t1
                + (2f * hermitePoints[0] - 5f * hermitePoints[1] + 4f * hermitePoints[2] - hermitePoints[3]) * t2
                + (-hermitePoints[0] + 3f * hermitePoints[1] - 3f * hermitePoints[2] + hermitePoints[3]) * t3);
            }
		}

        private void GetHermiteTangent(ref Vector3 direction, double t, int i)
        {
            float t1 = (float)t;
            float t2 = t1 * t1;
            if (points.Length > 0) direction = Vector3.forward;
            if (i >= points.Length) return;
            if (points.Length > 1)
            {
                GetHermitePoints(i);
                direction = (6 * t2 - 6 * t1) * hermitePoints[1]
                + (3 * t2 - 4 * t1 + 1) * (hermitePoints[2] - hermitePoints[0]) * 0.5f
                + (-6 * t2 + 6 * t1) * hermitePoints[2]
                + (3 * t2 - 2 * t1) * (hermitePoints[3] - hermitePoints[1]) * 0.5f;
            }
        }

        private void GetHermitePoints(int i)
        {
            //Fills the array with the current point, the previous one, the next one and the one after that. Used for Hermite and Bspline
            if (i > 0) hermitePoints[0] = points[i - 1].position;
            else if (closed && points.Length - 2 > i) hermitePoints[0] = points[points.Length - 2].position;
            else if (i + 1 < points.Length) hermitePoints[0] = points[i].position + (points[i].position - points[i + 1].position); //Extrapolate
            else hermitePoints[0] = points[i].position;
            hermitePoints[1] = points[i].position;
            if (i + 1 < points.Length) hermitePoints[2] = points[i + 1].position;
            else if (closed && (i + 2) - points.Length != i) hermitePoints[2] = points[(i + 2) - points.Length].position;
            else hermitePoints[2] = hermitePoints[1] + (hermitePoints[1] - hermitePoints[0]); //Extrapolate
            if (i + 2 < points.Length) hermitePoints[3] = points[i + 2].position;
            else if (closed && (i + 3) - points.Length != i) hermitePoints[3] = points[(i + 3) - points.Length].position;
            else hermitePoints[3] = hermitePoints[2] + (hermitePoints[2] - hermitePoints[1]); //Extrapolate
        }
    }


}
