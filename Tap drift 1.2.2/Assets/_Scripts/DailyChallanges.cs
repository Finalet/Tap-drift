using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyChallanges : MonoBehaviour
{
    public static DailyChallanges instance;
    public enum Type {
        Score,
        DriftScore,
        CrashTimes,
        Fuel
    };

    [System.Serializable] public struct Challage {
        public string description;
        public int reward;
        public Type type;
        public float progress;
    }
    
    public List<Challage> AllChallanges;

    public GameObject Challage1;
    float challange1Progress; 
    public GameObject Challage2;
    float challange2Progress; 

    Challage[] Array;
    int numberOfChallanges = 2;

    int lastDay;

    int score;
    int driftScore;
    int crashTimes;

    void Awake() {
        instance = this;
    }
    void Update() {

        lastDay = GameManager.instance.lastDay;

        if (Array == null) 
            return;

        CheckDailys();

        Challage1.transform.GetChild(0).GetComponent<Text>().text = Array[0].description;
        Challage1.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Array[0].reward.ToString();
        Challage1.transform.GetChild(1).GetChild(1).GetComponent<Image>().fillAmount = challange1Progress/Array[0].progress;

        Challage2.transform.GetChild(0).GetComponent<Text>().text = Array[1].description;
        Challage2.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Array[1].reward.ToString();
        Challage2.transform.GetChild(1).GetChild(1).GetComponent<Image>().fillAmount = challange2Progress/Array[1].progress;
    }
    
    public void GenerateArray () {

        Array = new Challage[numberOfChallanges];
        
        for (int i = 0; i < numberOfChallanges; i++)
        {
            int randChall = Random.Range(0, AllChallanges.Count);
            Array.SetValue(AllChallanges[randChall], i);
            AllChallanges.RemoveAt(randChall);
        }
        
        ES3.Save<Challage[]>("dailyChallangesArray", Array);
    }
    public void LoadArray () {
        if (ES3.KeyExists("dailyChallangesArray")) Array = ES3.Load<Challage[]>("dailyChallangesArray");

        if (ES3.KeyExists("score", "dailyChallanges")) score = ES3.Load<int>("score", "dailyChallanges");
        if (ES3.KeyExists("driftScore", "dailyChallanges")) driftScore = ES3.Load<int>("driftScore", "dailyChallanges");
        if (ES3.KeyExists("crashTimes", "dailyChallanges")) crashTimes = ES3.Load<int>("crashTimes", "dailyChallanges");
    }
    
    void CheckDailys () {
        if (Array[0].type == Type.Score) challange1Progress = score;
        else if (Array[0].type == Type.DriftScore) challange1Progress = driftScore;
        else if (Array[0].type == Type.CrashTimes) challange1Progress = crashTimes;
        else if (Array[0].type == Type.Fuel) challange1Progress = 0;

        if (Array[1].type == Type.Score) challange2Progress = score;
        else if (Array[1].type == Type.DriftScore) challange2Progress = driftScore;
        else if (Array[1].type == Type.CrashTimes) challange2Progress = crashTimes;
        else if (Array[1].type == Type.Fuel) challange2Progress = 0;
    }

    public void AddScore (int amount) {
        score += amount;
        ES3.Save<int>("score", score, "dailyChallanges");
    }
    public void AddDriftScore (int amount) {
        driftScore += amount;
        ES3.Save<int>("driftScore", driftScore, "dailyChallanges");
    }
    public void AddCrashTimes (int amount) {
        crashTimes += amount;
        ES3.Save<int>("crashTimes", crashTimes, "dailyChallanges");
    }
}