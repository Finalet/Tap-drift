using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Dreamteck.Splines.Examples
{

    public class MorphSlider : MonoBehaviour
    {
        public Slider slider;
        [Range(0f, 1f)]
        public float percent = 0f;
        private float lastPercent = 0f;
        SplineComputer computer;
        // Use this for initialization
        void Start()
        {
            computer = GetComponent<SplineComputer>();
        }

        public void OnChange()
        {
            percent = lastPercent = slider.value;
            computer.SetMorphState(percent);
        }

        // Update is called once per frame
        void Update()
        {
            if(percent != lastPercent)
            {
                if(slider != null) slider.value = percent;
                computer.SetMorphState(percent);
                lastPercent = percent;
            }
        }
    }
}
