﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesContainer : MonoBehaviour
{
    [Header("Multiplier")]
    public int upgradeMultiplier;
    public int upgradeMultiplierPrice;

    [Header("Bulldozer")]
    public int bulldozerAmount;
    public int bulldozerPrice;

    [Header("Bulldozer SmashTimes")]
    public int bulldozerSmashTimes;
    public int bulldozerSmashTimesPrice;


    void Awake() {
        if (ES3.KeyExists("upgradeMultiplier"))
            upgradeMultiplier = ES3.Load<int>("upgradeMultiplier");
        else 
            upgradeMultiplier = 1;

        if (ES3.KeyExists("bulldozerAmount"))
            bulldozerAmount = ES3.Load<int>("bulldozerAmount");
        else 
            bulldozerAmount = 0;

        if (ES3.KeyExists("bulldozerSmashTimes"))
            bulldozerSmashTimes = ES3.Load<int>("bulldozerSmashTimes");
        else 
            bulldozerSmashTimes = 1;
    }
    public void PurchaseUpgradeMultiplier() {
        Taptic.Selection();
        upgradeMultiplier ++;
        GetComponent<Crystals>().RemoveCrystal(upgradeMultiplierPrice);
        ES3.Save<int>("upgradeMultiplier", upgradeMultiplier);
    }

    public void PurchaseBulldozer() {
        Taptic.Selection();
        bulldozerAmount ++;
        GetComponent<Crystals>().RemoveCrystal(bulldozerPrice);
        ES3.Save<int>("bulldozerAmount", bulldozerAmount);
    }

    public void PurchaseBulldozerCrashTimes() {
        Taptic.Selection();
        bulldozerSmashTimes ++;
        GetComponent<Crystals>().RemoveCrystal(bulldozerSmashTimesPrice);
        ES3.Save<int>("bulldozerSmashTimes", bulldozerSmashTimes);
    }
    public void ConsumeBulldozer () {
        bulldozerAmount--;
        ES3.Save<int>("bulldozerAmount", bulldozerAmount);
    }

    void Update() {
        CalculatePrices();
    }
    void CalculatePrices () {
        upgradeMultiplierPrice = upgradeMultiplier * 30;
        bulldozerPrice = 10;
        bulldozerSmashTimesPrice = bulldozerSmashTimes * 30;
    }
}
