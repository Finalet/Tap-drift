using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crystals : MonoBehaviour
{
    public int crystalAmount;

    void Update()
    {
        crystalAmount = Mathf.Clamp(crystalAmount, 0, 9999);
    }

    void Awake()
    {
        if (ES3.KeyExists("crystals"))
            crystalAmount = ES3.Load<int>("crystals");
    }

    public void AddCrystal (int amount)
    {
        crystalAmount += amount;
        ES3.Save<int>("crystals", crystalAmount);
    }
    public void RemoveCrystal (int amount)
    {
        crystalAmount -= amount;
        ES3.Save<int>("crystals", crystalAmount);
    }
}
