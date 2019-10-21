using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class RoundedRectangleEditor : PrimitiveEditor
    {
        RoundedRectangle rect = new RoundedRectangle();

        public override string GetName()
        {
            return "Rounded Rect";
        }

        public override void Init(SplineComputer comp)
        {
            base.Init(comp);
            rect.offset = origin;
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            AxisGUI(rect);
            OffsetGUI(rect);
            RotationGUI(rect);
            rect.size = EditorGUILayout.Vector2Field("Size", rect.size);
            rect.xRadius = EditorGUILayout.FloatField("X Radius", rect.xRadius);
            rect.yRadius = EditorGUILayout.FloatField("Y Radius", rect.yRadius);
        }

        protected override void Update()
        {
            rect.UpdateSplineComputer(computer);
            base.Update();
        }
    }
}
