using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines
{
    [System.Serializable]
    public class SplineAddress
    {
        public int depth
        {
            get
            {
                if (_elements == null) return 0;
                return _elements.Length;
            }
        }

        public SplineComputer root
        {
            get
            {
                if (_elements == null) return null;
                if (_elements.Length == 0) return null;
                if (_elements[0] == null) return null;
                return _elements[0].computer;
            }
            set
            {
                _elements = new Element[1];
                _elements[0] = new Element();
                _elements[0].computer = value;
                _elements[0].endPoint = -1;
            }
        }
        public Element[] elements
        {
            get
            {
                return _elements;
            }
        }
        public Element[] _elements = new Element[0];

        public double moveStep
        {
            get
            {
                if (root == null) return 1.0;
                if (root.pointCount < 2) return 1.0;
                double multiplier = (double)(root.pointCount-1) / (GetTotalPointCount()-1);
                return root.moveStep * multiplier;
            }
        }


        [System.Serializable]
        public class Element
        {
            public SplineComputer computer;
            public int startPoint
            {
                get
                {
                    return _startPoint;
                }
                set
                {
                    if (value < 0) value = 0;
                    if (value >= computer.pointCount)
                    {
                        Debug.LogError("Out of bounds point index setting. Tried setting index to " + value + " when computer only has " + computer.pointCount);
                        return;
                    }
                    _startPoint = value;
                }
            }
            public int endPoint
            {
                get
                {
                    if (_endPoint < 0) return computer.pointCount - 1;
                    return _endPoint;
                }
                set
                {
                    if (computer == null)
                    {
                        _endPoint = -1;
                        return;
                    }
                    if (value >= computer.pointCount)
                    {
                        Debug.LogError("Out of bounds point index setting. Tried setting index to " + value + " when computer only has " + computer.pointCount);
                        return;
                    }
                    _endPoint = value;
                }
            }

            [SerializeField]
            private int _startPoint = 0;
            [SerializeField]
            private int _endPoint = -1;

            public double startPercent
            {
                get
                {
                    return (double)startPoint / (computer.pointCount - 1);
                }
            }

            public double endPercent
            {
                get
                {
                    return (double)endPoint / (computer.pointCount - 1);
                }
            }

            public int span
            {
                get
                {
                    if (endPoint < 0) return (computer.pointCount - 1) - startPoint;
                    return Mathf.Abs(endPoint - startPoint);
                }
            }
        }

        public SplineAddress(SplineComputer rootComp)
        {
            _elements = new Element[1];
            _elements[0] = new Element();
            _elements[0].computer = root;
        }

        public SplineAddress(SplineAddress copy)
        {
            if (copy == null) return;
            if (copy.depth == 0) return;
            for (int i = 0; i < copy.elements.Length; i++)
            {
                AddElement(copy.elements[i]);
            }
        }

        public SplineResult Evaluate(double percent)
        {
            SplineResult result = new SplineResult();
            Evaluate(result, percent);
            return result;
        }

        public void Evaluate(SplineResult result, double percent)
        {
            if (root == null) return;
            SplineComputer comp;
            Spline.Direction dir;
            double eval = 0.0;
            GetEvaluationValues(percent, out comp, out eval, out dir);
            if (comp == null) return;
            comp.Evaluate(result, eval);
            result.percent = percent;
        }

        public Vector3 EvaluatePosition(double percent)
        {
            if (root == null) return Vector3.zero;
            if (_elements.Length == 1) return _elements[0].computer.EvaluatePosition(percent);
            double totalLength = 0.0;
            for (int i = 0; i < _elements.Length; i++) totalLength += _elements[i].span;
            double passed = 0.0;
            for (int i = 0; i < _elements.Length; i++)
            {
                double computerPercent = _elements[i].span / totalLength;
                passed += computerPercent;
                if (passed >= percent || Mathf.Approximately((float)passed, (float)percent))
                {
                    double localPercent = DMath.Lerp(_elements[i].startPercent, _elements[i].endPercent, DMath.InverseLerp(passed - computerPercent, passed, percent));
                    return _elements[i].computer.EvaluatePosition(localPercent);
                }
            }
            return Vector3.zero;
        }

        public double Project(Vector3 point, int subdivide = 4, double from = 0.0, double to = 1.0)
        {
            if (root == null) return 0.0;
            if (to > 1.0) to = 1.0;
            if (from > to) from = to;
            if (from < 0.0) from = 0.0;
            float closestDistance = Mathf.Infinity;
            int closestElement = 0;
            double closestPercent = 0.0;
            for (int i = 0; i < _elements.Length; i++)
            {
                double localFrom = PathToLocalPercent(from, i);
                double localTo = PathToLocalPercent(to, i);
                double projection = _elements[i].computer.Project(point, subdivide, localFrom, localTo);
                Vector3 projectedPoint = _elements[i].computer.EvaluatePosition(projection);
                float dist = (projectedPoint - point).sqrMagnitude;
                if (i == 0 || dist < closestDistance)
                {
                    closestDistance = dist;
                    closestElement = i;
                    closestPercent = projection;
                }
            }
            return LocalToPathPercent(closestPercent, closestElement);
        }

        public float CalculateLength(double from = 0.0, double to = 1.0)
        {
            if (root == null) return 0f;
            float length = 0f;
            for (int i = 0; i < _elements.Length; i++)
            {
                double localFrom = PathToLocalPercent(from, i);
                double localTo = PathToLocalPercent(to, i);
                length += _elements[i].computer.CalculateLength(localFrom, localTo);
            }
            return length;
        }

        /// <summary>
        /// Returns the percent from the spline at a given distance from the start point
        /// </summary>
        /// <param name="start">The start point</param>
        /// /// <param name="distance">The distance to travel</param>
        /// <param name="direction">The direction towards which to move</param>
        /// <returns></returns>
        public double Travel(double start, float distance, Spline.Direction direction, int iterations)
        {
            if (GetTotalPointCount() <= 1) return 0.0;
            float moved = 0f;
            Vector3 lastPosition = EvaluatePosition(start);
            double lastPercent = start;
            int i = iterations - 1;
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

        public int GetElementIndex(double percent)
        {
            if (root == null) return 0;
            int totalSpan = 0;
            foreach (Element element in _elements) totalSpan += element.span;
            double passed = 0.0;
            for (int i = 0; i < _elements.Length; i++)
            {
                double computerPercent = (double)_elements[i].span / totalSpan;
                passed += computerPercent;
                if (passed >= percent || Mathf.Approximately((float)passed, (float)percent))
                {
                    return i;
                }
            }
            return 0;
        }

        public double PathToLocalPercent(double pathPercent, int elementIndex)
        {
            if (root == null) return 0.0;
            int totalSpan = 0;
            foreach (Element element in _elements) totalSpan += element.span;
            double passed = 0.0;
            for (int i = 0; i < _elements.Length; i++)
            {
                double computerPercent = (double)_elements[i].span / totalSpan;
                passed += computerPercent;
                if (i == elementIndex)
                {
                    passed -= computerPercent;
                    return DMath.Lerp(_elements[i].startPercent, _elements[i].endPercent, DMath.InverseLerp(passed, passed + computerPercent, pathPercent));
                }
            }
            return 0.0;
        }

        public double LocalToPathPercent(double localPercent, int elementIndex)
        {
            if (root == null) return 0.0;
            int totalSpan = 0;
            foreach (Element element in _elements) totalSpan += element.span;
            double passed = 0.0;
            for (int i = 0; i < _elements.Length; i++)
            {
                double computerPercent = (double)_elements[i].span / totalSpan;
                passed += computerPercent;
                if (i == elementIndex)
                {
                    passed -= computerPercent;
                    double normalized = DMath.InverseLerp(_elements[i].startPercent, _elements[i].endPercent, localPercent);
                    return passed + computerPercent * normalized;
                }
            }
            return 0.0;
        }

        public int GetTotalPointCount()
        {
            if (root == null) return 0;
            if (_elements.Length == 1) return root.pointCount;
            int count = 0;
            for (int i = 0; i < _elements.Length; i++)
            {
                count += (_elements[i].span + 1);
                if (i > 0) count -= 1;
            }
            return count;
        }

        public void GetEvaluationValues(double inputPercent, out SplineComputer computer, out double percent, out Spline.Direction direction)
        {
            computer = null;
            percent = 0.0;
            direction = Spline.Direction.Forward;
            if (root == null) return;
            int totalSpan = 0;
            foreach (Element element in _elements) totalSpan += element.span;
            double passed = 0.0;
            for (int i = 0; i < _elements.Length; i++)
            {
                if (passed > inputPercent) break;
                double computerPercent = (double)_elements[i].span / totalSpan;
                percent = DMath.Lerp(_elements[i].startPercent, _elements[i].endPercent, DMath.InverseLerp(passed, passed + computerPercent, inputPercent));
                computer = _elements[i].computer;
                passed += computerPercent;
            }
        }

        private int LocalToPathPoint(int point, int elementIndex)
        {
            if (root == null) return 0;
            int pathIndex = 0;
            for (int i = 0; i < elementIndex; i++)
            {
                pathIndex += _elements[i].span;
            }
            pathIndex -= elementIndex;
            pathIndex += point;
            return pathIndex;
        }

        private void PathToLocalPoint(int point, out int computerIndex, out int localPoint)
        {
            int passed = 0;
            localPoint = 0;
            computerIndex = 0;
            if (root == null) return;
            for (int i = 0; i < _elements.Length; i++)
            {
                passed += _elements[i].span;
                if (passed == point)
                {
                    computerIndex = i;
                    localPoint = point - passed;
                }
            }
        }

        [System.Obsolete("Enter is obsolete, use AddSpline instead")]
        public void Enter(Node node, int connectionIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            AddSpline(node, connectionIndex, direction);
        }

        public void AddSpline(Node node, int connectionIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            if (root == null) return;
            Node.Connection[] connections = node.GetConnections();
            Element newElement = new Element();
            foreach (Node.Connection connection in connections)
            {
                if (connection.computer == _elements[_elements.Length - 1].computer)
                {
                    if ((connection.pointIndex >= _elements[_elements.Length - 1].startPoint && connection.pointIndex <= _elements[_elements.Length - 1].endPoint) || (connection.pointIndex >= _elements[_elements.Length - 1].endPoint && connection.pointIndex <= _elements[_elements.Length - 1].startPoint))
                    {
                        if (_elements[_elements.Length - 1].startPoint < 0) _elements[_elements.Length - 1].startPoint = 0;
                        _elements[_elements.Length - 1].endPoint = connection.pointIndex;
                        newElement.computer = connections[connectionIndex].computer;
                        newElement.startPoint = connections[connectionIndex].pointIndex;
                        if (direction == Spline.Direction.Backward) newElement.endPoint = 0;
                        AddElement(newElement);
                        return;
                    }
                }
            }
            Debug.LogError("Connection not valid. Node must have computer " + _elements[_elements.Length - 1].computer.name + " in order to connect");
        }

        public void AddSpline(SplineComputer computer, int connectionIndex, int connectedIndex, Spline.Direction direction = Spline.Direction.Forward)
        {
            if (root == null) return;
            if (connectedIndex < 0 || connectedIndex >= computer.pointCount) throw new System.Exception("Invalid spline point index " + connectedIndex + ". Index must be in the range [" + 0 + "-" + (computer.pointCount) + "]");
            if (_elements[_elements.Length - 1].startPoint < 0) _elements[_elements.Length - 1].startPoint = 0;
            _elements[_elements.Length - 1].endPoint = connectionIndex;
            Element newElement = new Element();
            newElement.computer = computer;
            newElement.startPoint = connectedIndex;
            if (direction == Spline.Direction.Backward) newElement.endPoint = 0;
            AddElement(newElement);
        }

        public void Exit(int exitDepth)
        {
            int newLength = _elements.Length - exitDepth;
            if (newLength < 1) newLength = 1;
            Element[] newElements = new Element[newLength];
            for (int i = 0; i < newElements.Length; i++) newElements[i] = _elements[i];
            _elements = newElements;
            if (_elements[_elements.Length - 1].endPoint >= _elements[_elements.Length - 1].startPoint) _elements[_elements.Length - 1].endPoint = -1;
            else _elements[_elements.Length - 1].endPoint = 0;
        }

        public void Collapse()
        {
            SplineComputer lastComputer = _elements[_elements.Length - 1].computer;
            int lastStart = _elements[_elements.Length - 1].startPoint;
            int lastEnd = _elements[_elements.Length - 1].endPoint;
            _elements = new Element[1];
            _elements[0] = new Element();
            _elements[0].computer = lastComputer;
            _elements[0].startPoint = lastStart;
            _elements[0].endPoint = lastEnd;
        }

        public void Clear()
        {
            Element tmp = _elements[0];
            _elements = new Element[1];
            _elements[0] = tmp;
            _elements[0].endPoint = -1;
        }

        private void AddElement(Element element)
        {
            Element[] newElements = new Element[_elements.Length + 1];
            _elements.CopyTo(newElements, 0);
            newElements[newElements.Length - 1] = element;
            _elements = newElements;
        }
    }
}
