#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public class SplinePointRotationEditor : SplinePointEditor
    {
        private PointTransformer transformer;

        public bool rotateNormals = true;
        public bool rotateTangents = true;

        public override void LoadState()
        {
            base.LoadState();
            rotateNormals = LoadBool("rotateNormals");
            rotateTangents = LoadBool("rotateTangents");
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveBool("rotateNormals", rotateNormals);
            SaveBool("rotateTangents", rotateTangents);
        }

        public void Reset(ref SplinePoint[] points, ref List<int> selected, Quaternion o)
        {
            orientation = o;
            transformer = new PointTransformer(points, selected, orientation);
        }

        public override bool SceneEdit(ref SplinePoint[] points, ref List<int> selected)
        {
            bool change = false;
            if (transformer == null) Reset(ref points, ref selected, orientation);
            if (rotateNormals)
            {
                Handles.color = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.4f);
                for (int i = 0; i < selected.Count; i++)
                {
                    Vector3 normal = points[selected[i]].normal;
                    normal *= HandleUtility.GetHandleSize(points[selected[i]].position);
                    Handles.DrawLine(points[selected[i]].position, points[selected[i]].position + normal);
                    SplineEditorHandles.DrawArrowCap(points[selected[i]].position + normal, Quaternion.LookRotation(normal), HandleUtility.GetHandleSize(points[selected[i]].position));
                }
            }
            Handles.color = Color.white;
            Quaternion lastRotation = transformer.rotation;
            transformer.rotation = Handles.RotationHandle(lastRotation, transformer.center);
            if (lastRotation != transformer.rotation)
            {
                change = true;
                points = transformer.GetRotated(rotateNormals, rotateTangents);
            }
            return change;
        }
    }
}
#endif
