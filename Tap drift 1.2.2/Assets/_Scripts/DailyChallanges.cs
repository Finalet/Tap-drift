using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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

    public GameObject CompletedText1;
    public GameObject CompletedText2;

    public GameObject CompletedReward1;
    public GameObject CompletedReward2;

    Challage[] Array;
    int numberOfChallanges = 2;

    int score;
    int driftScore;
    int crashTimes;
    int ranOutOfFuel;
    bool challange1Completed;
    bool challange2Completed;

    NumberFormatInfo nfi;

    void Awake() {
        instance = this;
        nfi = new CultureInfo("ru-RU", false).NumberFormat;
        nfi.NumberDecimalDigits = 0;
    }
    void Update() {
        if (Array == null) 
            return;


        Challage1.transform.GetChild(0).GetComponent<Text>().text = Array[0].description;
        Challage1.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Array[0].reward.ToString();
        Challage1.transform.GetChild(1).GetChild(1).GetComponent<Image>().fillAmount = challange1Progress/Array[0].progress;

        Challage2.transform.GetChild(0).GetComponent<Text>().text = Array[1].description;
        Challage2.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = Array[1].reward.ToString();
        Challage2.transform.GetChild(1).GetChild(1).GetComponent<Image>().fillAmount = challange2Progress/Array[1].progress;

        if (challange1Completed)  {
            CompletedText1.SetActive(true);
            CompletedReward1.SetActive(false);
        } else  {
            CompletedText1.SetActive(false);
            CompletedReward1.SetActive(true);
        }

        if (challange2Completed) {
            CompletedText2.SetActive(true);
            CompletedReward2.SetActive(false);
        } else { 
            CompletedText2.SetActive(false);
            CompletedReward2.SetActive(true);
        }
        
        MatchDailys();
    }
    
    public void GenerateArray () {

        Array = new Challage[numberOfChallanges];
        
        for (int i = 0; i < numberOfChallanges; i++)
        {
            int randChall = Random.Range(0, AllChallanges.Count);
            Array.SetValue(AllChallanges[randChall], i);
            if (AllChallanges[randChall].type == Type.Score) {
                Array[i].progress *= GameManager.instance.GetComponent<UpgradesContainer>().upgradeMultiplier;
                Array[i].description = "Drive for " + Array[i].progress.ToString("N", nfi) + " points";
            } else if (AllChallanges[randChall].type == Type.DriftScore) {
                Array[i].progress *= GameManager.instance.GetComponent<UpgradesContainer>().upgradeMultiplier;
                Array[i].description = "Drift for " + Array[i].progress.ToString("N", nfi) + " points";
            }
            AllChallanges.RemoveAt(randChall);
        }
        
        ES3.Save<Challage[]>("dailyChallangesArray", Array);

        ES3.Save<bool>("challange1completed", false, "dailyChallanges");
        ES3.Save<bool>("challange2completed", false, "dailyChallanges");

    }
    public void LoadArray () {
        if (ES3.KeyExists("dailyChallangesArray")) Array = ES3.Load<Challage[]>("dailyChallangesArray");

        if (ES3.KeyExists("score", "dailyChallanges")) score = ES3.Load<int>("score", "dailyChallanges");
        if (ES3.KeyExists("driftScore", "dailyChallanges")) driftScore = ES3.Load<int>("driftScore", "dailyChallanges");
        if (ES3.KeyExists("crashTimes", "dailyChallanges")) crashTimes = ES3.Load<int>("crashTimes", "dailyChallanges");
        if (ES3.KeyExists("ranOutOfFuel", "dailyChallanges")) ranOutOfFuel = ES3.Load<int>("ranOutOfFuel", "dailyChallanges");


        if (ES3.KeyExists("challange1completed", "dailyChallanges")) challange1Completed = ES3.Load<bool>("challange1completed", "dailyChallanges");
        if (ES3.KeyExists("challange2completed", "dailyChallanges")) challange2Completed = ES3.Load<bool>("challange2completed", "dailyChallanges");
    }
    
    void MatchDailys () {
        if (Array[0].type == Type.Score) challange1Progress = score;
        else if (Array[0].type == Type.DriftScore) challange1Progress = driftScore;
        else if (Array[0].type == Type.CrashTimes) challange1Progress = crashTimes;
        else if (Array[0].type == Type.Fuel) challange1Progress = ranOutOfFuel;

        if (Array[1].type == Type.Score) challange2Progress = score;
        else if (Array[1].type == Type.DriftScore) challange2Progress = driftScore;
        else if (Array[1].type == Type.CrashTimes) challange2Progress = crashTimes;
        else if (Array[1].type == Type.Fuel) challange2Progress = ranOutOfFuel;
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
    public void RunOutOfFuel() {
        ranOutOfFuel ++;
        ES3.Save<int>("ranOutOfFuel", ranOutOfFuel, "dailyChallanges");
    }

    public void CheckDailys () {
        if (challange1Progress >= Array[0].progress)
            Reward1();
        
        if (challange2Progress >= Array[1].progress)
            Reward2();
    }

    void Reward1(){
        if (challange1Completed) return;

        GameManager.instance.GetComponent<Crystals>().AddCrystal(Array[0].reward);
        challange1Completed = true;
        ES3.Save<bool>("challange1completed", true, "dailyChallanges");
    }
    void Reward2(){
        if (challange2Completed) return;
        
        GameManager.instance.GetComponent<Crystals>().AddCrystal(Array[1].reward);
        challange2Completed = true;
        ES3.Save<bool>("challange2completed", true, "dailyChallanges");
    }


    public void OnOffPanel () {
        if (GetComponent<RectTransform>().localScale.x == 0.5f) {
            GetComponent<RectTransform>().localScale = Vector3.zero;
        } else {
            GetComponent<RectTransform>().localScale = new Vector3(0.5f,0.5f,0.5f);
        }
    }
    public void FullScreen () {
        if (GetComponent<RectTransform>().localScale.x == 0.5f) {
            GetComponent<RectTransform>().localScale = new Vector3 (1,1,1);
            GetComponent<RectTransform>().anchoredPosition = new Vector2 (0,200);
            GetComponent<RectTransform>().anchorMax = new Vector2 (0.5f,0.5f);
            GetComponent<RectTransform>().anchorMin = new Vector2 (0.5f,0.5f);
            GetComponent<RectTransform>().pivot = new Vector2 (0.5f,0.5f);

            GetComponent<Button>().interactable = false;

        } else { 
            GetComponent<Button>().interactable = true;

            GetComponent<RectTransform>().localScale = new Vector3(0.5f,0.5f,0.5f);
            GetComponent<RectTransform>().anchoredPosition = new Vector2 (-2,-400);
            GetComponent<RectTransform>().anchorMax = new Vector2 (0,1);
            GetComponent<RectTransform>().anchorMin = new Vector2 (0,1);
            GetComponent<RectTransform>().pivot = new Vector2 (0,1);
        }
    }
}