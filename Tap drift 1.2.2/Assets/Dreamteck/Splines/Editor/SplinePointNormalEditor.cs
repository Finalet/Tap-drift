#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public class SplinePointNormalEditor : SplinePointEditor
    {
        public int setNormalMode = 0;

        public override void LoadState()
        {
            base.LoadState();
            setNormalMode = LoadInt("setNormalMode");
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveInt("setNormalMode", setNormalMode);
        }

        public void SetNormals(ref SplinePoint[] points, ref List<int> selected)
        {
            Vector3 avg = Vector3.zero;
            for (int i = 0; i < selected.Count; i++) avg += points[selected[i]].position;
            if (selected.Count > 1) avg /= selected.Count;
            Camera editorCamera = SceneView.lastActiveSceneView.camera;

            for (int i = 0; i < selected.Count; i++)
            {
                switch (setNormalMode)
                {
                    case 0: points[selected[i]].normal = Vector3.Normalize(editorCamera.transform.position - points[selected[i]].position); break;
                    case 1: points[selected[i]].normal = editorCamera.transform.forward; break;
                    case 2: points[selected[i]].normal = CalculatePointNormal(ref points, selected[i]); break;
                    case 3: points[selected[i]].normal = Vector3.left; break;
                    case 4: points[selected[i]].normal = Vector3.right; break;
                    case 5: points[selected[i]].normal = Vector3.up; break;
                    case 6: points[selected[i]].normal = Vector3.down; break;
                    case 7: points[selected[i]].normal = Vector3.forward; break;
                    case 8: points[selected[i]].normal = Vector3.back; break;
                    case 9: points[selected[i]].normal *= -1; break;
                    case 10: points[selected[i]].normal = Vector3.Normalize(avg - points[selected[i]].position); break;
                    case 11:
                        double percent = (double)selected[i] / (points.Length - 1);
                        SplineResult result = computer.Evaluate(percent);
                        points[selected[i]].normal = Vector3.Cross(result.direction, result.right).normalized;
                        break;
                }
            }
        }

        Vector3 CalculatePointNormal(ref SplinePoint[] points, int index)
        {
            if (points.Length < 3)
            {
                Debug.Log("Spline needs to have at least 3 control points in order to calculate normals");
                return Vector3.zero;
            }
            Vector3 side1 = Vector3.zero;
            Vector3 side2 = Vector3.zero;
            if (index == 0)
            {
                if (computer.isClosed)
                {
                    side1 = points[index].position - points[index + 1].position;
                    side2 = points[index].position - points[points.Length - 2].position;
                }
                else
                {
                    side1 = points[0].position - points[1].position;
                    side2 = points[0].position - points[2].position;
                }
            }
            else if (index == points.Length - 1)
            {
                side1 = points[points.Length - 1].position - points[points.Length - 3].position;
                side2 = points[points.Length - 1].position - points[points.Length - 2].position;
            }
            else
            {
                side1 = points[index].position - points[index + 1].position;
                side2 = points[index].position - points[index - 1].position;
            }
            return Vector3.Cross(side1.normalized, side2.normalized).normalized;
        }

        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            for (int i = 0; i < selected.Count; i++)
            {
                if (computer.isClosed && selected[i] == computer.pointCount - 1) continue;
                Handles.color = SplinePrefs.highlightColor;
                Handles.DrawWireDisc(points[selected[i]].position, points[selected[i]].normal, HandleUtility.GetHandleSize(points[selected[i]].position) * 0.35f);
                Handles.DrawWireDisc(points[selected[i]].position, points[selected[i]].normal, HandleUtility.GetHandleSize(points[selected[i]].position) * 0.7f);
                Handles.color = Color.yellow;
                Handles.DrawLine(points[selected[i]].position, points[selected[i]].position + HandleUtility.GetHandleSize(points[selected[i]].position) * points[selected[i]].normal);
                Vector3 normalPos = points[selected[i]].position + points[selected[i]].normal * HandleUtility.GetHandleSize(points[selected[i]].position);
                Vector3 lastNormal = points[selected[i]].normal;
                normalPos = SplineEditorHandles.FreeMoveCircle(normalPos, HandleUtility.GetHandleSize(normalPos) * 0.15f);
                normalPos -= points[selected[i]].position;
                normalPos.Normalize();
                if (normalPos == Vector3.zero) normalPos = Vector3.up;
                points[selected[i]].normal = normalPos;
                if (!change)
                {
                    if (lastNormal != normalPos)
                    {
                        change = true;
                        Quaternion delta = Quaternion.FromToRotation(lastNormal, normalPos);
                        for(int n = 0; n < selected.Count; n++)
                        {
                            if (n == i) continue;
                            points[selected[n]].normal = delta * points[selected[n]].normal;
                        }
                        return true;
                    }
                }
            }
            return change;
        }
    }
}
#endif
