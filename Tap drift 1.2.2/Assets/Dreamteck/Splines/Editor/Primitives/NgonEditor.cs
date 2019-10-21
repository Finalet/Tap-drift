using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class NgonEditor : PrimitiveEditor
    {
        Ngon ngon = new Ngon();

        public override string GetName()
        {
            return "Ngon";
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            AxisGUI(ngon);
            OffsetGUI(ngon);
            RotationGUI(ngon);
            ngon.radius = EditorGUILayout.FloatField("Radius", ngon.radius);
            ngon.sides = EditorGUILayout.IntField("Sides", ngon.sides);
        }

        protected override void Update()
        {
            ngon.UpdateSplineComputer(computer);
            base.Update();
        }
    }
}
