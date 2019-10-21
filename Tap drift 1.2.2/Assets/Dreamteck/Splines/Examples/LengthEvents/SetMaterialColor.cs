using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines.Examples
{
    public class SetMaterialColor : MonoBehaviour
    {
        public Color[] colors;
        SplineRenderer rend;

        private void Start()
        {
            rend = GetComponent<SplineRenderer>();
        }

        public void SetColor(int index)
        {
            if (!Application.isPlaying) return;
            rend.color = colors[index];
        }

    }
}
