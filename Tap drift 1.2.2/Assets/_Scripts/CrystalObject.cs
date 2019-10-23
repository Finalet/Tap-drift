using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class CrystalObject : MonoBehaviour
{
    public GameObject crystalPrefab;
    public bool additional;

    SplineFollower follower;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Taptic.Medium();
            GameManager.instance.GetComponent<Crystals>().AddCrystal(1);
            StartCoroutine(CollectAnim());
        }
    }

    void Awake()
    {
        follower = transform.parent.GetComponent<SplineFollower>();
    }

    void Update()
    {
        transform.Rotate(Vector3.up, 1.5f);
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
            transform.localPosition = new Vector3(randomLine(), transform.localPosition.y, transform.localPosition.z); //Random line of the road
            StartCoroutine(lineupType());
        }
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

    public void PlaceThreeInARow (float dis, Vector3 pos)
    {
        follower.computer = gameObject.GetComponentInParent<SplineComputer>();
        follower.followSpeed = 0;
        transform.localPosition = pos;
        follower.SetPercent(dis);   
    }

    IEnumerator lineupType()
    {
        int x = Random.Range(0, 3);
        if (x == 0) //3 in a row
        {
            yield return new WaitForSeconds(0.05f);
            GameObject nextCrystal = Instantiate(crystalPrefab, transform.parent.transform.parent);
            nextCrystal.transform.GetChild(0).GetComponent<CrystalObject>().additional = true;
            nextCrystal.transform.GetChild(0).GetComponent<CrystalObject>().PlaceThreeInARow(distance + 0.015f, transform.localPosition);

            yield return new WaitForSeconds(0.05f);
            GameObject nextCrystal2 = Instantiate(crystalPrefab, transform.parent.transform.parent);
            nextCrystal2.transform.GetChild(0).GetComponent<CrystalObject>().additional = true;
            nextCrystal2.transform.GetChild(0).GetComponent<CrystalObject>().PlaceThreeInARow(distance + 0.03f, transform.localPosition);
        }
        else if (x == 1) //2 in a row
        {
            yield return new WaitForSeconds(0.05f);
            GameObject nextCrystal = Instantiate(crystalPrefab, transform.parent.transform.parent);
            nextCrystal.transform.GetChild(0).GetComponent<CrystalObject>().additional = true;
            nextCrystal.transform.GetChild(0).GetComponent<CrystalObject>().PlaceThreeInARow(distance + 0.015f, transform.localPosition);
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
