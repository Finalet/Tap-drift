#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SurfaceGenerator))]
    public class SurfaceGeneratorEditor : MeshGenEditor
    {
        protected override void OnSceneGUI()
        {
            SurfaceGenerator user = (SurfaceGenerator)target;
            if(user.extrudeComputer != null)
            SplineDrawer.DrawSplineComputer(user.extrudeComputer, 0.0, 1.0, 0.5f);
        }
        
        protected override void BodyGUI()
        {
            showSize = false;
            showRotation = false;
            base.BodyGUI();
            SurfaceGenerator user = (SurfaceGenerator)target;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            user.expand = EditorGUILayout.FloatField("Expand", user.expand);
            if (user.extrudeComputer == null)
            {
                user.extrude = EditorGUILayout.FloatField("Extrude", user.extrude);
            }
            user.extrudeComputer = (SplineComputer)EditorGUILayout.ObjectField("Extrude Path", user.extrudeComputer, typeof(SplineComputer), true);
            if (user.extrudeComputer != null)
            {
                float clipFrom = (float)user.extrudeClipFrom;
                float clipTo = (float)user.extrudeClipTo;
                EditorGUILayout.MinMaxSlider(new GUIContent("Extrude Clip Range:"), ref clipFrom, ref clipTo, 0f, 1f);
                user.extrudeClipFrom = clipFrom;
                user.extrudeClipTo = clipTo;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Uv Coordinates", EditorStyles.boldLabel);
            user.uvOffset = EditorGUILayout.Vector2Field("UV Offset", user.uvOffset);
            user.uvScale = EditorGUILayout.Vector2Field("UV Scale", user.uvScale);

            if (user.extrude != 0f || user.extrudeComputer != null)
            {
                user.sideUvOffset = EditorGUILayout.Vector2Field("Side UV Offset", user.sideUvOffset);
                user.sideUvScale = EditorGUILayout.Vector2Field("Side UV Scale", user.sideUvScale);
                user.uniformUvs = EditorGUILayout.Toggle("Unform UVs", user.uniformUvs);
            }
            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(user);
        }  
    }
}
#endif
