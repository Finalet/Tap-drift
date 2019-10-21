using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace Dreamteck.Splines
{
    public class SplinePointMirrorEditor : SplinePointEditor
    {
        public Vector3 center = Vector3.zero;
        public enum Axis { X, Y, Z }
        public Axis axis = Axis.X;
        public bool flip = false;
        public float weldDistance = 0f;

        private SplinePoint[] originalPoints = new SplinePoint[0];
        private SplinePoint[] mirrored = new SplinePoint[0];
        private Vector3 lastCenter = Vector3.zero;
        private Axis lastAxis = Axis.X;
        private bool lastFlip = false;

        public override void LoadState()
        {
            axis = (Axis)LoadInt("axis");
            flip = LoadBool("flip");
            weldDistance = LoadFloat("weldDistance");
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveInt("axis", (int)axis);
            SaveBool("flip", flip);
            SaveFloat("weldDistance", weldDistance);
        }

        public void Reset()
        {
            originalPoints = new SplinePoint[0];
            mirrored = new SplinePoint[0];
        }

        public void Revert(ref SplinePoint[] points)
        {
            points = originalPoints;
        }

        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            if (originalPoints.Length == 0)
            {
                originalPoints = points;
                center = computer.transform.position;
            }
            center = Handles.PositionHandle(center, computer.transform.rotation);
            DrawMirror();
            if (lastCenter != center || lastAxis != axis || lastFlip || mirrored.Length != originalPoints.Length * 2)
            {
                List<int> half = GetHalf(ref originalPoints);
                int welded = -1;
                if (half.Count > 0)
                {
                    if (flip)
                    {
                        if (IsWeldable(originalPoints[half[0]]))
                        {
                            welded = half[0];
                            half.RemoveAt(0);
                        }
                    }
                    else
                    {
                        if (IsWeldable(originalPoints[half[half.Count - 1]]))
                        {
                            welded = half[half.Count - 1];
                            half.RemoveAt(half.Count - 1);
                        }
                    }

                    int offset = welded >= 0 ? 1 : 0;
                    int additionalSlot = computer.isClosed && half.Count > 0 ? 1 : 0;
                    if (additionalSlot > 0)
                    {
                        if (flip)
                        {
                            if (IsWeldable(originalPoints[half[half.Count - 1]])) additionalSlot = 0;
                        }
                        else
                        {
                            if (IsWeldable(originalPoints[half[0]])) additionalSlot = 0;
                        }
                    }
                    mirrored = new SplinePoint[half.Count * 2 + offset + additionalSlot];
                    for (int i = 0; i < half.Count; i++)
                    {
                        if (flip)
                        {
                            mirrored[i] = new SplinePoint(originalPoints[half[(half.Count - 1) - i]]);
                            mirrored[i + half.Count + offset] = GetMirrored(originalPoints[half[i]]);
                            SwapTangents(ref mirrored[i]);
                            SwapTangents(ref mirrored[i + half.Count + offset]);
                        }
                        else
                        {
                            mirrored[i] = new SplinePoint(originalPoints[half[i]]);
                            mirrored[i + half.Count + offset] = GetMirrored(originalPoints[half[(half.Count - 1) - i]]);
                        }
                    }
                    if (welded >= 0)
                    {
                        mirrored[half.Count] = new SplinePoint(originalPoints[welded]);
                        if (flip) SwapTangents(ref mirrored[half.Count]);
                        MakeMiddlePoint(ref mirrored[half.Count]);
                    }

                    if (computer.isClosed && mirrored.Length > 0)
                    {
                        if (additionalSlot == 0) MakeMiddlePoint(ref mirrored[0]);
                        mirrored[mirrored.Length - 1] = new SplinePoint(mirrored[0]);
                    }
                } else mirrored = new SplinePoint[0];
                
                lastCenter = center;
                lastAxis = axis;
                change = true;
            }
            points = mirrored;
            selected.Clear();
            return change;
        }

        void SwapTangents(ref SplinePoint point)
        {
            Vector3 temp = point.tangent;
            point.tangent = point.tangent2;
            point.tangent2 = temp;
        }

        void MakeMiddlePoint(ref SplinePoint point)
        {
            point.type = SplinePoint.Type.Broken;
            point.position = computer.transform.InverseTransformPoint(point.position);
            point.tangent = computer.transform.InverseTransformPoint(point.tangent);
            point.tangent2 = computer.transform.InverseTransformPoint(point.tangent2);
            Vector3 newPos = point.position;
            Vector3 localCenter = computer.transform.InverseTransformPoint(center);
            switch (axis)
            {
                case Axis.X:
                   
                    newPos.x = localCenter.x;
                    point.SetPosition(newPos);
                    if ((point.tangent.x >= localCenter.x && flip) || (point.tangent.x <= localCenter.x && !flip))
                    {
                        point.tangent2 = point.tangent;
                        point.tangent2.x = point.position.x + (point.position.x - point.tangent.x);
                    }
                    else
                    {
                        point.tangent = point.tangent2;
                        point.tangent.x = point.position.x + (point.position.x - point.tangent2.x);
                    }
                    break;
                case Axis.Y:
                    newPos.y = localCenter.y;
                    point.SetPosition(newPos);
                    if ((point.tangent.y >= localCenter.y && flip) || (point.tangent.y <= localCenter.y && !flip))
                    {
                        point.tangent2 = point.tangent;
                        point.tangent2.y = point.position.y + (point.position.y - point.tangent.y);
                    }
                    else
                    {
                        point.tangent = point.tangent2;
                        point.tangent.y = point.position.y + (point.position.y - point.tangent2.y);
                    }
                    break;
                case Axis.Z:
                    newPos.z = localCenter.z;
                    point.SetPosition(newPos);
                    if ((point.tangent.z >= localCenter.z && flip) || (point.tangent.z <= localCenter.z && !flip))
                    {
                        point.tangent2 = point.tangent;
                        point.tangent2.z = point.position.z + (point.position.z - point.tangent.z);
                    }
                    else
                    {
                        point.tangent = point.tangent2;
                        point.tangent.z = point.position.z + (point.position.z - point.tangent2.z);
                    }
                    break;
            }
            point.position = computer.transform.TransformPoint(point.position);
            point.tangent = computer.transform.TransformPoint(point.tangent);
            point.tangent2 = computer.transform.TransformPoint(point.tangent2);
        }

        bool IsWeldable(SplinePoint point)
        {
            Vector3 localCenter = computer.transform.InverseTransformPoint(center);
            switch (axis)
            {
                case Axis.X:
                    if (Mathf.Abs(point.position.x - localCenter.x) <= weldDistance) return true;
                    break;
                case Axis.Y:
                    if (Mathf.Abs(point.position.y - localCenter.y) <= weldDistance) return true;
                    break;
                case Axis.Z:
                    if (Mathf.Abs(point.position.z - localCenter.z) <= weldDistance) return true;
                    break;
            }
            return false;
        }

        void DrawMirror()
        {
            Vector3[] points = new Vector3[4];
            Color color = Color.white;
            float size = HandleUtility.GetHandleSize(center);
            Vector3 forward = computer.transform.forward * size;
            Vector3 back = -forward;
            Vector3 right = computer.transform.right * size;
            Vector3 left = -right;
            Vector3 up = computer.transform.up * size;
            Vector3 down = -up;
            switch (axis)
            {
                case Axis.X:
                    points[0] = back + up;
                    points[1] = forward + up;
                    points[2] = forward + down;
                    points[3] = back + down;
                    color = Color.red;
                    break;
                case Axis.Y:
                    points[0] = back + left;
                    points[1] = forward + left;
                    points[2] = forward + right;
                    points[3] = back + right;
                    color = Color.green;
                    break;
                case Axis.Z:
                    points[0] = left + up;
                    points[1] = right + up;
                    points[2] = right + down;
                    points[3] = left + down;
                    color = Color.blue;
                    break;
            }
            Handles.color = color;
            Handles.DrawLine(center + points[0], center + points[1]);
            Handles.DrawLine(center + points[1], center + points[2]);
            Handles.DrawLine(center + points[2], center + points[3]);
            Handles.DrawLine(center + points[3], center + points[0]);
            Handles.color = Color.white;
        }

        SplinePoint GetMirrored(SplinePoint source)
        {
            SplinePoint newPoint = new SplinePoint(source);
            newPoint.position = computer.transform.InverseTransformPoint(newPoint.position);
            newPoint.tangent = computer.transform.InverseTransformPoint(newPoint.tangent);
            newPoint.tangent2 = computer.transform.InverseTransformPoint(newPoint.tangent2);
            Vector3 localCenter = computer.transform.InverseTransformPoint(center);
            switch (axis)
            {
                case Axis.X:
                    newPoint.position.x = localCenter.x - (newPoint.position.x - localCenter.x);
                    newPoint.normal.x *= -1f;
                    newPoint.tangent.x = localCenter.x - (newPoint.tangent.x - localCenter.x);
                    newPoint.tangent2.x = localCenter.x - (newPoint.tangent2.x - localCenter.x);
                    SwapTangents(ref newPoint);
                    break;
                case Axis.Y:
                    newPoint.position.y = localCenter.y - (newPoint.position.y - localCenter.y);
                    newPoint.normal.y *= -1f;
                    newPoint.tangent.y = localCenter.y - (newPoint.tangent.y - localCenter.y);
                    newPoint.tangent2.y = localCenter.y - (newPoint.tangent2.y - localCenter.y);
                    break;
                case Axis.Z:
                    newPoint.position.z = localCenter.z - (newPoint.position.z - localCenter.z);
                    newPoint.normal.z *= -1f;
                    newPoint.tangent.z = localCenter.z - (newPoint.tangent.z - localCenter.z);
                    newPoint.tangent2.z = localCenter.z - (newPoint.tangent2.z - localCenter.z);
                    break;
            }
            newPoint.position = computer.transform.TransformPoint(newPoint.position);
            newPoint.tangent = computer.transform.TransformPoint(newPoint.tangent);
            newPoint.tangent2 = computer.transform.TransformPoint(newPoint.tangent2);
            return newPoint;
        }



        List<int> GetHalf(ref SplinePoint[] points)
        {
            List<int> found = new List<int>();
            Vector3 localCenter = computer.transform.InverseTransformPoint(center);
            switch (axis)
            {
                case Axis.X:
                    for(int i = 0; i < points.Length; i++)
                    {
                        if (computer.isClosed && i == points.Length - 1) break;
                        if (flip)
                        {
                            if (computer.transform.InverseTransformPoint(points[i].position).x >= localCenter.x) found.Add(i);
                        } else
                        {
                            if (computer.transform.InverseTransformPoint(points[i].position).x <= localCenter.x) found.Add(i);
                        }
                    }
                    break;
                case Axis.Y:
                    for (int i = 0; i < points.Length; i++)
                    {
                        if (computer.isClosed && i == points.Length - 1) break;
                        if (flip)
                        {
                            if (computer.transform.InverseTransformPoint(points[i].position).y >= localCenter.y) found.Add(i);
                        }
                        else
                        {
                            if (computer.transform.InverseTransformPoint(points[i].position).y <= localCenter.y) found.Add(i);
                        }
                    }
                    break;
                case Axis.Z:
                    for (int i = 0; i < points.Length; i++)
                    {
                        if (computer.isClosed && i == points.Length - 1) break;
                        if (flip)
                        {
                            if (computer.transform.InverseTransformPoint(points[i].position).z >= localCenter.z) found.Add(i);
                        }
                        else
                        {
                            if (computer.transform.InverseTransformPoint(points[i].position).z <= localCenter.z) found.Add(i);
                        }
                    }
                    break;
            }
            return found;
        }



    }
}
