using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class SpiralEditor : PrimitiveEditor
    {
        Spiral spiral = new Spiral();

        public override string GetName()
        {
            return "Spiral";
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            AxisGUI(spiral);
            OffsetGUI(spiral);
            RotationGUI(spiral);
            spiral.curve = EditorGUILayout.CurveField("Radius Interpolation", spiral.curve);
            spiral.startRadius = EditorGUILayout.FloatField("Start Radius", spiral.startRadius);
            spiral.endRadius = EditorGUILayout.FloatField("End Radius", spiral.endRadius);
            spiral.stretch = EditorGUILayout.FloatField("Stretch", spiral.stretch);
            spiral.iterations = EditorGUILayout.IntField("Iterations", spiral.iterations);
        }

        protected override void Update()
        {
            spiral.UpdateSplineComputer(computer);
            base.Update();
        }
    }
}
