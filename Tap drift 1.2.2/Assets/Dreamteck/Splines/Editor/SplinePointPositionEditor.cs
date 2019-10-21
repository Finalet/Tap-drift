#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public class SplinePointPositionEditor : SplinePointEditor
    {
        public bool snap = false;
        public float snapGridSize = 1f;

        public void Reset(Quaternion o)
        {
            orientation = o;
        }

        public override void LoadState()
        {
            base.LoadState();
            snap = LoadBool("snap");
            snapGridSize = LoadFloat("snapGridSize", 0.5f);
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveBool("snap", snap);
            SaveFloat("snapGridSize", snapGridSize);
        }

        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            Vector3 avg = Vector3.zero;
            for (int i = 0; i < selected.Count; i++) avg += points[selected[i]].position;
            avg /= selected.Count;
            Vector3 lastPos = avg;
            avg = Handles.PositionHandle(avg, orientation);

            for (int i = 0; i < selected.Count; i++)
            {
                if (computer.isClosed && selected[i] == computer.pointCount - 1) continue;
                if (!change)
                {
                    if (lastPos != avg)
                    {
                        change = true;
                        for (int n = 0; n < selected.Count; n++)
                        {
                            points[selected[n]].SetPosition(points[selected[n]].position + (avg - lastPos));
                            if(snap) points[selected[n]].SetPosition(SnapPoint(points[selected[n]].position));
                        }
                    }
                }

            }

            if (computer.type == Spline.Type.Bezier && selected.Count == 1)
            {
                int index = selected[0];
                Vector3 lastTangent = points[index].tangent;
                points[index].SetTangentPosition(Handles.PositionHandle(points[index].tangent, orientation));
                if (lastTangent != points[index].tangent) change = true;
                if (snap) points[index].SetTangentPosition(SnapPoint(points[index].tangent));
                if (!snap && lastTangent != points[index].tangent) change = true;
                lastTangent = points[index].tangent2;
                points[index].SetTangent2Position(Handles.PositionHandle(points[index].tangent2, orientation));
                if (lastTangent != points[index].tangent2) change = true;
                if (snap) points[index].SetTangent2Position(SnapPoint(points[index].tangent2));
            }
            return change;
        }

        public Vector3 SnapPoint(Vector3 point)
        {
            point.x = Mathf.RoundToInt(point.x / snapGridSize) * snapGridSize;
            point.y = Mathf.RoundToInt(point.y / snapGridSize) * snapGridSize;
            point.z = Mathf.RoundToInt(point.z / snapGridSize) * snapGridSize;
            return point;
        }
    }
}
#endif
