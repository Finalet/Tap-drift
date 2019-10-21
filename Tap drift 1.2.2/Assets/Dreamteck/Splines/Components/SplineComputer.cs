using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public delegate void EmptySplineHandler();
    //MonoBehaviour wrapper for the spline class. It transforms the spline using the object's transform and provides thread-safe methods for sampling
    [AddComponentMenu("Dreamteck/Splines/Spline Computer")]
    public partial class SplineComputer : MonoBehaviour
    {
        [System.Serializable]
        public class NodeLink
        {
            public Node node = null;
            public int pointIndex = 0;
        }
#if UNITY_EDITOR
        [HideInInspector]
        public Color editorPathColor = Color.white;
        [HideInInspector]
        public bool alwaysDraw = false;
        [HideInInspector]
        public bool drawThinckness = false;
        [HideInInspector]
        public bool billboardThickness = true;
        [HideInInspector]
        public bool is2D = false;
#endif
        public enum Space { World, Local };
        public Space space
        {
            get { return _space; }
            set
            {
                if (value != _space) Rebuild();
                _space = value;
            }
        }
        public Spline.Type type
        {
            get
            {
                return spline.type;
            }

            set
            {
                if (value != spline.type)
                {
                    spline.type = value;
                    Rebuild();
                }
                else spline.type = value;
            }
        }

        public double precision
        {
            get { return spline.precision; }
            set
            {
                if (value != spline.precision)
                {
                    if (value >= 1f) value = 0.99999;
                    spline.precision = value;
                    Rebuild();
                }
                else
                {
                    spline.precision = value;
                }
            }
        }

        public AnimationCurve customValueInterpolation
        {
            get { return spline.customValueInterpolation; }
            set
            {
                spline.customValueInterpolation = value;
                Rebuild();
            }
        }

        public AnimationCurve customNormalInterpolation
        {
            get { return spline.customNormalInterpolation; }
            set
            {
                spline.customNormalInterpolation = value;
                Rebuild();
            }
        }

        public int iterations
        {
            get
            {
                return spline.iterations;
            }
        }

        public double moveStep
        {
            get
            {
                return spline.moveStep;
            }
        }

        public bool isClosed
        {
            get
            {
                return spline.isClosed;
            }
        }

        public int pointCount
        {
            get
            {
                return spline.points.Length;
            }
        }

        public Morph morph
        {
            get
            {
                if (_morph == null) _morph = new Morph(this);
                else if (!_morph.initialized) _morph = new Morph(this);
                return _morph;
            }
        }

        public NodeLink[] nodeLinks
        {
            get { return _nodeLinks; }
        }

        public bool hasMorph
        {
            get
            {
                if (_morph == null) return false;
                return _morph.GetChannelCount() > 0;
            }
        }

        /// <summary>
        /// Thread-safe transform's position
        /// </summary>
        public Vector3 position
        {
            get {
                if (tsTransform == null) return this.transform.position;
                return tsTransform.position; }
               }
        /// <summary>
        /// Thread-safe transform's rotation
        /// </summary>
        public Quaternion rotation
        {
            get {
                if (tsTransform == null) return this.transform.rotation;
                return tsTransform.rotation; 
            }
        }
        /// <summary>
        /// Thread-safe transform's scale
        /// </summary>
        public Vector3 scale
        {
            get {
                if (tsTransform == null) return this.transform.localScale;
                return tsTransform.scale; 
            }
        }

        /// <summary>
        /// returns the number of subscribers this computer has
        /// </summary>
        public int subscriberCount
        {
            get
            {
                return subscribers.Length;
            }
        }

        [HideInInspector]
        [SerializeField]
        private Spline spline = new Spline(Spline.Type.Hermite);
        [HideInInspector]
        [SerializeField]
        private Morph _morph = null;
        [HideInInspector]
        [SerializeField]
        private Space _space = Space.Local;
        [HideInInspector]
        [SerializeField]
        private SplineUser[] subscribers = new SplineUser[0];
        [HideInInspector]
        [SerializeField]
        private NodeLink[] _nodeLinks = new NodeLink[0];
        private bool rebuildPending = false;

        private TS_Transform tsTransform = null;

        private bool updateRebuild = false;
        private bool lateUpdateRebuild = false;
        private SplineUser.UpdateMethod method = SplineUser.UpdateMethod.Update;

        public event EmptySplineHandler onRebuild;

#if UNITY_EDITOR
        public void EditorAwake()
        {
            UpdateConnectedNodes();
            RebuildImmediate();
        }

        public void EditorUpdateConnectedNodes()
        {
            UpdateConnectedNodes();
        }
#endif

        void Awake()
        {
            tsTransform = new TS_Transform(this.transform);
        }

        void LateUpdate()
        {
            method = SplineUser.UpdateMethod.LateUpdate;
            Run();
            if (lateUpdateRebuild) RebuildOnUpdate();
            lateUpdateRebuild = false;
        }

        void Update()
        {
            method = SplineUser.UpdateMethod.Update;
            Run();
            if (updateRebuild) RebuildOnUpdate();
            updateRebuild = false;
        }

        private void Run()
        {
            if (tsTransform.HasChange())
            {
                updateRebuild = true;
                lateUpdateRebuild = true;
                if (_nodeLinks.Length > 0) UpdateConnectedNodes();
                ResampleTransform();
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            editorPathColor = SplinePrefs.defaultColor;
            drawThinckness = SplinePrefs.defaultShowThickness;
            is2D = SplinePrefs.default2D;
            alwaysDraw = SplinePrefs.defaultAlwaysDraw;
            space = SplinePrefs.defaultComputerSpace;
            type = SplinePrefs.defaultType;
        }
#endif

        void OnEnable()
        {
            if (rebuildPending)
            {
                rebuildPending = false;
                Rebuild();
            }
        }

        /// <summary>
        /// Immediately sample the computer's transform (thread-unsafe). Call this before SetPoint(s) if the transform has been modified in the same frame
        /// </summary>
        public void ResampleTransform()
        {
            tsTransform.Update();
        }

        /// <summary>
        /// Subscribe a SplineUser to this computer. This will rebuild the user automatically when there are changes.
        /// </summary>
        /// <param name="input">The SplineUser to subscribe</param>
        public void Subscribe(SplineUser input)
        {
            int emptySlot = -1;
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == input) return;
                else if (subscribers[i] == null && emptySlot < 0) emptySlot = i;
            }
            if (emptySlot >= 0) subscribers[emptySlot] = input;
            else
            {
                SplineUser[] newSubscribers = new SplineUser[subscribers.Length + 1];
                subscribers.CopyTo(newSubscribers, 0);
                newSubscribers[subscribers.Length] = input;
                subscribers = newSubscribers;
            }
        }

        /// <summary>
        /// Unsubscribe a SplineUser from this computer's updates
        /// </summary>
        /// <param name="input">The SplineUser to unsubscribe</param>
        public void Unsubscribe(SplineUser input)
        {
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == input)
                {
                    SplineUser[] newSubscribers = new SplineUser[subscribers.Length - 1];
                    for (int j = 0; j < subscribers.Length; j++)
                    {
                        if (j < i) newSubscribers[j] = subscribers[j];
                        else if (j > i) newSubscribers[j - 1] = subscribers[j];
                    }
                    subscribers = newSubscribers;
                    break;
                }
            }
        }

        /// <summary>
        /// Checks if a user is subscribed to that computer
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsSubscribed(SplineUser user)
        {
            for (int i = 0; i < subscribers.Length; i++)
            {
                if (subscribers[i] == user)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns an array of subscribed suers
        /// </summary>
        /// <returns></returns>
        public SplineUser[] GetSubscribers()
        {
            return subscribers;
        }

        /// <summary>
        /// Add a link to a Node for a point in this computer. This is called by the Node component.
        /// </summary>
        /// <param name="node">Node input</param>
        /// <param name="pointIndex">Point index to link</param>
        public void AddNodeLink(Node node, int pointIndex)
        {
            if (node == null) return;
            if (pointIndex < 0 || pointIndex >= spline.points.Length) return;
            for (int i = 0; i < nodeLinks.Length; i++)
            {
                if (nodeLinks[i].node == node && nodeLinks[i].pointIndex == pointIndex)
                {
                    Debug.LogError("Junction already exists, cannot add junction " + node.name + " at point " + pointIndex);
                    return;
                }
            }
            if (!node.HasConnection(this, pointIndex))
            {
                Debug.LogError("Junction " + node.name + " does not have a connection for point " + pointIndex + " Call AddConnection on the junction.");
                return;
            }
            NodeLink newLink = new NodeLink();
            newLink.node = node;
            newLink.pointIndex = pointIndex;
            NodeLink[] newLinks = new NodeLink[_nodeLinks.Length + 1];
            _nodeLinks.CopyTo(newLinks, 0);
            newLinks[_nodeLinks.Length] = newLink;
            _nodeLinks = newLinks;
        }

        /// <summary>
        /// Remove a link to a node from a point in this computer. This is called by the Node component.
        /// </summary>
        /// <param name="pointIndex">Point index to remove link from.</param>
        public void RemoveNodeLink(int pointIndex)
        {
            int index = -1;
            for (int i = 0; i < _nodeLinks.Length; i++)
            {
                if (_nodeLinks[i].pointIndex == pointIndex)
                {
                    if (_nodeLinks[i].node != null && _nodeLinks[i].node.HasConnection(this, pointIndex))
                    {
                        _nodeLinks[i].node.RemoveConnection(this, pointIndex);
                        return;
                    }
                    else
                    {
                        index = i;
                        break;
                    }
                }
            }
            if (index < 0) return;
            RemoveNodeLinkAt(index);
        }

        /// <summary>
        /// Get the points from this computer's spline. All points are transformed in world coordinates.
        /// </summary>
        /// <returns></returns>
        public SplinePoint[] GetPoints(Space getSpace = Space.World)
        {
            SplinePoint[] points = new SplinePoint[spline.points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = spline.points[i];
                if (_space == Space.Local && getSpace == Space.World)
                {
                    points[i].position =TransformPoint(points[i].position);
                    points[i].tangent = TransformPoint(points[i].tangent);
                    points[i].tangent2 = TransformPoint(points[i].tangent2);
                    points[i].normal = TransformDirection(points[i].normal);
                }
            }
            return points;
        }

        /// <summary>
        /// Get a point from this computer's spline. The point is transformed in world coordinates.
        /// </summary>
        /// <param name="index">Point index</param>
        /// <returns></returns>
        public SplinePoint GetPoint(int index, Space getSpace = Space.World)
        {
            if (index < 0 || index >= spline.points.Length) return new SplinePoint();
            if (_space == Space.Local && getSpace == Space.World)
            {
                SplinePoint point = spline.points[index];
                point.position = TransformPoint(point.position);
                point.tangent = TransformPoint(point.tangent);
                point.tangent2 = TransformPoint(point.tangent2);
                point.normal = TransformDirection(point.normal);
                return point;
            } else return spline.points[index];
        }

        public Vector3 GetPointPosition(int index, Space getSpace = Space.World)
        {
            if (_space == Space.Local && getSpace == Space.World) return TransformPoint(spline.points[index].position);
            else return spline.points[index].position;
        }

        public Vector3 GetPointNormal(int index, Space getSpace = Space.World)
        {
            if (_space == Space.Local && getSpace == Space.World) return TransformDirection(spline.points[index].normal).normalized;
            else return spline.points[index].normal;
        }

        public Vector3 GetPointTangent(int index, Space getSpace = Space.World)
        {
            if (_space == Space.Local && getSpace == Space.World) return TransformPoint(spline.points[index].tangent);
            else return spline.points[index].tangent;
        }

        public Vector3 GetPointTangent2(int index, Space getSpace = Space.World)
        {
            if (_space == Space.Local && getSpace == Space.World) return TransformPoint(spline.points[index].tangent2);
            else return spline.points[index].tangent2;
        }

        public float GetPointSize(int index, Space getSpace = Space.World)
        {
            return spline.points[index].size;
        }

        public Color GetPointColor (int index, Space getSpace = Space.World)
        {
            return spline.points[index].color;
        }

        /// <summary>
        /// Set the points of this computer's spline.
        /// </summary>
        /// <param name="points">The points array</param>
        /// <param name="setSpace">Use world or local space</param>
        public void SetPoints(SplinePoint[] points, Space setSpace = Space.World)
        {
            bool rebuild = false;
            if (points.Length != spline.points.Length)
            {
                rebuild = true;
                if (points.Length < 4) Break();
                spline.points = new SplinePoint[points.Length];
            }
            SplinePoint newPoint;
            for (int i = 0; i < points.Length; i++)
            {
                newPoint = points[i];
                if (_space == Space.Local && setSpace == Space.World)
                {
                    newPoint.position = InverseTransformPoint(points[i].position);
                    newPoint.tangent = InverseTransformPoint(points[i].tangent);
                    newPoint.tangent2 = InverseTransformPoint(points[i].tangent2);
                    newPoint.normal = InverseTransformDirection(points[i].normal);
                }
                if (!rebuild)
                {
                    if (newPoint.position != spline.points[i].position) rebuild = true;
                    else if (newPoint.tangent != spline.points[i].tangent) rebuild = true;
                    else if (newPoint.tangent2 != spline.points[i].tangent2) rebuild = true;
                    else if (newPoint.size != spline.points[i].size) rebuild = true;
                    else if (newPoint.type != spline.points[i].type) rebuild = true;
                    else if (newPoint.color != spline.points[i].color) rebuild = true;
                    else if (newPoint.normal != spline.points[i].normal) rebuild = true;
                }
                spline.points[i] = newPoint;
            }
            if (rebuild)
            {
                Rebuild();
                UpdateConnectedNodes(points);
            }
        }

        /// <summary>
        /// Set the position of a control point. This is faster than SetPoint
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pos"></param>
        /// <param name="setSpace"></param>
        public void SetPointPosition(int index, Vector3 pos, Space setSpace = Space.World)
        {
            if (index < 0) return;
            if (index >= spline.points.Length) AppendPoints((index + 1) - spline.points.Length);
            Vector3 newPos = pos;
            if (_space == Space.Local && setSpace == Space.World) newPos = InverseTransformPoint(pos);
            if(newPos != spline.points[index].position)
            {
                spline.points[index].position = newPos;
                Rebuild();
                SetNodeForPoint(index, GetPoint(index));
            }
        }

        /// <summary>
        /// Set the tangents of a control point. This is faster than SetPoint
        /// </summary>
        /// <param name="index"></param>
        /// <param name="tan1"></param>
        /// <param name="tan2"></param>
        /// <param name="setSpace"></param>
        public void SetPointTangents(int index, Vector3 tan1, Vector3 tan2, Space setSpace = Space.World)
        {
            if (index < 0) return;
            if (index >= spline.points.Length) AppendPoints((index + 1) - spline.points.Length);
            Vector3 newTan1 = tan1;
            Vector3 newTan2 = tan2;
            if (_space == Space.Local && setSpace == Space.World)
            {
                newTan1 = InverseTransformPoint(tan1);
                newTan2 = InverseTransformPoint(tan2);
            }
            bool rebuild = false;
            if(newTan2 != spline.points[index].tangent2)
            {
                rebuild = true;
                spline.points[index].SetTangent2Position(newTan2);
            }
            if (newTan1 != spline.points[index].tangent)
            {
                rebuild = true;
                spline.points[index].SetTangentPosition(newTan1);
            }
            if (rebuild)
            {
                Rebuild();
                SetNodeForPoint(index, GetPoint(index));
            }
        }

        /// <summary>
        /// Set the normal of a control point. This is faster than SetPoint
        /// </summary>
        /// <param name="index"></param>
        /// <param name="nrm"></param>
        /// <param name="setSpace"></param>
        public void SetPointNormal(int index, Vector3 nrm, Space setSpace = Space.World)
        {
            if (index < 0) return;
            if (index >= spline.points.Length) AppendPoints((index + 1) - spline.points.Length);
            Vector3 newNrm = nrm;
            if (_space == Space.Local && setSpace == Space.World) newNrm = InverseTransformDirection(nrm);
            if (newNrm != spline.points[index].normal)
            {
                spline.points[index].normal = newNrm;
                Rebuild();
                SetNodeForPoint(index, GetPoint(index));
            }
        }

        /// <summary>
        /// Set the size of a control point. This is faster than SetPoint
        /// </summary>
        /// <param name="index"></param>
        /// <param name="size"></param>
        public void SetPointSize(int index, float size)
        {
            if (index < 0) return;
            if (index >= spline.points.Length) AppendPoints((index + 1) - spline.points.Length);
            if (size != spline.points[index].size)
            {
                spline.points[index].size = size;
                Rebuild();
                SetNodeForPoint(index, GetPoint(index));
            }
        }

        /// <summary>
        /// Set the color of a control point. THis is faster than SetPoint
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        public void SetPointColor(int index, Color color)
        {
            if (index < 0) return;
            if (index >= spline.points.Length) AppendPoints((index + 1) - spline.points.Length);
            if (color != spline.points[index].color)
            {
                spline.points[index].color = color;
                Rebuild();
                SetNodeForPoint(index, GetPoint(index));
            }
        }

        /// <summary>
        /// Set a control point in world coordinates
        /// </summary>
        /// <param name="index"></param>
        /// <param name="point"></param>
        public void SetPoint(int index, SplinePoint point, Space setSpace = Space.World)
        {
            if (index < 0) return;
            if (index >= spline.points.Length) AppendPoints((index + 1) - spline.points.Length);
            bool rebuild = false;
            SplinePoint newPoint = point;
            if (_space == Space.Local && setSpace == Space.World)
            {
                newPoint.position = InverseTransformPoint(point.position);
                newPoint.tangent = InverseTransformPoint(point.tangent);
                newPoint.tangent2 = InverseTransformPoint(point.tangent2);
                newPoint.normal = InverseTransformDirection(point.normal);
            }
            if (newPoint.position != spline.points[index].position) rebuild = true;
            else if (newPoint.tangent != spline.points[index].tangent) rebuild = true;
            else if (newPoint.tangent2 != spline.points[index].tangent2) rebuild = true;
            else if (newPoint.size != spline.points[index].size) rebuild = true;
            else if (newPoint.type != spline.points[index].type) rebuild = true;
            else if (newPoint.color != spline.points[index].color) rebuild = true;
            else if (newPoint.normal != spline.points[index].normal) rebuild = true;
            spline.points[index] = newPoint;
            if (rebuild)
            {
                Rebuild();
                SetNodeForPoint(index, point);
            }
        }

        private void AppendPoints(int count)
        {
            SplinePoint[] newPoints = new SplinePoint[spline.points.Length + count];
            spline.points.CopyTo(newPoints, 0);
            spline.points = newPoints;
            Rebuild();
        }

        /// <summary>
        /// Converts a point index to spline percent
        /// </summary>
        /// <param name="pointIndex">The point index</param>
        /// <returns></returns>
        public double GetPointPercent(int pointIndex)
        {
            return DMath.Clamp01((double)pointIndex / (pointCount - 1));
        }

        /// <summary>
        /// Same as Spline.EvaluatePosition but the result is transformed by the computer's transform
        /// </summary>
        /// <param name="percent">Evaluation percent</param>
        /// <param name="address">Optional address if junctions should be used</param>
        /// <returns></returns>
        public Vector3 EvaluatePosition(double percent)
        {
            Vector3 result = spline.EvaluatePosition(percent);
            if (_space == Space.Local) result = TransformPoint(result);
            return result;
        }

        /// <summary>
        /// Same as Spline.Evaluate but the result is transformed by the computer's transform
        /// </summary>
        /// <param name="percent">Evaluation percent</param>
        /// <returns></returns>
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
        /// Evaluate the spline at the position of a given point and write in the SplineResult output
        /// </summary>
        /// <param name="pointIndex">Point index</param>
        public void Evaluate(SplineResult result, int pointIndex)
        {
            Evaluate(result, GetPointPercent(pointIndex));
        }

        /// <summary>
        /// Same as Spline.Evaluate but the result is transformed by the computer's transform
        /// </summary>
        /// <param name="result"></param>
        /// <param name="percent"></param>
        public void Evaluate(SplineResult result, double percent)
        {
            spline.Evaluate(result, percent);
            if (_space == Space.Local) TransformResult(result);
        }

        /// <summary>
        /// Same as Spline.Evaluate but the results are transformed by the computer's transform
        /// </summary>
        /// <param name="from">Start position [0-1]</param>
        /// <param name="to">Target position [from-1]</param>
        /// <returns></returns>
        public void Evaluate(ref SplineResult[] samples, double from = 0.0, double to = 1.0)
        {
            spline.Evaluate(ref samples, from, to);
            if (_space == Space.Local)
            {
                for (int i = 0; i < samples.Length; i++)
                {
                    TransformResult(samples[i]);
                }
            }
        }

        /// <summary>
        /// Same as Spline.EvaluatePositions but the results are transformed by the computer's transform
        /// </summary>
        /// <param name="from">Start position [0-1]</param>
        /// <param name="to">Target position [from-1]</param>
        /// <returns></returns>
        public void EvaluatePositions(ref Vector3[] positions, double from = 0.0, double to = 1.0)
        {
            spline.EvaluatePositions(ref positions, from, to);
            if (_space == Space.Local)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = TransformPoint(positions[i]);
                }
            }
        }

        /// <summary>
        /// Returns the percent from the spline at a given distance from the start point
        /// </summary>
        /// <param name="start">The start point</param>
        /// /// <param name="distance">The distance to travel</param>
        /// <param name="direction">The direction towards which to move</param>
        /// <returns></returns>
        public double Travel(double start, float distance, Spline.Direction direction)
        {
            if(pointCount <= 1) return 0.0;
            if (direction == Spline.Direction.Forward && start >= 1.0) return 1.0;
            else if (direction == Spline.Direction.Backward && start <= 0.0) return 0.0;
            if (distance == 0f) return DMath.Clamp01(start);
            float moved = 0f;
            Vector3 lastPosition = EvaluatePosition(start);
            double lastPercent = start;
            int i = iterations-1;
            int nextSampleIndex = direction == Spline.Direction.Forward ? DMath.CeilInt(start * i) : DMath.FloorInt(start * i);
            float lastDistance = 0f;
            Vector3 pos = Vector3.zero;
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

        private void TransformResult(SplineResult result)
        {
            result.position = TransformPoint(result.position);
            result.direction = TransformDirection(result.direction);
            result.normal = TransformDirection(result.normal);
        }

        public void Rebuild()
        {
#if UNITY_EDITOR
            //If it's the editor and it's not playing, then rebuild immediate
            if (Application.isPlaying)
            {
                updateRebuild = true;
                lateUpdateRebuild = true;
            } else RebuildImmediate();
#else
            updateRebuild = true;
            lateUpdateRebuild = true;
#endif
        }

        public void RebuildImmediate()
        {
            if(Application.isPlaying) ResampleTransform();
            for (int i = subscribers.Length-1; i >= 0; i--)
            {
                if (subscribers[i] != null)
                {
                    if (subscribers[i].computer != this) RemoveSubscriber(i);
                    else subscribers[i].RebuildImmediate(true);
                }
                else RemoveSubscriber(i);
            }
            if (onRebuild != null) onRebuild();
        }

        private void RemoveSubscriber(int index)
        {
            SplineUser[] newSubscribers = new SplineUser[subscribers.Length - 1];
            for(int i = 0; i < subscribers.Length; i++)
            {
                if (i == index) continue;
                else if (i < index) newSubscribers[i] = subscribers[i];
                else newSubscribers[i - 1] = subscribers[i];
            }
            subscribers = newSubscribers;
        }

        private void RebuildOnUpdate()
        {
            for (int i = subscribers.Length - 1; i >= 0; i--)
            {
                if (subscribers[i] != null)
                {
                    if (subscribers[i].computer != this) RemoveSubscriber(i);
                    else subscribers[i].Rebuild(true);
                } else RemoveSubscriber(i);
            }
            if (onRebuild != null) onRebuild();
        }


        /// <summary>
        /// Rebuilds the users of connected via nodes computers
        /// </summary>
        public void RebuildConnectedUsers()
        {

        }

        private void RebuildUser(int index)
        {
            if (index >= subscribers.Length) return;
            if (subscribers[index] == null)
            {
                RemoveSubscriber(index);
                return;
            }
            if (method == subscribers[index].updateMethod) subscribers[index].Rebuild(true);
            else if (method == SplineUser.UpdateMethod.Update && subscribers[index].updateMethod == SplineUser.UpdateMethod.FixedUpdate) subscribers[index].Rebuild(true);
        }

        /// <summary>
        /// Same as Spline.Project but the point is transformed by the computer's transform.
        /// </summary>
        /// <param name="point">Point in space</param>
        /// <param name="subdivide">Subdivisions default: 4</param>
        /// <param name="from">Sample from [0-1] default: 0f</param>
        /// <param name="to">Sample to [0-1] default: 1f</param>
        /// <returns></returns>
        public double Project(Vector3 point, int subdivide = 3, double from = 0.0, double to = 1.0)
        {
            if (_space == Space.Local) point = InverseTransformPoint(point);
            return spline.Project(point, subdivide, from, to);
        }

        /// <summary>
        /// Same as Spline.Break() but it will update all subscribed users
        /// </summary>
        public void Break()
        {
            Break(0);
        }

        /// <summary>
        /// Same as Spline.Break(at) but it will update all subscribed users
        /// </summary>
        /// <param name="at"></param>
        public void Break(int at)
        {
            if (spline.isClosed)
            {
                spline.Break(at);
                Rebuild();
            }
        }

        /// <summary>
        /// Same as Spline.Close() but it will update all subscribed users
        /// </summary>
        public void Close()
        {
            if (!spline.isClosed)
            {
                spline.Close();
                Rebuild();
            }
        }

        /// <summary>
        /// Same as Spline.ConvertToBezier() but it will update all subscribed users
        /// </summary>
        public void ConvertToBezier()
        {
            spline.ConvertToBezier();
        }


        /// <summary>
        /// Same as Spline.CalculateLength but this takes the computer's transform into account when calculating the length.
        /// </summary>
        /// <param name="from">Calculate from [0-1] default: 0f</param>
        /// <param name="to">Calculate to [0-1] default: 1f</param>
        /// <param name="resolution">Resolution [0-1] default: 1f</param>
        /// <param name="address">Node address of junctions</param>
        /// <returns></returns>
        public float CalculateLength(double from = 0.0, double to = 1.0, double resolution = 1.0)
        {
            if (pointCount <= 1) return 0f;
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
        /// Casts a ray along the transformed spline against all scene colliders.
        /// </summary>
        /// <param name="hit">Hit information</param>
        /// <param name="hitPercent">The percent of evaluation where the hit occured</param>
        /// <param name="layerMask">Layer mask for the raycast</param>
        /// <param name="resolution">Resolution multiplier for precision [0-1] default: 1f</param>
        /// <param name="from">Raycast from [0-1] default: 0f</param>
        /// <param name="to">Raycast to [0-1] default: 1f</param>
        /// <param name="hitTriggers">Should hit triggers? (not supported in 5.1)</param>
        /// <param name="address">Node address of junctions</param>
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
        /// Casts a ray along the transformed spline against all scene colliders and returns all hits. Order is not guaranteed.
        /// </summary>
        /// <param name="hit">Hit information</param>
        /// <param name="hitPercent">The percents of evaluation where each hit occured</param>
        /// <param name="layerMask">Layer mask for the raycast</param>
        /// <param name="resolution">Resolution multiplier for precision [0-1] default: 1f</param>
        /// <param name="from">Raycast from [0-1] default: 0f</param>
        /// <param name="to">Raycast to [0-1] default: 1f</param>
        /// <param name="hitTriggers">Should hit triggers? (not supported in 5.1)</param>
        /// <param name="address">Node address of junctions</param>
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
        /// Returns an array of junctions available from the current position towards the end of the spline.
        /// </summary>
        /// <param name="percent">Current position</param>
        /// <param name="direction">Direction (forward or backward)</param>
        /// <returns></returns>
        public int[] GetAvailableNodeLinksAtPosition(double percent, Spline.Direction direction)
        {
            List<int> available = new List<int>();
            double pointValue = (pointCount-1)*percent;
            for (int i = 0; i < _nodeLinks.Length; i++)
            {
                if (direction == Spline.Direction.Forward)
                {
                    if (_nodeLinks[i].pointIndex >= pointValue)
                    {
                        available.Add(i);
                    }
                }
                else
                {
                    if (_nodeLinks[i].pointIndex <= pointValue)
                    {
                        available.Add(i);
                    }
                }
            }
            return available.ToArray();
        }

        /// <summary>
        /// Gets all connected computers along with the connected indices and connection indices
        /// </summary>
        /// <param name="computers">A list of the connected computers</param>
        /// <param name="connectionIndices">The point indices of this computer where the other computers are connected</param>
        /// <param name="connectedIndices">The point indices of the other computers where they are connected</param>
        /// <param name="percent"></param>
        /// <param name="direction"></param>
        /// <param name="includeEqual">Should point indices that are placed exactly at the percent be included?</param>
        public void GetConnectedComputers(List<SplineComputer> computers, List<int> connectionIndices, List<int> connectedIndices, double percent, Spline.Direction direction, bool includeEqual)
        {
            if (computers == null) computers = new List<SplineComputer>();
            if (connectionIndices == null) connectionIndices = new List<int>();
            if (connectedIndices == null) connectionIndices = new List<int>();
            computers.Clear();
            connectionIndices.Clear();
            connectedIndices.Clear();
            int pointValue = Mathf.FloorToInt((pointCount - 1) * (float)percent);
            for (int i = 0; i < _nodeLinks.Length; i++)
            {
                bool condition = false;
                if (includeEqual)
                {
                    if (direction == Spline.Direction.Forward) condition = _nodeLinks[i].pointIndex >= pointValue;
                    else condition = _nodeLinks[i].pointIndex <= pointValue;
                } else
                {
                    if (direction == Spline.Direction.Forward) condition = _nodeLinks[i].pointIndex > pointValue;
                    else condition = _nodeLinks[i].pointIndex < pointValue;
                }
                if (condition)
                {
                    Node.Connection[] connections = _nodeLinks[i].node.GetConnections();
                    for (int j = 0; j < connections.Length; j++)
                    {
                        if (connections[j].computer != this) {
                            computers.Add(connections[j].computer);
                            connectionIndices.Add(_nodeLinks[i].pointIndex);
                            connectedIndices.Add(connections[j].pointIndex);
                        }
                    }
                } 
            }
        }

        /// <summary>
        /// Set the morph's weight to 1 and all others to 0
        /// </summary>
        /// <param name="index">The index of the morph which should be set to 1</param>
        public void SetMorphState(int index)
        {
            if (!hasMorph) return;
            for (int i = 0; i < _morph.GetChannelCount(); i++)
            {
                if (i != index) _morph.SetWeight(i, 0f);
                else _morph.SetWeight(i, 1f);
            }
        }

        /// <summary>
        /// Set the morph's weight to 1 and all others to 0
        /// </summary>
        /// <param name="morphName">The name of the morph which should be set to 1</param>
        public void SetMorphState(string morphName)
        {
            if (!hasMorph) return;
            string[] morphNames = _morph.GetChannelNames();
            for (int i = 0; i < morphNames.Length; i++)
            {
                if (morphNames[i] == morphName)
                {
                    SetMorphState(i);
                    return;
                }
            }
            Debug.LogError("Morph state " + morphName + " not found");
        }

        /// <summary>
        /// Set the morph's weight and reduce the weights of all other morphs automatically
        /// </summary>
        /// <param name="index">The index of the morph</param>
        /// <param name="percent">percent (0-1) to set the morph's weight to</param>
        public void SetMorphState(int index, float percent)
        {
            if (!hasMorph) return;
            percent = Mathf.Clamp01(percent);
            float inversePercent = 1f - percent;
            for (int i = 0; i < _morph.GetChannelCount(); i++)
            {
                if (i != index)
                {
                    float weight = _morph.GetWeight(i);
                    weight = Mathf.Clamp(weight, 0f, inversePercent);
                    _morph.SetWeight(i, weight);
                }
                else _morph.SetWeight(i, percent);
            }
        }

        /// <summary>
        /// Set the morph's weight and reduce the weights of all other morphs automatically
        /// </summary>
        /// <param name="morphName">The name of the morph which should be set to 1</param>
        /// <param name="percent">percent (0-1) to set the morph's weight to</param>
        public void SetMorphState(string morphName, float percent)
        {
            if (!hasMorph) return;
            string[] morphNames = _morph.GetChannelNames();
            for (int i = 0; i < morphNames.Length; i++)
            {
                if (morphNames[i] == morphName)
                {
                    SetMorphState(i, percent);
                    return;
                }
            }
            Debug.LogError("Morph state " + morphName + " not found");
        }

        /// <summary>
        /// Automatically blends through all morphs based on the given percent
        /// </summary>
        /// <param name="percent">Percent for blending</param>
        public void SetMorphState(float percent)
        {
            if (!hasMorph) return;
            int morphCount = _morph.GetChannelCount();
            float morphValue = percent * (morphCount - 1);
            for (int i = 0; i < morphCount; i++)
            {
                float delta = Mathf.Abs(i - morphValue);
                if (delta > 1f) _morph.SetWeight(i, 0f);
                else
                {
                    if (morphValue <= i) _morph.SetWeight(i, 1f - (i - morphValue));
                    else _morph.SetWeight(i, 1f - (morphValue - i));
                }
            }
        }

        /// <summary>
        /// Returns a list of all connected computers. This includes the base computer too.
        /// </summary>
        /// <returns></returns>
        public List<SplineComputer> GetConnectedComputers()
        {
            List<SplineComputer> computers = new List<SplineComputer>();
            computers.Add(this);
            if (nodeLinks.Length == 0) return computers;
            GetConnectedComputers(ref computers);
            return computers;
        }

        private void GetConnectedComputers(ref List<SplineComputer> computers)
        {
            SplineComputer comp = computers[computers.Count - 1];
            if (comp == null) return;
            for (int i = 0; i < comp._nodeLinks.Length; i++)
            {
                if (comp._nodeLinks[i].node == null) continue;
                Node.Connection[] connections = comp._nodeLinks[i].node.GetConnections();
                for (int n = 0; n < connections.Length; n++)
                {
                    bool found = false;
                    if (connections[n].computer == this) continue;
                    for (int x = 0; x < computers.Count; x++)
                    {
                        if (computers[x] == connections[n].computer)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        computers.Add(connections[n].computer);
                        GetConnectedComputers(ref computers);
                    }
                }
            }
        }

        private void RemoveNodeLinkAt(int index)
        {
            //First remove the address from the subscribers
            for (int i = 0; i < subscribers.Length; i++)
            {
                for (int n = 0; n < subscribers[i].address.depth; n++)
                {
                    if (subscribers[i].address.elements[n].computer == this)
                    {
                        if (subscribers[i].address.elements[n].startPoint == nodeLinks[index].pointIndex || subscribers[i].address.elements[n].endPoint == nodeLinks[index].pointIndex)
                        {
                            subscribers[i].ExitAddress(subscribers[i].address.depth - n);
                            break;
                        }
                    }
                }
            }
            //Then remove the node link
            NodeLink[] newLinks = new NodeLink[_nodeLinks.Length - 1];
            for (int i = 0; i < _nodeLinks.Length; i++)
            {
                if (i == index) continue;
                else if (i < index) newLinks[i] = _nodeLinks[i];
                else newLinks[i - 1] = _nodeLinks[i];
            }
            _nodeLinks = newLinks;
        }

        //This magically updates the Node's position and all other points, connected to it when a point, linked to a Node is edited.
        private void SetNodeForPoint(int index, SplinePoint worldPoint)
        {
            for (int i = 0; i < _nodeLinks.Length; i++)
            {
                if (_nodeLinks[i].pointIndex == index)
                {
                    _nodeLinks[i].node.UpdatePoint(this, _nodeLinks[i].pointIndex, worldPoint);
                    break;
                }
            }
        }

        private void UpdateConnectedNodes(SplinePoint[] worldPoints)
        {
            for (int i = 0; i < _nodeLinks.Length; i++)
            {
                if (_nodeLinks[i].node == null)
                {
                    RemoveNodeLinkAt(i);
                    i--;
                    Rebuild();
                    continue;
                }
                bool found = false;
                foreach(Node.Connection connection in _nodeLinks[i].node.GetConnections())
                {
                    if(connection.computer == this)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    RemoveNodeLinkAt(i);
                    i--;
                    Rebuild();
                    continue;
                }
                _nodeLinks[i].node.UpdatePoint(this, _nodeLinks[i].pointIndex, worldPoints[_nodeLinks[i].pointIndex]);
                _nodeLinks[i].node.UpdateConnectedComputers(this);
            }
        }


        private void UpdateConnectedNodes()
        {
            for (int i = 0; i < _nodeLinks.Length; i++)
            {
                if (_nodeLinks[i].node == null)
                {
                    RemoveNodeLinkAt(i);
                    Rebuild();
                    i--;
                    continue;
                }
                _nodeLinks[i].node.UpdatePoint(this, _nodeLinks[i].pointIndex, GetPoint(_nodeLinks[i].pointIndex));
                _nodeLinks[i].node.UpdateConnectedComputers(this);
            }
        }

        public Vector3 TransformPoint(Vector3 point)
        {
            if (tsTransform != null && tsTransform.transform != null) return tsTransform.TransformPoint(point);
            else return transform.TransformPoint(point);
        }

        public Vector3 InverseTransformPoint(Vector3 point)
        {
            if (tsTransform != null && tsTransform.transform != null) return tsTransform.InverseTransformPoint(point);
            else return transform.InverseTransformPoint(point);
        }

        public Vector3 TransformDirection(Vector3 direction)
        {
            if (tsTransform != null && tsTransform.transform != null) return tsTransform.TransformDirection(direction);
            else return transform.TransformDirection(direction);
        }

        public Vector3 InverseTransformDirection(Vector3 direction)
        {
            if (tsTransform != null && tsTransform.transform != null) return tsTransform.InverseTransformDirection(direction);
            else return transform.InverseTransformDirection(direction);
        }
    }
}
