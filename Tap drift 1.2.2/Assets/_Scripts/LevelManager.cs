using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;

public class LevelManager : MonoBehaviour
{
    [SerializeField] List<float> Levels;

    public int currentLevel;
    [System.NonSerialized] public int nextLevel;
    public int rewardAmount;


    [System.NonSerialized] public bool dontChangeRewardText;

    public float XP;
    public float currentLevelXp;

    CanvasScript canvas;

    public bool maxLevelReached;
    void Awake() {
        if (ES3.KeyExists("level")) currentLevel = ES3.Load<int>("level");
        else currentLevel = 1;

        GenerateLevels();

        canvas = GameManager.instance.Canvas.GetComponent<CanvasScript>();
    }

    void Update() {
        nextLevel = currentLevel + 1;

        if (currentLevel < Levels.Count) {
            currentLevelXp = Levels[currentLevel-1];  
            maxLevelReached = false;            
        } else if (currentLevel >= Levels.Count) {
            maxLevelReached = true;
        }
        if (GameManager.instance.Player.GetComponent<SplineFollower>().enabled && !maxLevelReached)
            XP += (GameManager.instance.Player.GetComponent<Player>().currentSpeed * Time.deltaTime * GetComponent<UpgradesContainer>().upgradeMultiplier * GameManager.instance.scoreMultBonus)/1000;

        if (XP >= currentLevelXp && !maxLevelReached) {
            LevelUp();
        }

        CalcReward();

    }
    void CalcReward () {
        if (nextLevel < 10)
            rewardAmount = 5;
        else if (nextLevel >= 10 && nextLevel < 20)
            rewardAmount = 10;
        else if (nextLevel >= 20 && nextLevel < 30)
            rewardAmount = 20;
        else if (nextLevel >= 30 && nextLevel < 40)
            rewardAmount = 30;
        else if (nextLevel >= 40 && nextLevel < 50)
            rewardAmount = 40;
        else if (nextLevel >= 50 && nextLevel < 60)
            rewardAmount = 50;
        else if (nextLevel >= 60 && nextLevel < 70)
            rewardAmount = 60;
        else if (nextLevel >= 60 && nextLevel < 80)
            rewardAmount = 70;
        else if (nextLevel >= 60 && nextLevel < 90)
            rewardAmount = 80;
        else if (nextLevel >= 60 && nextLevel < 100)
            rewardAmount = 90;
        else if (nextLevel >= 100)
            rewardAmount = 100;
    }

    public void Lost () {
        XP = 0;
    }

    void LevelUp () {
        StartCoroutine(LevelUpAnimation(rewardAmount));
        currentLevel++;
        XP = 0;
    }
    IEnumerator LevelUpAnimation(int reward) {
        dontChangeRewardText = true;
        Vector2 prevPos = canvas.rewardText.rectTransform.localPosition;

        while (Vector2.Distance(canvas.rewardText.rectTransform.localPosition, new Vector2(0, -800)) >= 0.2f) {
            canvas.rewardText.rectTransform.localPosition = Vector2.Lerp(canvas.rewardText.rectTransform.localPosition, new Vector2(0, -800), 0.2f);
            canvas.rewardText.rectTransform.localScale = Vector2.Lerp(canvas.rewardText.rectTransform.localScale, new Vector2(2.5f, 2.5f), 0.2f);
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(0.5f);
        while (Vector2.Distance(canvas.rewardText.rectTransform.localPosition, new Vector2(-450, 150)) >= 20) {
            canvas.rewardText.rectTransform.localPosition = Vector2.Lerp(canvas.rewardText.rectTransform.localPosition, new Vector2(-450, 150), 0.2f);
            canvas.rewardText.rectTransform.localScale = Vector2.Lerp(canvas.rewardText.rectTransform.localScale, new Vector2(1, 1), 0.2f);
            yield return new WaitForSeconds(0.01f);
        }
        GetComponent<Crystals>().AddCrystal(reward);
        canvas.rewardText.rectTransform.localPosition = prevPos;
        dontChangeRewardText = false;
    }

    public void SaveLevel () {
        ES3.Save<int>("level", currentLevel);
    }

    void GenerateLevels() {
        float delta = 0.3f;
        Levels.Add(delta);
        for (int i = 1; i < 100; i++)
        {
            delta *= 1.2f;
            Levels.Add(delta);
        }
    }

}
