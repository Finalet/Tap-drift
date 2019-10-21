using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class RectangleEditor : PrimitiveEditor
    {
        Rectangle rect = new Rectangle();

        public override string GetName()
        {
            return "Rectangle";
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
        }

        protected override void Update()
        {
            rect.UpdateSplineComputer(computer);
            base.Update();
        }
    }
}
