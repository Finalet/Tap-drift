using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carPoolStats : MonoBehaviour
{
    public int carsInLeftRow;
    public int carsInMiddleRow;
    public int carsInRightRow;


    void Start()
    {
        InvokeRepeating("Check", 2, 1);
    }

    void Check()
    {
        carsInMiddleRow = 0;
        carsInRightRow = 0;
        carsInLeftRow = 0;
        CitizenCar[] cars = GetComponentsInChildren<CitizenCar>();
        foreach(CitizenCar car in cars)
        {
            if (car.line == "center")
                carsInMiddleRow++;
            else if (car.line == "right")
                carsInRightRow++;
            else if (car.line == "left")
                carsInLeftRow++;
        }
    }
}
