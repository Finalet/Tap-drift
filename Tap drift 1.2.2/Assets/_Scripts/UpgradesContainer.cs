using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradesContainer : MonoBehaviour
{
    public int upgradeMultiplier;
    public int upgradeMultiplierPrice;

    void Awake() {
        if (ES3.KeyExists("upgradeMultiplier"))
            upgradeMultiplier = ES3.Load<int>("upgradeMultiplier");
        else 
            upgradeMultiplier = 1;
    }
    public void PurchaseUpgradeMultiplier() {
        Taptic.Selection();
        upgradeMultiplier ++;
        GetComponent<Crystals>().RemoveCrystal(upgradeMultiplierPrice);
        ES3.Save<int>("upgradeMultiplier", upgradeMultiplier);
    }

    void Update() {
        CalculatePrices();
    }
    void CalculatePrices () {
        upgradeMultiplierPrice = upgradeMultiplier * 40;
    }
}
