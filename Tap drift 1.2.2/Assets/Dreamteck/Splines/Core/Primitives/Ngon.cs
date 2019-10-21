using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
    public class Ngon : SplinePrimitive
    {
        public float radius = 1f;
        public int sides = 3;

        protected override void Generate()
        {
            base.Generate();
            type = Spline.Type.Linear;
            closed = true;
            CreatePoints(sides + 1, SplinePoint.Type.SmoothMirrored);
            for (int i = 0; i < sides; i++)
            {
                float percent = (float)i / sides;
                Vector3 pos = Quaternion.AngleAxis(360f * percent, Vector3.forward) * Vector3.right * radius;
                points[i].SetPosition(pos);
            }
            points[points.Length - 1] = points[0];
        }
    }
}