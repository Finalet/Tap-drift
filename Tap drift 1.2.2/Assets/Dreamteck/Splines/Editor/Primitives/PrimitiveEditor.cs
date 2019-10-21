using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Dreamteck.Splines.Primitives
{
    [System.Serializable]
    public class PrimitiveEditor
    { 
        [System.NonSerialized]
        protected SplineComputer computer;
        [System.NonSerialized]
        protected bool lastClosed = false;
        [System.NonSerialized]
        protected SplinePoint[] lastPoints = new SplinePoint[0];
        [System.NonSerialized]
        protected Spline.Type lastType = Spline.Type.Bezier;
        [System.NonSerialized]
        public Vector3 origin = Vector3.zero;

        public virtual string GetName()
        {
            return "Primitive";
        }

        public virtual void Init(SplineComputer comp)
        {
            computer = comp;
            lastClosed = comp.isClosed;
            lastType = comp.type;
            lastPoints = comp.GetPoints(SplineComputer.Space.Local);
        }

        public virtual void Open()
        {
            Update();
        }

        public virtual void Close()
        {
            if (lastClosed) computer.Close();
            else computer.Break();
            computer.SetPoints(lastPoints, SplineComputer.Space.Local);
            computer.type = lastType;
        }

        public void Draw()
        {
            EditorGUI.BeginChangeCheck();
            OnGUI();
            if (EditorGUI.EndChangeCheck()) Update();
        }

        protected virtual void OnGUI()
        {

        }

        protected virtual void Update()
        {
            if (computer == null) return;
            SplineUser[] users = computer.GetComponents<SplineUser>();
            foreach (SplineUser user in users) user.Rebuild(true);
            computer.Rebuild();
            SceneView.RepaintAll();
        }

        protected void AxisGUI(SplinePrimitive primitive)
        {
            primitive.axis = (SplinePrimitive.Axis)EditorGUILayout.EnumPopup("Axis", primitive.axis);
        }

        protected void OffsetGUI(SplinePrimitive primitive)
        {
            primitive.offset = EditorGUILayout.Vector3Field("Offset", primitive.offset);
        }

        protected void RotationGUI(SplinePrimitive primitive)
        {
            primitive.rotation = EditorGUILayout.Vector3Field("Rotation", primitive.rotation);
        }
    }
}
