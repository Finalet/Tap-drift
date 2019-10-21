using UnityEngine;
using System.Collections;

namespace Dreamteck.Splines.Examples
{
    public class Raycaster : MonoBehaviour
    {
        SplineComputer comp;
        public double from = 0.0;
        public double to = 1.0;
        public double resolution = 1.0;
        public LayerMask mask;

        private SplineUser user;
        // Use this for initialization
        void Start()
        {
            comp = GetComponent<SplineComputer>();
            user = GetComponent<SplineUser>();
        }

        // Update is called once per frame
        void Update()
        {
            user.clipFrom = from;
            RaycastHit hit;
            double percent = 0.0;
            if (comp.Raycast(out hit, out percent, mask, resolution, from, to))
            {
                user.clipTo = percent;
            }
            else user.clipTo = DMath.Move(user.clipTo, to, Time.deltaTime);
        }
    }
}