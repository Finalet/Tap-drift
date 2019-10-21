using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class EllipseEditor : PrimitiveEditor
    {
        Ellipse ellipse = new Ellipse();

        public override string GetName()
        {
            return "Ellipse";
        }

        public override void Init(SplineComputer comp)
        {
            base.Init(comp);
            ellipse.offset = origin;
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            ellipse.axis = (SplinePrimitive.Axis)EditorGUILayout.EnumPopup("Axis", ellipse.axis);
            ellipse.xRadius = EditorGUILayout.FloatField("X Radius", ellipse.xRadius);
            ellipse.yRadius = EditorGUILayout.FloatField("Y Radius", ellipse.yRadius);
        }

        protected override void Update()
        {
            ellipse.UpdateSplineComputer(computer);
            base.Update();
        }
    }
}
