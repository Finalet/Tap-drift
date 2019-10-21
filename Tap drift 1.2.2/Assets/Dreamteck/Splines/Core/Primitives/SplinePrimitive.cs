using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines.Primitives {
    public class SplinePrimitive
    {
        protected bool closed = false;
        protected SplinePoint[] points = new SplinePoint[0];
        protected Spline.Type type = Spline.Type.Linear;

        public enum Axis { X, Y, Z, nX, nY, nZ }
        public Axis axis = Axis.Y;
        public Vector3 offset = Vector3.zero;
        public Vector3 rotation = Vector3.zero;

        protected virtual void Generate()
        {
        
        }

        public Spline GetSpline()
        {
            Generate();
            ApplyOffset();
            Spline spline = new Spline(type);
            spline.points = points;
            if (closed) spline.Close();
            return spline;
        }

        public void UpdateSpline(Spline spline)
        {
            Generate();
            ApplyOffset();
            spline.type = type;
            spline.points = points;
            if (closed) spline.Close();
            else if (spline.isClosed) spline.Break();
        }

        public SplineComputer CreateSplineComputer(string name, Vector3 position, Quaternion rotation)
        {
            Generate();
            ApplyOffset();
            GameObject go = new GameObject(name);
            SplineComputer comp = go.AddComponent<SplineComputer>();
            comp.type = type;
            comp.SetPoints(points, SplineComputer.Space.Local);
            if (closed) comp.Close();
            comp.transform.position = position;
            comp.transform.rotation = rotation;
            return comp;
        }

        public void UpdateSplineComputer(SplineComputer comp)
        {
            Generate();
            ApplyOffset();
            comp.type = type;
            comp.SetPoints(points, SplineComputer.Space.Local);
            if (closed) comp.Close();
            else if (comp.isClosed) comp.Break();
        }

        void ApplyOffset()
        {
            Quaternion lookRot = Quaternion.LookRotation(GetNormal());
            Quaternion freeRot = Quaternion.Euler(rotation);
            for (int i = 0; i < points.Length; i++)
            {
                points[i].position = lookRot * freeRot * points[i].position;
                points[i].tangent = lookRot * freeRot *  points[i].tangent;
                points[i].tangent2 = lookRot * freeRot * points[i].tangent2;
                points[i].normal = lookRot * freeRot * Vector3.forward;
            }
            for (int i = 0; i < points.Length; i++) points[i].SetPosition(points[i].position + offset);
        }

        protected void CreatePoints(int count, SplinePoint.Type type)
        {
            if (points.Length != count) points = new SplinePoint[count];
            Vector3 look = GetNormal();
            for (int i = 0; i < points.Length; i++)
            {
                points[i].type = type;
                points[i].normal = look;
                points[i].color = Color.white;
                points[i].size = 1f;
            }
        }

        protected Vector3 GetNormal()
        {
            switch (axis)
            {
                case Axis.X: return Vector3.right;
                case Axis.Y: return Vector3.up;
                case Axis.Z: return Vector3.forward;
                case Axis.nX: return Vector3.left;
                case Axis.nY: return Vector3.down;
                case Axis.nZ: return Vector3.back;
                default: return Vector3.up;
            }
        }

    }
}
