using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamteck
{
    public static class TransformUtility
    {
        public static Vector3 GetPosition(ref Matrix4x4 m)
        {
            return m.GetColumn(3);
        }

        public static Quaternion GetRotation(ref Matrix4x4 m)
        {
            return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        }

        public static Vector3 GetScale(ref Matrix4x4 m)
        {
            return new Vector3(m.GetColumn(0).magnitude, m.GetColumn(1).magnitude, m.GetColumn(2).magnitude);
        }

        public static void SetPosition(ref Matrix4x4 m, ref Vector3 p)
        {
            m.SetColumn(3, new Vector4(p.x, p.y, p.z, 1f));
        }
    }
}
