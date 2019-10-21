#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(SplineRenderer), true)]
    public class SplineRendererEditor : MeshGenEditor
    {
        protected override void BodyGUI()
        {
            showDoubleSided = false;
            showFlipFaces = false;
            showRotation = false;
            showNormalMethod = false;
            base.BodyGUI();
            EditorGUI.BeginChangeCheck();
            SplineRenderer user = (SplineRenderer)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Geometry", EditorStyles.boldLabel);
            user.slices = EditorGUILayout.IntField("Slices", user.slices);
            if (user.slices < 1) user.slices = 1;
            UVControls(user);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Render", EditorStyles.boldLabel);
            user.autoOrient = EditorGUILayout.Toggle("Auto orient", user.autoOrient);
            if (user.autoOrient)
            {
                user.updateFrameInterval = EditorGUILayout.IntField("Update frame interval", user.updateFrameInterval);
                if (user.updateFrameInterval < 0) user.updateFrameInterval = 0; 
            }

            if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(user);
        }

    }
}
#endif
