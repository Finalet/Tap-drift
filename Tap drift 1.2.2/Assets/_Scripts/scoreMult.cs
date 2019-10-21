using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class scoreMult : MonoBehaviour
{
    SplineFollower follower;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Taptic.Medium();

            GameManager.instance.scoreMultTimer = 10;
            gameObject.GetComponent<BoxCollider>().enabled = false;
            StartCoroutine(CollectAnim());
        }
    }

    void Awake()
    {
        follower = transform.parent.GetComponent<SplineFollower>();
    }


    float distance;
    void Start()
    {
        follower.computer = gameObject.GetComponentInParent<SplineComputer>();
        follower.followSpeed = 0;
        distance = Random.Range(0f, 1f);
        follower.SetPercent(distance);
        transform.localPosition = new Vector3(randomLine(), transform.localPosition.y, transform.localPosition.z); //Random line of the road
    }

    float randomLine()
    {
        int x = Random.Range(0, 3);
        if (x == 0)
        {
            float X = 0;
            return X;
        }
        else if (x == 1)
        {
            float X = 2.12f;
            return X;
        }
        else if (x == 2)
        {
            float X = -2.12f;
            return X;
        }
        else
        {
            return 0;
        }
    }

    IEnumerator CollectAnim () {
        float duration = Time.time + 0.07f;
        while (Time.time <= duration) {
            transform.position += new Vector3(0, 0.5f, 0);
            yield return new WaitForSeconds(0.01f);
        }
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        Destroy(transform.parent.gameObject, 11);
    }
}
