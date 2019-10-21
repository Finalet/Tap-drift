using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class LineEditor : PrimitiveEditor
    {
        Line line = new Line();

        public override string GetName()
        {
            return "Line";
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            AxisGUI(line);
            OffsetGUI(line);
            RotationGUI(line);
            line.length = EditorGUILayout.FloatField("Length", line.length);
            line.mirror = EditorGUILayout.Toggle("Mirror", line.mirror);
            line.rotation = EditorGUILayout.Vector3Field("Rotation", line.rotation);
            line.segments = EditorGUILayout.IntField("Segments", line.segments);
        }

        protected override void Update()
        {
            line.UpdateSplineComputer(computer);
            base.Update();
        }
    }
}
