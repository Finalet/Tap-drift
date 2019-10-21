﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck.Splines.Primitives
{
    public class Star : SplinePrimitive
    {
        public float radius = 1f;
        public float depth = 0.5f;
        public int sides = 5;

        protected override void Generate()
        {
            base.Generate();
            type = Spline.Type.Linear;
            closed = true;
            CreatePoints(sides * 2 + 1, SplinePoint.Type.SmoothMirrored);
            float innerRadius = radius * depth;
            for (int i = 0; i < sides * 2; i++)
            {
                float percent = (float)i / (float)(sides * 2);
                Vector3 pos = Quaternion.AngleAxis(180 + 360f * percent, Vector3.forward) * Vector3.right * ((float)i % 2f == 0 ? radius : innerRadius);
                points[i].SetPosition(pos);
            }
            points[points.Length - 1] = points[0];
        }
    }
}