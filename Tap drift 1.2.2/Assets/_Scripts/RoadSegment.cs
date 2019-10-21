using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class RoadSegment : MonoBehaviour
{
    public GameObject citizenCarPrefab;
    public GameObject driftFuelPrefab;
    public GameObject boosterPrefab;
    public GameObject scoreMultPrefab;
    public GameObject crystalPrefab;
    void Start()
    {
        transform.position = GameManager.instance.lastPoint.position;

        SplinePoint[] pointsArray = new SplinePoint[GameManager.instance.numberOfPointsInRoadSegment];

        for (int i = 0; i < GameManager.instance.numberOfPointsInRoadSegment; i++)
        {
            if(i == 0) //First Point
            {
                pointsArray[i].SetPosition(transform.position);
                pointsArray[i].size = 1;
                pointsArray[i].color = Color.white;
            } else if (i == 1) //Second Point
            {
                pointsArray[i].SetPosition(transform.position + Vector3.forward * GameManager.instance.distanceMultiplier / 2);
                pointsArray[i].size = 1;
                pointsArray[i].color = Color.white;
            }
            else if (i == pointsArray.Length - 2 || i == pointsArray.Length - 1) //Last two points
            {
                pointsArray[i].SetPosition(transform.position + i * Vector3.forward * GameManager.instance.distanceMultiplier);
                pointsArray[i].size = 1;
                pointsArray[i].color = Color.white;
            }
            else // All the rest points
            {
                pointsArray[i].SetPosition(generatedPosition(i));
                pointsArray[i].size = 1;
                pointsArray[i].color = Color.white;
            }
        }

        GetComponent<SplineComputer>().SetPoints(pointsArray);
        GetComponent<SplineComputer>().precision = 0.98;
        GameManager.instance.lastPoint = pointsArray[pointsArray.Length - 1];

        //Generate citizen cars
        StartCoroutine(SpawnCitizenCars());

        //Generate drift fuel
        StartCoroutine(SpawnDriftFuel());

        //Generate score multiplier
        for (int i = 0; i < GameManager.instance.numberOfScoreMultipliers; i++)
        {
            GameObject scoreMult = Instantiate(scoreMultPrefab);
            scoreMult.transform.parent = gameObject.transform;
        }

        //Generate boosters
        for (int i = 0; i < GameManager.instance.numberOfBoosters; i++)
        {
            GameObject booster = Instantiate(boosterPrefab);
            booster.transform.parent = gameObject.transform;
        }

        //Generate crystals
        for (int i = 0; i < GameManager.instance.numberOfCrystals; i++)
        {
            float x = Random.Range(0f, 1f);
            if (x >= 0.3f)
            {
                GameObject crystal = Instantiate(crystalPrefab);
                crystal.transform.parent = gameObject.transform;
            }
        }
    }

    IEnumerator SpawnCitizenCars()
    {
        for (int i = 0; i < GameManager.instance.numberOfCitizenCars; i++)
        {
            GameObject citizenCar = Instantiate(citizenCarPrefab);
            citizenCar.transform.parent = gameObject.transform;
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator SpawnDriftFuel ()
    {
        for (int i = 0; i < GameManager.instance.numberOfDriftFuels; i++)
        {
            GameObject driftFuel = Instantiate(driftFuelPrefab);
            driftFuel.transform.parent = gameObject.transform;
            yield return new WaitForSeconds(0.05f);
        }
    }

    Vector3 generatedPosition(int i)
    {
        Vector3 begPosition = transform.position;
        Vector3 fwd = transform.forward;
        Vector3 sides = transform.right * Random.Range(-1f, 1f);
        Vector3 height = transform.up * Random.Range(-1f, 1f);
        return begPosition + i * fwd * GameManager.instance.distanceMultiplier + sides * GameManager.instance.sidesMultiplier + height * GameManager.instance.heightMultiplier;
    } 
}
