#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines
{
    [CustomEditor(typeof(PolygonColliderGenerator))]
    public class PolygonColliderGenEditor : SplineUserEditor
    {
        protected override void BodyGUI()
        {
            base.BodyGUI();
            PolygonColliderGenerator generator = (PolygonColliderGenerator)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Polygon", EditorStyles.boldLabel);

            generator.type = (PolygonColliderGenerator.Type)EditorGUILayout.EnumPopup("Type", generator.type);
            if (generator.type == PolygonColliderGenerator.Type.Path) generator.size = EditorGUILayout.FloatField("Size", generator.size);
            generator.offset = EditorGUILayout.FloatField("Offset", generator.offset);
            generator.updateRate = EditorGUILayout.FloatField("Update Iterval", generator.updateRate);
            if (generator.updateRate < 0f) generator.updateRate = 0f;
            serializedObject.Update();
        }
        
    }
}
#endif