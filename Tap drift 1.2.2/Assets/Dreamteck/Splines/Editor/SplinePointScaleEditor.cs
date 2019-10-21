#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public class SplinePointScaleEditor : SplinePointEditor
    {
        private PointTransformer transformer;

        public bool scaleSize = true;
        public bool scaleTangents = true;

        public override void LoadState()
        {
            base.LoadState();
            scaleSize = LoadBool("scaleSize");
            scaleTangents = LoadBool("scaleTangents");
        }

        public override void SaveState()
        {
            base.SaveState();
            SaveBool("scaleSize", scaleSize);
            SaveBool("scaleTangents", scaleTangents);
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
            Vector3 lastScale = transformer.scale;
            transformer.scale = Handles.ScaleHandle(transformer.scale, transformer.center, orientation, HandleUtility.GetHandleSize(transformer.center));
            if (lastScale != transformer.scale)
            {
                change = true;
                points = transformer.GetScaled(scaleSize, scaleTangents);
            }
            return change;
        }
    }
}
#endif
