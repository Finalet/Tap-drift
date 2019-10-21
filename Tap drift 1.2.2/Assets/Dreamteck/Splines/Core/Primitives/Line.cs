using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
    public class Line : SplinePrimitive
    {
        public bool mirror = true;
        public float length = 1f;
        public int segments = 1;

        protected override void Generate()
        {
            base.Generate();
            type = Spline.Type.Linear;
            closed = false;
            CreatePoints(segments + 1, SplinePoint.Type.SmoothMirrored);
            Quaternion quaternion = Quaternion.Euler(rotation);
            Vector3 direction = quaternion * Vector3.forward;
            Vector3 origin = Vector3.zero;
            if (mirror) origin = -direction * length * 0.5f;
            for (int i = 0; i < points.Length; i++)
            {
                points[i].position = origin + direction * length * ((float)i / (points.Length - 1));
            }
        }
    }
}