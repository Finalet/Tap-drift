#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(PathGenerator), true)]
    public class PathGeneratorEditor : MeshGenEditor
    {
        protected override void BodyGUI()
        {
            base.BodyGUI();
            PathGenerator pathGenerator = (PathGenerator)target;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
            pathGenerator.slices = EditorGUILayout.IntField("Slices", pathGenerator.slices);
            pathGenerator.useShapeCurve = EditorGUILayout.Toggle("Use Curve Shape", pathGenerator.useShapeCurve);
            if (pathGenerator.useShapeCurve)
            {
                if (pathGenerator.slices == 1) EditorGUILayout.HelpBox("Slices are set to 1. The curve shape may not be approximated correctly. You can increase the slices in order to fix that.", MessageType.Warning);
                pathGenerator.shape = EditorGUILayout.CurveField("Shape Curve", pathGenerator.shape);
                pathGenerator.shapeExposure = EditorGUILayout.FloatField("Shape Exposure", pathGenerator.shapeExposure);
            }
            if (pathGenerator.slices < 1) pathGenerator.slices = 1;
            UVControls(pathGenerator);
        }
    }
}
#endif
