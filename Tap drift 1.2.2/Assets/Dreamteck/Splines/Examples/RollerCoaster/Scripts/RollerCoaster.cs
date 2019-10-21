using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines.Examples
{
    public class RollerCoaster : MonoBehaviour
    {
        [System.Serializable]
        public class CoasterSound
        {
            public float startPercent = 0f;
            public float endPercent = 1f;
            public AudioSource source;
            public float startPitch = 1f;
            public float endPitch = 1f;
        }

        public float speed = 10f;
        public float minSpeed = 1f;
        public float maxSpeed = 20f;
        public float frictionForce = 0.1f;
        public float gravityForce = 1f;
        public float slopeRange = 60f;
        SplineFollower follower;
        public AnimationCurve speedGain;
        public AnimationCurve speedLoss;
        public float brakeSpeed = 0f;
        public float brakeReleaseSpeed = 0f;

        private float brakeTime = 0f;
        private float brakeForce = 0f;
        private float addForce = 0f;

        public CoasterSound[] sounds;
        public AudioSource brakeSound;
        public AudioSource boostSound;
        public float soundFadeLength = 0.15f;


        // Use this for initialization
        void Start()
        {
            follower = GetComponent<SplineFollower>();
            follower.onEndReached += OnEndReached;
            Cursor.lockState = CursorLockMode.Locked;
        }

        void OnEndReached()
        {
            //Detect when the wagon has reached the end of the spline
            List<SplineComputer> computers = new List<SplineComputer>();
            List<int> connections = new List<int>();
            List<int> connected = new List<int>();
            follower.computer.GetConnectedComputers(computers, connections, connected, 1.0, follower.direction, true); //Get the avaiable connected computers at the end of the spline
            if (computers.Count == 0) return;
            //Do not select computers that are not connected at the first point so that we don't reverse direction
            for (int i = 0; i < computers.Count; i++)
            {
                if(connected[i] != 0)
                {
                    computers.RemoveAt(i);
                    connections.RemoveAt(i);
                    connected.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            float distance = follower.CalculateLength(0.0, follower.result.percent); //Get the excess distance after looping
            follower.computer = computers[Random.Range(0, computers.Count)]; //Change the spline computer to the new spline
            follower.SetDistance(distance); //Set the excess distance along the new spline
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;
            float dot = Vector3.Dot(this.transform.forward, Vector3.down);
            float dotPercent = Mathf.Lerp(-slopeRange / 90f, slopeRange / 90f, (dot + 1f) / 2f);
            speed -= Time.deltaTime * frictionForce * (1f - brakeForce);
            float speedAdd = 0f;
            float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
            if (dotPercent > 0f)
            {
                speedAdd = gravityForce * dotPercent * speedGain.Evaluate(speedPercent) * Time.deltaTime;
            }
            else
            {
                speedAdd = gravityForce * dotPercent * speedLoss.Evaluate(1f-speedPercent) * Time.deltaTime;
            }
            speed += speedAdd * (1f-brakeForce);
            speed = Mathf.Clamp(speed, minSpeed, maxSpeed);
            if (addForce > 0f) {
                float lastAdd = addForce;
                addForce = Mathf.MoveTowards(addForce, 0f, Time.deltaTime * 30f);
                speed += lastAdd - addForce;
             }
            follower.followSpeed = speed;
            follower.followSpeed *= (1f - brakeForce);
            if (brakeTime > Time.time) brakeForce = Mathf.MoveTowards(brakeForce, 1f, Time.deltaTime * brakeSpeed);
            else brakeForce = Mathf.MoveTowards(brakeForce, 0f, Time.deltaTime * brakeReleaseSpeed);

            speedPercent = Mathf.Clamp01(speed/maxSpeed)*(1f-brakeForce);
            for (int i = 0; i < sounds.Length; i++) {
                if (speedPercent < sounds[i].startPercent - soundFadeLength || speedPercent > sounds[i].endPercent + soundFadeLength)
                {
                    if (sounds[i].source.isPlaying) sounds[i].source.Pause();
                    continue;
                }
                else if (!sounds[i].source.isPlaying) sounds[i].source.UnPause();
                float volume = 1f;
                if (speedPercent < sounds[i].startPercent+soundFadeLength) volume = Mathf.InverseLerp(sounds[i].startPercent, sounds[i].startPercent+soundFadeLength, speedPercent);
                else if (speedPercent > sounds[i].endPercent) volume = Mathf.InverseLerp(sounds[i].endPercent + soundFadeLength, sounds[i].endPercent, speedPercent);
                float pitchPercent = Mathf.InverseLerp(sounds[i].startPercent, sounds[i].endPercent, speedPercent);
                sounds[i].source.volume = volume;
                sounds[i].source.pitch = Mathf.Lerp(sounds[i].startPitch, sounds[i].endPitch, pitchPercent);
            }

        }

        public void AddBrake(float time)
        {
            brakeTime = Time.time + time;
            brakeSound.Stop();
            brakeSound.Play();
        }

        public void RemoveBrake()
        {
            brakeTime = 0f;
        }

        public void AddForce(float amount)
        {
            addForce = amount;
            boostSound.Stop();
            boostSound.Play();
        }
    }
}
