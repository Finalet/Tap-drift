using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyBonus : MonoBehaviour
{
    public enum Type {
        crystal,
        car
    };
    [System.Serializable]
    public class Day {
        public Type type;
        public int amonut;
        public Sprite icon;
    }

    
    public Day[] Days;
    public GameObject DaysObjectsContainer;
    public float[] FillAmounts;

    public Sprite crystal;
    public Sprite jeep;

    public Image timeLine;
    public Image collectImage;

    public Button collectButton;

    public int dayNumber;
    int lastDay;

    void Awake() {
        if (GameManager.instance.jeepUn) {
            Days[5].type = Type.crystal;
            Days[5].amonut = 100;
            Days[5].icon = crystal;
        }

        Image[] daysObjects = DaysObjectsContainer.GetComponentsInChildren<Image>();
        for (int i = 0; i < 5; i++)
        {
            if (Days[i].type != Type.car) {
                daysObjects[i].sprite = Days[i].icon;
                daysObjects[i].rectTransform.sizeDelta = new Vector2 (70, 70);
                daysObjects[i].transform.GetChild(0).GetComponent<Text>().text = Days[i].amonut.ToString();
            } else {
                daysObjects[i].sprite = Days[i].icon;
                daysObjects[i].transform.GetChild(0).GetComponent<Text>().text = "Jeep";
            }
            
        }
    }
    void Start() {
        StartCoroutine(DrawTimeLine());        
    }
    void Update() {
        dayNumber = GameManager.instance.dayNumber;
    }

    public IEnumerator DrawTimeLine () {
        yield return new WaitForSeconds(1);
        while (transform.localScale.x <= 1) {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1.1f,1.1f,1.1f), 0.3f);
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(2);
        while (timeLine.fillAmount != FillAmounts[dayNumber-1]) {
            timeLine.fillAmount = Mathf.MoveTowards(timeLine.fillAmount, FillAmounts[dayNumber-1], 0.01f);
            yield return new WaitForSeconds(0.016f);
        }
        collectButton.interactable = true;
    }

    public void Collect () {
        GetComponent<Animation>().Play();
        GameManager.instance.Canvas.GetComponent<CanvasScript>().dailyBlockPanel.gameObject.SetActive(false);
        if (Days[dayNumber-1].type == Type.crystal) {
            collectImage.sprite = crystal;
            collectImage.transform.GetChild(0).GetComponent<Text>().text = Days[dayNumber-1].amonut.ToString();
        } else {
            collectImage.sprite = jeep;
            collectImage.transform.GetChild(0).GetComponent<Text>().text = "";
        }
    }

    IEnumerator DrawCollect () {
        yield return new WaitForSeconds(1);
        while (Vector2.Distance(collectImage.rectTransform.position, GameManager.instance.Canvas.GetComponent<CanvasScript>().crystalAmountText.rectTransform.position) >= 100) {
            collectImage.rectTransform.position = Vector2.Lerp(collectImage.rectTransform.position, GameManager.instance.Canvas.GetComponent<CanvasScript>().crystalAmountText.rectTransform.position, 0.1f);
            collectImage.rectTransform.localScale = Vector2.Lerp(collectImage.rectTransform.localScale, new Vector2(0.4f,0.4f), 0.1f);
            yield return new WaitForSeconds(0.01f);
        }
        collectImage.gameObject.SetActive(false);
        if (Days[dayNumber-1].type == Type.crystal) {
            GameManager.instance.GetComponent<Crystals>().AddCrystal(Days[dayNumber-1].amonut);
        }
    }

    public void ReceiveBonus() {
        StartCoroutine(DrawCollect());
    }
}