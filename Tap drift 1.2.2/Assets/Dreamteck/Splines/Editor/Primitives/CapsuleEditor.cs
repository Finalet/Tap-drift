using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Dreamteck.Splines.Primitives
{
    public class CapsuleEditor : PrimitiveEditor
    {
        Capsule capsule = new Capsule();

        public override string GetName()
        {
            return "Capsule";
        }

        protected override void OnGUI()
        {
            base.OnGUI();
            AxisGUI(capsule);
            OffsetGUI(capsule);
            RotationGUI(capsule);
            capsule.radius = EditorGUILayout.FloatField("Radius", capsule.radius);
            capsule.height = EditorGUILayout.FloatField("Height", capsule.height);
        }

        protected override void Update()
        {
            capsule.UpdateSplineComputer(computer);
            base.Update();
        }
    }
}
