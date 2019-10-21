
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(WaveformGenerator), true)]
    public class WaveGeneratorEditor : MeshGenEditor
    {
        protected override void BodyGUI()
        {
            showSize = false;
            showRotation = false;
            base.BodyGUI();
            WaveformGenerator user = (WaveformGenerator)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Axis", EditorStyles.boldLabel);
            user.axis = (WaveformGenerator.Axis)EditorGUILayout.EnumPopup("Axis", user.axis);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            user.slices = EditorGUILayout.IntField("Slices", user.slices);
            if (user.slices < 1) user.slices = 1;
            user.symmetry = EditorGUILayout.Toggle("Use Symmetry", user.symmetry);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Uv Coordinates", EditorStyles.boldLabel);
            user.uvWrapMode = (WaveformGenerator.UVWrapMode)EditorGUILayout.EnumPopup("Wrap Mode", user.uvWrapMode);
            user.uvOffset = EditorGUILayout.Vector2Field("UV Offset", user.uvOffset);
            user.uvScale = EditorGUILayout.Vector2Field("UV Scale", user.uvScale);
        }
    }
}
#endif
