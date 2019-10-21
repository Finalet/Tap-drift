using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
    public class Spiral : SplinePrimitive
    {
        public float startRadius = 1f;
        public float endRadius = 1f;
        public float stretch = 1f;
        public int iterations = 3;
        public AnimationCurve curve = new AnimationCurve();

        protected override void Generate()
        {
            base.Generate();
            type = Spline.Type.Bezier;
            closed = false;
            CreatePoints(iterations * 4 + 1, SplinePoint.Type.SmoothMirrored);
            float radiusDelta = Mathf.Abs(endRadius - startRadius);
            float radiusDeltaPercent = radiusDelta / Mathf.Max(Mathf.Abs(endRadius), Mathf.Abs(startRadius));
            float multiplier = 1f;
            if (endRadius > startRadius) multiplier = -1;
            float angle = 0f;
            float str = 0f;
            for (int i = 0; i <= iterations * 4; i++)
            {
                float percent = curve.Evaluate((float)i / (iterations * 4));
                float radius = Mathf.Lerp(startRadius, endRadius, percent);
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
                points[i].position = rot * Vector3.up / 2f * radius + Vector3.forward * str;
                Quaternion tangentRot = Quaternion.identity;
                if (multiplier > 0) tangentRot = Quaternion.AngleAxis(Mathf.Lerp(0f, 90f * 0.16f, radiusDeltaPercent * percent), Vector3.forward);
                else tangentRot = Quaternion.AngleAxis(Mathf.Lerp(0f, -90f * 0.16f, (1f - percent) * radiusDeltaPercent), Vector3.forward);
                points[i].tangent = points[i].position + (tangentRot * rot * Vector3.right * radius - Vector3.forward * stretch / 4f) * 2 * (Mathf.Sqrt(2f) - 1f) / 3f;
                points[i].tangent2 = points[i].position - (points[i].tangent - points[i].position);
                str += stretch / 4f;
                angle += 90f;
            }
        }
    }
}
