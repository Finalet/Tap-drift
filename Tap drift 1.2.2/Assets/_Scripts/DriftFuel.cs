using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class DriftFuel : MonoBehaviour
{
    SplineFollower follower;
    public GameObject driftFuelPrefab;

    public bool additional;
    public string line;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Taptic.Medium();

            other.transform.parent.GetComponent<Player>().driftFuel += GameManager.instance.driftFuelAddition;
            
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
        if (additional == false)
        {
            follower.computer = gameObject.GetComponentInParent<SplineComputer>();
            follower.followSpeed = 0;
            distance = Random.Range(0f, 1f);
            follower.SetPercent(distance);
            //model.transform.localPosition = new Vector3(Random.Range(-2.4f, 2.4f), model.transform.localPosition.y, model.transform.localPosition.z); //Random place on the road
            transform.localPosition = new Vector3(randomLine(), transform.localPosition.y, transform.localPosition.z); //Random line of the road
            StartCoroutine(lineupType());
        }
    }
    void Update()
    {
        transform.Rotate(Vector3.up, 3);
    }


    public void PlaceThreeInARow (float dis, Vector3 pos)
    {
        follower.computer = gameObject.GetComponentInParent<SplineComputer>();
        follower.followSpeed = 0;
        transform.localPosition = pos;
        follower.SetPercent(dis);   
    }
    public void PlaceOneOnEachRow(float dis, float line)
    {
        follower.computer = gameObject.GetComponentInParent<SplineComputer>();
        follower.followSpeed = 0;
        transform.localPosition = new Vector3(line, transform.localPosition.y, transform.localPosition.z);
        follower.SetPercent(dis);
    }

    float randomLine()
    {
        int x = Random.Range(0, 3);
        if (x == 0)
        {
            float X = 0;
            line = "center";
            return X;
        }
        else if (x == 1)
        {
            float X = 2.12f;
            line = "right";
            return X;
        }
        else if (x == 2)
        {
            float X = -2.12f;
            line = "left";
            return X;
        }
        else
        {
            return 0;
        }
    }

    IEnumerator lineupType ()
    {
        int x = Random.Range(0, 3);
        if (x == 0) //3 in a row
        {
            yield return new WaitForSeconds(0.05f);
            GameObject nextFuel = Instantiate(driftFuelPrefab, transform.parent.transform.parent);
            nextFuel.transform.GetChild(0).GetComponent<DriftFuel>().additional = true;
            nextFuel.transform.GetChild(0).GetComponent<DriftFuel>().PlaceThreeInARow(distance + 0.015f, transform.localPosition);

            yield return new WaitForSeconds(0.05f);
            GameObject nextFuel2 = Instantiate(driftFuelPrefab, transform.parent.transform.parent);
            nextFuel2.transform.GetChild(0).GetComponent<DriftFuel>().additional = true;
            nextFuel2.transform.GetChild(0).GetComponent<DriftFuel>().PlaceThreeInARow(distance + 0.03f, transform.localPosition);
        }
        else if (x == 1) //One on each line
        {
            float line1 = 0;
            float line2 = 0;
            if (line == "center")
            {
                line1 = -2.12f;
                line2 = 2.12f;
            } else if (line == "left")
            {
                line1 = 0;
                line2 = 2.12f;
            }
            else if (line == "right")
            {
                line1 = 0;
                line2 = -2.12f;
            }

            yield return new WaitForSeconds(0.05f);
            GameObject nextFuel = Instantiate(driftFuelPrefab, transform.parent.transform.parent);
            nextFuel.transform.GetChild(0).GetComponent<DriftFuel>().additional = true;
            nextFuel.transform.GetChild(0).GetComponent<DriftFuel>().PlaceOneOnEachRow(distance, line1);

            yield return new WaitForSeconds(0.05f);
            GameObject nextFuel2 = Instantiate(driftFuelPrefab, transform.parent.transform.parent);
            nextFuel2.transform.GetChild(0).GetComponent<DriftFuel>().additional = true;
            nextFuel2.transform.GetChild(0).GetComponent<DriftFuel>().PlaceOneOnEachRow(distance, line2);
        }
        else if (x == 2) //One on two lines
        {
            float line1 = 0;
            if (line == "center")
            {
                line1 = -2.12f;
            }
            else if (line == "left" || line == "right")
            {
                line1 = 0;
            }

            yield return new WaitForSeconds(0.05f);
            GameObject nextFuel = Instantiate(driftFuelPrefab, transform.parent.transform.parent);
            nextFuel.transform.GetChild(0).GetComponent<DriftFuel>().additional = true;
            nextFuel.transform.GetChild(0).GetComponent<DriftFuel>().PlaceOneOnEachRow(distance, line1);
        }
    }

    IEnumerator CollectAnim () {
        float duration = Time.time + 0.07f;
        while (Time.time <= duration) {
            transform.position += new Vector3(0, 0.5f, 0);
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(transform.parent.gameObject);
    }
}
