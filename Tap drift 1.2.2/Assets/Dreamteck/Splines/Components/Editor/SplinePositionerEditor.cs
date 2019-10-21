#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplinePositioner), true)]
    public class SplinePositionerEditor : SplineTracerEditor
    {
        protected override void BodyGUI()
        {
            EditorGUILayout.Space();
            SplinePositioner positioner = (SplinePositioner)target;
            positioner.mode = (SplinePositioner.Mode)EditorGUILayout.EnumPopup("Mode", positioner.mode);
            if (positioner.mode == SplinePositioner.Mode.Distance) positioner.position = EditorGUILayout.FloatField("Distance", (float)positioner.position);
            else
            {
                double pos = positioner.ClipPercent(positioner.result.percent);
                EditorGUI.BeginChangeCheck();
                pos = EditorGUILayout.Slider("Percent", (float)pos, 0f, 1f);
                if (EditorGUI.EndChangeCheck()) positioner.position = pos;
            }
            positioner.targetObject = (GameObject)EditorGUILayout.ObjectField("Target object", positioner.targetObject, typeof(GameObject), true);
            base.BodyGUI();
        }
    }
}
#endif
