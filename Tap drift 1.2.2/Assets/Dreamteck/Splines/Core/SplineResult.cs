using UnityEngine;
using Dreamteck;

namespace Dreamteck.Splines{
    [System.Serializable]
	public class SplineResult{
        public Vector3 position = Vector3.zero;
        public Vector3 normal = Vector3.up;
        public Vector3 direction = Vector3.forward;
        public Color color = Color.white;
        public float size = 1f;
        public double percent = 0.0;

        public Quaternion rotation
        {
            get {
                if (normal == direction)
                {
                    if (normal == Vector3.up) return Quaternion.LookRotation(Vector3.up, Vector3.back);
                    else return Quaternion.LookRotation(direction, Vector3.up);
                }
                return Quaternion.LookRotation(direction, normal); }
        }

        public Vector3 right
        {
            get {
                if(normal == direction)
                {
                    if (normal == Vector3.up) return Vector3.right;
                    else return Vector3.Cross(Vector3.up, direction).normalized;
                }
                return Vector3.Cross(normal, direction).normalized; }
        }


        public static SplineResult Lerp(SplineResult a, SplineResult b, float t)
        {
            SplineResult result = new SplineResult();
            Lerp(a, b, t, result);
            return result;
        }

        public static SplineResult Lerp(SplineResult a, SplineResult b, double t)
        {
            SplineResult result = new SplineResult();
            Lerp(a, b, t, result);
            return result;
        }

        public static void Lerp(SplineResult a, SplineResult b, double t, SplineResult target)
        {
            float ft = (float)t;
            target.position = DMath.LerpVector3(a.position, b.position, t);
            target.direction = Vector3.Slerp(a.direction, b.direction, ft);
            target.normal = Vector3.Slerp(a.normal, b.normal, ft);
            target.color = Color.Lerp(a.color, b.color, ft);
            target.size = Mathf.Lerp(a.size, b.size, ft);
            target.percent = DMath.Lerp(a.percent, b.percent, t);
        }

        public static void Lerp(SplineResult a, SplineResult b, float t, SplineResult target)
        {
            target.position = DMath.LerpVector3(a.position, b.position, t);
            target.direction = Vector3.Slerp(a.direction, b.direction, t);
            target.normal = Vector3.Slerp(a.normal, b.normal, t);
            target.color = Color.Lerp(a.color, b.color, t);
            target.size = Mathf.Lerp(a.size, b.size, t);
            target.percent = DMath.Lerp(a.percent, b.percent, t);
        }

        public void Lerp(SplineResult b, double t)
        {
            Lerp(this, b, t, this);
        }

        public void Lerp(SplineResult b, float t)
        {
            Lerp(this, b, t, this);
        }

        public void CopyFrom(SplineResult input)
        {
            position = input.position;
            direction = input.direction;
            normal = input.normal;
            color = input.color;
            size = input.size;
            percent = input.percent;
        }

        public SplineResult()
        {
        }
		
        public SplineResult(Vector3 p, Vector3 n, Vector3 d, Color c, float s, double t)
        {
            position = p;
            normal = n;
            direction = d;
            color = c;
            size = s;
            percent = t;
        }

        public SplineResult(SplineResult input)
        {
            position = input.position;
            normal = input.normal;
            direction = input.direction;
            color = input.color;
            size = input.size;
            percent = input.percent;
        }
	}
}
