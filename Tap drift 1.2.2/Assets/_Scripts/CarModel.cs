using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarModel : MonoBehaviour
{
    public GameObject leftWheel;
    public GameObject rightWheel;
    public GameObject rearWheels;

    public float rotation = 0;

    public GameObject trails;
    public GameObject exhausts;

    TrailRenderer[] trailsArray;
    ParticleSystem[] exhaustArray;

    void Start()
    {

        if (gameObject.tag != "CitizenCar")
        {
            trailsArray = trails.GetComponentsInChildren<TrailRenderer>();
            exhaustArray = exhausts.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem pr in exhaustArray)
            {
                if (pr.isPlaying) pr.Stop();
            }
        } else
        {
            Destroy(trails);
            Destroy(exhausts);
        }
    }
    void Update()
    {

        if (gameObject.tag != "CitizenCar" && transform.parent.GetComponent<Player>().gameStarted == false)
            return;

        rotation += GameManager.instance.Player.GetComponent<Player>().currentSpeed * 0.01f * Time.timeScale;

        leftWheel.transform.localRotation = Quaternion.EulerRotation(rotation + leftWheel.transform.localRotation.x, transform.localRotation.y * 2, 0);
        rightWheel.transform.localRotation = Quaternion.EulerRotation(rotation + rightWheel.transform.localRotation.x, transform.localRotation.y * 2, 0);

        
        rearWheels.transform.Rotate(Vector3.right, GameManager.instance.Player.GetComponent<Player>().currentSpeed * 0.5f * Time.timeScale);

        if (gameObject.tag != "CitizenCar")
        {
            if (transform.parent.GetComponent<Player>().driftLeft || transform.parent.GetComponent<Player>().driftRight)
            {
                foreach (TrailRenderer tr in trailsArray)
                {
                    tr.emitting = true;
                }
                foreach (ParticleSystem pr in exhaustArray)
                {
                    if (pr.isStopped) pr.Play();
                }
            }
            else
            {
                foreach (TrailRenderer tr in trailsArray)
                {
                    tr.emitting = false;
                }
                foreach (ParticleSystem pr in exhaustArray)
                {
                    if (pr.isPlaying) pr.Stop();
                }
            }
        }
    }
}
