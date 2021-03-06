﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class citizenModelTrigger : MonoBehaviour
{

    bool triggered;

    void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "CitizenCar")
        {
            triggered = true;
            StartCoroutine(Recheck());
        } else if (other.gameObject.tag == "Player")
        {
            if (!other.transform.parent.GetComponent<Player>().bulldozerDeployed)
                GameManager.instance.Loose();
        } else if (other.gameObject.tag == "Bulldozer") {
            StartCoroutine(Bulldoze());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "CitizenCar")
        {
            triggered = false;
        }
    }

    IEnumerator Recheck()
    {
        yield return new WaitForSeconds(0.2f);
        if (triggered == true)
        {
            Destroy(transform.parent.gameObject);
        }
    }

    IEnumerator Bulldoze () {
        Taptic.Heavy();
        
        GameManager.instance.Player.GetComponent<Player>().timesSmashed ++;
        StartCoroutine(GameManager.instance.Player.GetComponent<Player>().CameraShake(0.3f, 0.1f));

        Rigidbody rb = GetComponent<Rigidbody>();
        transform.parent.GetComponent<SplineFollower>().enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        rb.isKinematic = false;
        float speed = GameManager.instance.Player.GetComponent<Player>().currentSpeed;

        int x, y, z, torque;
        if (transform.parent.GetComponent<CitizenCar>().line == "right") {
            x = 30; y = 25; z = 40; torque = -500;
        } else if (transform.parent.GetComponent<CitizenCar>().line == "left") {
            x = -30; y = 25; z = 40; torque = 500; 
        } else {
            float x1 = Random.Range(0f, 1f);
            if (x1>= 0.5f) {
                x = 45; y = 25; z = 40; torque = -500; 
            } else {
                x = -45; y = 25; z = 40; torque = 500; 
            }
        }

        rb.AddRelativeForce(new Vector3(x, y, z) * speed * 0.015f, ForceMode.Impulse);
        rb.AddRelativeTorque(new Vector3(0, 0, torque)); 

        GameManager.instance.Player.GetComponent<Player>().bulldozer.GetComponent<Animation>().Play("bulldozerSmash");
        if (GameManager.instance.Player.GetComponent<Player>().timesSmashed < GameManager.instance.GetComponent<UpgradesContainer>().bulldozerSmashTimes) {
            yield return new WaitForSeconds(0.2167f);
            GameManager.instance.Player.GetComponent<Player>().bulldozer.GetComponent<Animation>().Stop();
        }
        yield return new WaitForSeconds(0.33f);
        if (GameManager.instance.Player.GetComponent<Player>().timesSmashed >= GameManager.instance.GetComponent<UpgradesContainer>().bulldozerSmashTimes) {
            GameManager.instance.Player.GetComponent<Player>().timesSmashed = 0;
            GameManager.instance.Player.GetComponent<Player>().bulldozerDeployed = false;
        }
        Destroy(transform.parent.gameObject);
        
    }
}
