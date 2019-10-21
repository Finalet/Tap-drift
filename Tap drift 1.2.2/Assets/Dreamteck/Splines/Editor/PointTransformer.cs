using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines {
    public class PointTransformer
    {
        private SplinePoint[] originalPoints = new SplinePoint[0];
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;
        public Vector3 center = Vector3.zero;
        private int[] selected = new int[0];
        private Quaternion orientation = Quaternion.identity;


        public PointTransformer(SplinePoint[] points, List<int> selection, Quaternion or)
        {
            rotation = or;
            orientation = or;
            selected = selection.ToArray();
            originalPoints = points;
            CalculateCenter(points);
        }

        void CalculateCenter(SplinePoint[] points)
        {
            center = Vector3.zero;
            for (int i = 0; i < selected.Length; i++)
            {
                center += points[selected[i]].position;
            }
            center /= selected.Length;
        }

        public SplinePoint[] GetRotated(bool rotateNormal, bool rotateTangents)
        {
            SplinePoint[] points = new SplinePoint[originalPoints.Length];
            for (int i = 0; i < points.Length; i++) points[i] = originalPoints[i];
            Quaternion rot = rotation * Quaternion.Inverse(orientation);
            for (int i = 0; i < selected.Length; i++)
            {
                points[selected[i]].position = originalPoints[selected[i]].position - center;
                points[selected[i]].position = rot * points[selected[i]].position;
                points[selected[i]].position += center;
                if (rotateTangents)
                {
                    points[selected[i]].tangent = originalPoints[selected[i]].tangent - center;
                    points[selected[i]].tangent2 = originalPoints[selected[i]].tangent2 - center;
                    points[selected[i]].tangent = rot * points[selected[i]].tangent;
                    points[selected[i]].tangent2 = rot * points[selected[i]].tangent2;
                    points[selected[i]].tangent += center;
                    points[selected[i]].tangent2 += center;
                }
                else
                {
                    points[selected[i]].tangent = points[selected[i]].position + (originalPoints[selected[i]].tangent - originalPoints[selected[i]].position);
                    points[selected[i]].tangent2 = points[selected[i]].position + (originalPoints[selected[i]].tangent2 - originalPoints[selected[i]].position);
                }
                if (rotateNormal) points[selected[i]].normal = rot * originalPoints[selected[i]].normal;
            }
            return points;
        }

        public SplinePoint[] GetScaled(bool scaleSize, bool scaleTangents)
        {
            SplinePoint[] points = new SplinePoint[originalPoints.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = originalPoints[i];
            }
            for (int i = 0; i < selected.Length; i++)
            {
                points[selected[i]].position = originalPoints[selected[i]].position - center;
                points[selected[i]].position.x *= scale.x;
                points[selected[i]].position.y *= scale.y;
                points[selected[i]].position.z *= scale.z;
                points[selected[i]].position += center;
                if (scaleTangents)
                {
                    points[selected[i]].tangent = originalPoints[selected[i]].tangent - center;
                    points[selected[i]].tangent2 = originalPoints[selected[i]].tangent2 - center;
                    points[selected[i]].tangent.x *= scale.x;
                    points[selected[i]].tangent.y *= scale.y;
                    points[selected[i]].tangent.z *= scale.z;
                    points[selected[i]].tangent2.x *= scale.x;
                    points[selected[i]].tangent2.y *= scale.y;
                    points[selected[i]].tangent2.z *= scale.z;
                    points[selected[i]].tangent += center;
                    points[selected[i]].tangent2 += center;
                }
                else
                {
                    points[selected[i]].tangent = points[selected[i]].position + (originalPoints[selected[i]].tangent - originalPoints[selected[i]].position);
                    points[selected[i]].tangent2 = points[selected[i]].position + (originalPoints[selected[i]].tangent2 - originalPoints[selected[i]].position);
                }
                if (scaleSize) points[selected[i]].size = originalPoints[selected[i]].size * (scale.x + scale.y + scale.z) / 3f;
            }
            return points;
        }
    }
}
