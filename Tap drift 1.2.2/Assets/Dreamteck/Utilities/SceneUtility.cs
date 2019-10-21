using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Dreamteck
{
    public static class SceneUtility
    {
        public static void GetChildrenRecursively(Transform current, ref List<Transform> transformList)
        {
            transformList.Add(current);
            foreach (Transform child in current) GetChildrenRecursively(child, ref transformList);
        }
    }
}
