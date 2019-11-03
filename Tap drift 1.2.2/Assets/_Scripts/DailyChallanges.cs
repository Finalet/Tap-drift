using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyChallanges : MonoBehaviour
{
    public static DailyChallanges instance;

    [System.Serializable] public struct Challage {
        public string description;
        public int reward;
        public float progress;
    }
    
    public List<Challage> AllChallanges;

    public GameObject Challage1;
    public float challange1Progress; 
    public GameObject Challage2;
    public float challange2Progress; 

    Challage[] Array;
    int numberOfChallanges = 2;

    int lastDay;

    void Awake() {
        instance = this;
    }

    void Update() {
        lastDay = GameManager.instance.lastDay;

        if (Array == null) 
            return;

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
    }
    
}
