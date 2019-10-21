using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class StarEditor : PrimitiveEditor
    {
        Star star = new Star();

        public override string GetName()
        {
            return "Star";
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            AxisGUI(star);
            OffsetGUI(star);
            RotationGUI(star);
            star.radius = EditorGUILayout.FloatField("Radius", star.radius);
            star.depth = EditorGUILayout.FloatField("Depth", star.depth);
            star.sides = EditorGUILayout.IntField("Sides", star.sides);
        }

        protected override void Update()
        {
            star.UpdateSplineComputer(computer);
            base.Update();
        }
    }
}
