using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasScript : MonoBehaviour
{
    public TextMeshProUGUI scoreText, driftScoreText, tapToPlay, topScore, countDown, carName, tipsText, crystalAmountText, bulldozerAmountText, currentLevelText, nextLevelText, rewardText;
    public GameObject menuUI, fullFuelBar, play, lostScreen, driftTip, Tutorial, upgradesPanel, dailyBonusPanel;
    public Image fualBar, screenFiller, discPanel, progressBar, lostBar, sound, cursor, multBar, levelBar, dailyBlockPanel;
    public Text discText, progressText, upgradeMultiplierText, bulldozerSmashTimesIconText;

    public Sprite locked, pick, soundOn, soundOff;

    public Transform eleganrCarCamera, mustangCamera, sportsCamera,  hotrodCamera, SUVcamera, jeepCamera, mainCameraPos;
    public Button garageButton, leftButton, rightButton, exitButton, pickButton, policyButton, pauseButton, exitUpgradesButton;

    [Header("Unlock Rules")]
    string mustangRule;
    string sportsRule;
    string hotrodRule;
    string suvRule;
    string jeepRule;

    public bool showDriftTip;

    public GameObject continueButton;

    [Header("Upgrades")]
    public TextMeshProUGUI crystalAmountTextInShop;
    public TextMeshProUGUI bulldozerAmountTextInShop;
    public TextMeshProUGUI multUpgradePriceText;
    public TextMeshProUGUI bulldozerPriceText;
    public TextMeshProUGUI bulldozerSmashTimesPriceText; 
    public Button upgradeMultiplierButton;
    public Button bulldozerButton;
    public Button bulldozerSmashTimesButton;

    [Header("Tips")]
    public string[] TipsTexts;
    
    NumberFormatInfo nfi;

    float size = 0;

    void Awake()
    {
        continueButton = lostScreen.transform.GetChild(0).gameObject;

        driftScoreText.text = null;

        screenFiller.rectTransform.sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y / 2.4f);

        scoreText.gameObject.SetActive(false);
        fullFuelBar.SetActive(false);
        levelBar.gameObject.SetActive(false);

        mustangRule = "Drive for 20 000 points";
        sportsRule = "Earn 5 000 points non-stop while drifting";
        hotrodRule = "Recieve overall 50 000 drift points";
        suvRule = "Crash 30 times";
        jeepRule = "Play 5 days in a row";

        ShowRandomTip();

        if (Application.platform == RuntimePlatform.Android)
        {
            policyButton.gameObject.SetActive(true);
        }
        else
        {
            policyButton.gameObject.SetActive(false);
        }

        nfi = new CultureInfo("ru-RU", false).NumberFormat;
        nfi.NumberDecimalDigits = 0;

        baseColor = levelBar.color;
        baseColor2 = levelBar.transform.GetChild(1).GetComponent<Image>().color;
    }

    void DrawCursor ()
    {
        Cursor.visible = false;
        cursor.rectTransform.position = new Vector2 (Camera.main.WorldToScreenPoint(GameManager.instance.Player.GetComponent<Player>().carModel.transform.position).x + 25, 150);
    }
    void FixedUpdate()
    {
        float sinSize;
        size += 0.1f;
        sinSize = 1.05f + Mathf.Sin(size) * 0.05f; 
        continueButton.GetComponent<RectTransform>().localScale = new Vector3(sinSize, sinSize, 1);
    }

    void Update()
    {
        //For Ads
        //DrawCursor();

        topScore.text = "TOP SCORE:\n" + Mathf.Round(GameManager.instance.maxScore).ToString("N", nfi);

        //Crystals
        crystalAmountText.text = GameManager.instance.GetComponent<Crystals>().crystalAmount.ToString();
        if (!GameManager.instance.GetComponent<LevelManager>().dontChangeRewardText)
            rewardText.text = GameManager.instance.GetComponent<LevelManager>().rewardAmount.ToString();

        //Bulldozer
        bulldozerAmountText.text = GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount.ToString();


        DrawUpgradesUI();

        ScoreText();
        FuelBar();
        LevelBar();
        TapToPlay();
        UpdateCarInfo();
        DiscPanelInfo();
        ChangeColorOfLVLui();

        if (showDriftTip)
            driftTip.SetActive(true);
        else
            driftTip.SetActive(false);

        if (GameManager.instance.soundOn)
            sound.sprite = soundOn;
        else
            sound.sprite = soundOff;

        DrawAmounts();


        //Swipe Left in garag
        if (Input.touchCount > 0 && Input.GetTouch(0).deltaPosition.x <= -Screen.width / 8 && inGarage)
        {
            ScrollRight();
        } 
        //Swipe Right in garage
        if (Input.touchCount > 0 && Input.GetTouch(0).deltaPosition.x >= Screen.width / 8 && inGarage)
        {
            ScrollLeft();
        }
    }

    void ScoreText ()
    {
        scoreText.text = Mathf.Round(GameManager.instance.score).ToString("N", nfi);
        if (GameManager.instance.keepDriftScore != 0 && !GameManager.instance.lost)
        {
            driftScoreText.text = Mathf.Round(GameManager.instance.keepDriftScore).ToString("N", nfi);

            driftScoreText.GetComponent<RectTransform>().sizeDelta = Vector2.Lerp(driftScoreText.GetComponent<RectTransform>().sizeDelta, new Vector2(500, 60), 0.3f);
            driftScoreText.fontSize = Mathf.Lerp(driftScoreText.fontSize, 95, 0.3f);

        }

        if (moveText==true)
        {
            driftScoreText.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp(driftScoreText.GetComponent<RectTransform>().anchoredPosition, scoreText.GetComponent<RectTransform>().anchoredPosition, 0.07f);
        }
        if(increase)
            driftScoreText.fontSize = Mathf.Lerp(driftScoreText.fontSize, 150, 0.2f);
        else if (decrease)
            driftScoreText.fontSize = Mathf.Lerp(driftScoreText.fontSize, 0, 0.2f);


    }

    bool moveText;
    bool increase;
    bool decrease;
    public IEnumerator AddDriftScore ()
    {
        if (GameManager.instance.lost)
        {
            driftScoreText.text = "0";
            moveText = true;
            increase = true;
            yield return new WaitForSeconds(0.125f);
            increase = false;
            decrease = true;
            yield return new WaitForSeconds(0.2f);
            moveText = false;
            decrease = false;
            driftScoreText.text = null;
            driftScoreText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -615);
        }
        else
        {
            moveText = true;
            increase = true;
            yield return new WaitForSeconds(0.125f);
            increase = false;
            decrease = true;
            yield return new WaitForSeconds(0.2f);
            moveText = false;
            decrease = false;
            driftScoreText.text = null;
            GameManager.instance.AddDriftScore();
            driftScoreText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -615);
        }
        
    }

    void FuelBar()
    {
        float fuel = GameManager.instance.Player.GetComponent<Player>().driftFuel;
        float maxFuel = GameManager.instance.Player.GetComponent<Player>().maxDriftFuel;
        fualBar.GetComponent<RectTransform>().sizeDelta = new Vector2( 600 * fuel/maxFuel, 20);
    }

    void LevelBar()
    {
        float XP = GameManager.instance.GetComponent<LevelManager>().XP;
        float currentLevelXp = GameManager.instance.GetComponent<LevelManager>().currentLevelXp;
        levelBar.transform.GetChild(1).transform.GetComponent<Image>().fillAmount = XP / currentLevelXp;
        levelBar.transform.GetChild(2).transform.GetComponent<RectTransform>().anchoredPosition = new Vector2 (Mathf.Lerp(-360f, 360f, levelBar.transform.GetChild(1).transform.GetComponent<Image>().fillAmount), 0); //You are a fucking genious

        if (!GameManager.instance.GetComponent<LevelManager>().maxLevelReached) {
            currentLevelText.text = GameManager.instance.GetComponent<LevelManager>().currentLevel.ToString();
            nextLevelText.text = GameManager.instance.GetComponent<LevelManager>().nextLevel.ToString();
        } else {
            currentLevelText.text = "MAX";
            nextLevelText.text = "MAX";
            levelBar.transform.GetChild(2).gameObject.SetActive(false);
        }
        
    }

    float y = 0;
    void TapToPlay ()
    {
        float x = Mathf.Sin(y) * 10;
        y += 5 * Time.deltaTime;
        tapToPlay.fontSize = 130 + x;
    }


    public void StartGame ()
    {
        menuUI.SetActive(false);
        fullFuelBar.SetActive(true);
        fualBar.gameObject.SetActive(true);
        levelBar.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        tipsText.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        menuUI.SetActive(true);
        tipsText.gameObject.SetActive(true);
        ShowRandomTip();
        fullFuelBar.SetActive(false);
        fualBar.gameObject.SetActive(false);
        levelBar.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(false);
    }

    public IEnumerator CountDown()
    {
        yield return new WaitForSeconds(0.5f);
        countDown.text = "3";
        Taptic.Success();
        yield return new WaitForSeconds(0.5f);
        countDown.text = "2";
        Taptic.Success();
        yield return new WaitForSeconds(0.5f);
        countDown.text = "1";
        Taptic.Success();
        yield return new WaitForSeconds(0.5f);
        countDown.text = "GO!";
        Taptic.Failure();
        yield return new WaitForSeconds(0.7f);
        countDown.text = "";
        pauseButton.gameObject.SetActive(true);
    }

    void UpdateCarInfo()
    {
        if (cameraPos == 0)
            carName.text = "Elegant Car";
        else if (cameraPos == 1)
            carName.text = "Mustang";
        else if (cameraPos == 2)
            carName.text = "Hypercar";
        else if (cameraPos == 3)
            carName.text = "Hot Rod";
        else if (cameraPos == 4)
            carName.text = "Huge SUV";
        else if (cameraPos == 5)
            carName.text = "Big Jeep";
    }

    void DiscPanelInfo()
    {
        if (cameraPos == 0)
        {
            discPanel.enabled = false;
            discText.text = "";
            pickButton.GetComponent<Image>().sprite = pick;
            progressBar.transform.parent.gameObject.SetActive(false);
            progressText.transform.parent.gameObject.SetActive(false);
        }
        else if (cameraPos == 1)
        {
            if (!GameManager.instance.mustangUn)
            {
                discPanel.enabled = true;
                discText.text = mustangRule;
                pickButton.GetComponent<Image>().sprite = locked;
                progressBar.transform.parent.gameObject.SetActive(true);
                progressText.transform.parent.gameObject.SetActive(true);
                progressBar.fillAmount = GameManager.instance.maxScore / 20000;
                progressText.text = Mathf.Round(GameManager.instance.maxScore).ToString("N", nfi);
            }
            else
            {
                discPanel.enabled = false;
                discText.text = "";
                pickButton.GetComponent<Image>().sprite = pick;
                progressBar.transform.parent.gameObject.SetActive(false);
                progressText.transform.parent.gameObject.SetActive(false);
            }
        }
        else if (cameraPos == 2)
        {
            if (!GameManager.instance.sportsUn)
            {
                discPanel.enabled = true;
                discText.text = sportsRule;
                pickButton.GetComponent<Image>().sprite = locked;
                progressBar.transform.parent.gameObject.SetActive(true);
                progressText.transform.parent.gameObject.SetActive(true);
                progressBar.fillAmount = GameManager.instance.maxDrift / 5000;
                progressText.text = Mathf.Round(GameManager.instance.maxDrift).ToString("N", nfi);
            }
            else
            {
                discPanel.enabled = false;
                discText.text = "";
                pickButton.GetComponent<Image>().sprite = pick;
                progressBar.transform.parent.gameObject.SetActive(false);
                progressText.transform.parent.gameObject.SetActive(false);
            }
        }
        else if (cameraPos == 3)
        {
            if (!GameManager.instance.hotrodUn)
            {
                discPanel.enabled = true;
                discText.text = hotrodRule;
                pickButton.GetComponent<Image>().sprite = locked;
                progressBar.transform.parent.gameObject.SetActive(true);
                progressText.transform.parent.gameObject.SetActive(true);
                progressBar.fillAmount = GameManager.instance.overallDriftScore / 50000;
                progressText.text = Mathf.Round(GameManager.instance.overallDriftScore).ToString("N", nfi);
            }
            else
            {
                discPanel.enabled = false;
                discText.text = "";
                pickButton.GetComponent<Image>().sprite = pick;
                progressBar.transform.parent.gameObject.SetActive(false);
                progressText.transform.parent.gameObject.SetActive(false);
            }
        }
        else if (cameraPos == 4)
        {
            if (!GameManager.instance.suvUn)
            {
                discPanel.enabled = true;
                discText.text = suvRule;
                pickButton.GetComponent<Image>().sprite = locked;
                progressBar.transform.parent.gameObject.SetActive(true);
                progressText.transform.parent.gameObject.SetActive(true);
                progressBar.fillAmount = (float)GameManager.instance.crashTimes / 30;
                progressText.text = Mathf.Round(GameManager.instance.crashTimes).ToString("N", nfi);
            }
            else
            {
                discPanel.enabled = false;
                discText.text = "";
                pickButton.GetComponent<Image>().sprite = pick;
                progressBar.transform.parent.gameObject.SetActive(false);
                progressText.transform.parent.gameObject.SetActive(false);
            }
        }
        else if (cameraPos == 5)
        {
            if (!GameManager.instance.jeepUn)
            {
                discPanel.enabled = true;
                discText.text = jeepRule;
                pickButton.GetComponent<Image>().sprite = locked;
                progressBar.transform.parent.gameObject.SetActive(true);
                progressText.transform.parent.gameObject.SetActive(true);
                progressBar.fillAmount = (float)GameManager.instance.dayNumber / 5;
                progressText.text = Mathf.Round(GameManager.instance.dayNumber).ToString("N", nfi);
            }
            else
            {
                discPanel.enabled = false;
                discText.text = "";
                pickButton.GetComponent<Image>().sprite = pick;
                progressBar.transform.parent.gameObject.SetActive(false);
                progressText.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    [Space]
    public bool inGarage;
    public void EnterGarage ()
    {
        inGarage = true;

        if (disableButton)
            return;

        Taptic.Selection();

        Player.Model currentModel = GameManager.instance.Player.GetComponent<Player>().currentModel;


        if (currentModel == Player.Model.Elegant)
        {
            cameraPos = 0;
            StartCoroutine(MoveCamera(eleganrCarCamera, 30));
        }
        else if (currentModel == Player.Model.Jeep)
        {
            cameraPos = 5;
            StartCoroutine(MoveCamera(jeepCamera, 30));
        }
        else if (currentModel == Player.Model.HotRod)
        {
            cameraPos = 3;
            StartCoroutine(MoveCamera(hotrodCamera, 30));
        }
        else if (currentModel == Player.Model.Mustang)
        {
            cameraPos = 1;
            StartCoroutine(MoveCamera(mustangCamera, 30));
        }
        else if (currentModel == Player.Model.Sports)
        {
            cameraPos = 2;
            StartCoroutine(MoveCamera(sportsCamera, 30));
        }
        else if (currentModel == Player.Model.SUV)
        {
            cameraPos = 4;
            StartCoroutine(MoveCamera(SUVcamera, 30));
        }

        topScore.gameObject.SetActive(false);
        tapToPlay.gameObject.SetActive(false);
        play.gameObject.SetActive(false);
        garageButton.gameObject.SetActive(false);
        tipsText.gameObject.SetActive(false);
        crystalAmountText.gameObject.SetActive(false);
        bulldozerAmountText.gameObject.SetActive(false);

        leftButton.gameObject.SetActive(true);
        rightButton.gameObject.SetActive(true);
        exitButton.gameObject.SetActive(true);
        pickButton.gameObject.SetActive(true);
        carName.gameObject.SetActive(true);
        discPanel.gameObject.SetActive(true);
    }
    public void ExitToMenu ()
    {
        if (inGarage) {
            inGarage = false;

            if (disableButton)
                return;

            Taptic.Selection();

            StartCoroutine(MoveCamera(mainCameraPos, 30));
            topScore.gameObject.SetActive(true);
            garageButton.gameObject.SetActive(true);
            tipsText.gameObject.SetActive(true);
            crystalAmountText.gameObject.SetActive(true);
            bulldozerAmountText.gameObject.SetActive(true);

            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(false);
            pickButton.gameObject.SetActive(false);
            carName.gameObject.SetActive(false);
            discPanel.gameObject.SetActive(false);
        } else if (inUpgrades) {
            inUpgrades = false;

            if (disableButton)
                return;

            Taptic.Selection();
            upgradesPanel.SetActive(false);
            exitButton.gameObject.SetActive(false);
            exitUpgradesButton.gameObject.SetActive(false);

            crystalAmountText.gameObject.SetActive(true);
            bulldozerAmountText.gameObject.SetActive(true);
            topScore.gameObject.SetActive(true);
            tapToPlay.gameObject.SetActive(true);
            play.gameObject.SetActive(true);
            garageButton.gameObject.SetActive(true);
            tipsText.gameObject.SetActive(true);
        }
    }

    int cameraPos = 0;
    bool disableButton;
    public void ScrollRight ()
    {
        if (disableButton)
            return;

        Taptic.Selection();

        if (cameraPos == 0)
            StartCoroutine(MoveCamera(mustangCamera, 15));
        else if (cameraPos == 1)
            StartCoroutine(MoveCamera(sportsCamera, 15));
        else if (cameraPos == 2)
            StartCoroutine(MoveCamera(hotrodCamera, 15));
        else if (cameraPos == 3)
            StartCoroutine(MoveCamera(SUVcamera, 15));
        else if (cameraPos == 4)
            StartCoroutine(MoveCamera(jeepCamera, 15));

        if (cameraPos == 5)
        {
            cameraPos = 0;
            StartCoroutine(MoveCamera(eleganrCarCamera, 45));
        }
        else
        {
            cameraPos++;
        }
    }
    public void ScrollLeft()
    {
        if (disableButton)
            return;

        Taptic.Selection();

        if (cameraPos == 5)
            StartCoroutine(MoveCamera(SUVcamera, 15));
        else if (cameraPos == 4)
            StartCoroutine(MoveCamera(hotrodCamera, 15));
        else if (cameraPos == 3)
            StartCoroutine(MoveCamera(sportsCamera, 15));
        else if (cameraPos == 2)
            StartCoroutine(MoveCamera(mustangCamera, 15));
        else if (cameraPos == 1)
            StartCoroutine(MoveCamera(eleganrCarCamera, 15));

        if (cameraPos == 0)
        {
            cameraPos = 5;
            StartCoroutine(MoveCamera(jeepCamera, 45));
        } else
        {
            cameraPos--;
        }
    }


    IEnumerator MoveCamera (Transform pos, int speed)
    {

        while (Vector3.Distance(Camera.main.transform.position, pos.position) != 0)
        {
            disableButton = true;

            Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position, pos.position, speed * Time.deltaTime);
            Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, pos.rotation, 10f * Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
        disableButton = false;

        if (pos == mainCameraPos)
        {
            tapToPlay.gameObject.SetActive(true);
            play.gameObject.SetActive(true);
        }
    }

    public void PickCar ()
    {
        bool picked = false;
        if (cameraPos == 0)
        {
            //GameManager.instance.Player.GetComponent<Player>().modelIndex = 0;
            GameManager.instance.Player.GetComponent<Player>().currentModel = Player.Model.Elegant;
            picked = true;
        }
        else if (cameraPos == 1 && GameManager.instance.mustangUn)
        {
            //GameManager.instance.Player.GetComponent<Player>().modelIndex = 3;
            GameManager.instance.Player.GetComponent<Player>().currentModel = Player.Model.Mustang;
            picked = true;
        }
        else if (cameraPos == 2 && GameManager.instance.sportsUn)
        {
            //GameManager.instance.Player.GetComponent<Player>().modelIndex = 4;
            GameManager.instance.Player.GetComponent<Player>().currentModel = Player.Model.Sports;
            picked = true;
        }
        else if (cameraPos == 3 && GameManager.instance.hotrodUn)
        {
            //GameManager.instance.Player.GetComponent<Player>().modelIndex = 2;
            GameManager.instance.Player.GetComponent<Player>().currentModel = Player.Model.HotRod;
            picked = true;
        }
        else if (cameraPos == 4 && GameManager.instance.suvUn)
        {
            //GameManager.instance.Player.GetComponent<Player>().modelIndex = 5;
            GameManager.instance.Player.GetComponent<Player>().currentModel = Player.Model.SUV;
            picked = true;
        }
        else if (cameraPos == 5 && GameManager.instance.jeepUn)
        {
            //GameManager.instance.Player.GetComponent<Player>().modelIndex = 1;
            GameManager.instance.Player.GetComponent<Player>().currentModel = Player.Model.Jeep;
            picked = true;
        }

        if (picked)
        {
            Taptic.Selection();

            ExitToMenu();
            GameManager.instance.Player.GetComponent<Player>().SaveModelIndex();
            GameManager.instance.Player.GetComponent<Player>().SpawnNewModel();
        } else
        {
            Taptic.Warning();

            discText.GetComponent<Animation>().Play();
        }
    }

    public IEnumerator ShowLostScreen()
    {
        lostScreen.SetActive(true);
        pauseButton.gameObject.SetActive(false);
        multBar.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.2f);

        float t = 3;
        while (t > 0)
        {
            t -= Time.deltaTime;
            lostBar.fillAmount = t/3;
            yield return new WaitForSeconds(Time.deltaTime);
            if (!GameManager.instance.Player.GetComponent<Player>().gameStarted)
                t = 0;
        } 
        if (GameManager.instance.lost)
            GameManager.instance.Restart();
    }
    public void HideLostScreen ()
    {
        lostScreen.SetActive(false);
    }

    public void SoundSwitch()
    {
        Taptic.Selection();

        if (GameManager.instance.soundOn)
        {
            GameManager.instance.soundOn = false;
            GameManager.instance.GetComponent<AudioSource>().Stop();
            ES3.Save<bool>("sound", false);
        }
        else
        {
            GameManager.instance.soundOn = true;
            GameManager.instance.GetComponent<AudioSource>().Play();
            ES3.Save<bool>("sound", true);
        }
    }
    public void PurchaseNoAds()
    {
        ES3.Save<bool>("noAds", true);
        GameManager.instance.noAds = true;
        GameManager.instance.StopShowingBanner();
    }

    public void ShowRandomTip ()
    {
        int x = Random.Range(0, TipsTexts.Length);
        tipsText.text = TipsTexts[x];
    }

    public void PrivacyPolicyButton ()
    {
        Application.OpenURL("https://app.termly.io/document/privacy-policy/3a2093eb-51c1-49fc-a1ba-cffb473ba845");
    }

    bool resumeDriftSound;
    public void PauseSwitch ()
    {
        if (!GameManager.instance.isPaused)
        {
            Time.timeScale = 0;
            if (Camera.main.GetComponent<AudioSource>().isPlaying)
            {
                Camera.main.GetComponent<AudioSource>().Pause();
                resumeDriftSound = true;
            }
            GameManager.instance.isPaused = true;
        }
        else
        {
            Time.timeScale = 1;
            if (resumeDriftSound)
            {
                Camera.main.GetComponent<AudioSource>().Play();
                resumeDriftSound = false;
            }
            GameManager.instance.isPaused = false;
        }
    }



 #region Upgrades

    public bool inUpgrades;
    public void OpenUpgradesPanel () {
        inUpgrades = true;

        Taptic.Selection();
        upgradesPanel.SetActive(true);
        exitButton.gameObject.SetActive(true);
        exitUpgradesButton.gameObject.SetActive(true);

        crystalAmountText.gameObject.SetActive(false);
        bulldozerAmountText.gameObject.SetActive(false);
        topScore.gameObject.SetActive(false);
        tapToPlay.gameObject.SetActive(false);
        play.gameObject.SetActive(false);
        garageButton.gameObject.SetActive(false);
        tipsText.gameObject.SetActive(false);
    }
    void DrawUpgradesUI () {
        crystalAmountTextInShop.text = GameManager.instance.GetComponent<Crystals>().crystalAmount.ToString();
        bulldozerAmountTextInShop.text = GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount.ToString();
        //Mulpitplier
        upgradeMultiplierText.text = "x" + GameManager.instance.GetComponent<UpgradesContainer>().upgradeMultiplier.ToString();
        multUpgradePriceText.text = GameManager.instance.GetComponent<UpgradesContainer>().upgradeMultiplierPrice.ToString();

        if (GameManager.instance.GetComponent<Crystals>().crystalAmount >= GameManager.instance.GetComponent<UpgradesContainer>().upgradeMultiplierPrice)
            upgradeMultiplierButton.interactable = true;
        else 
            upgradeMultiplierButton.interactable = false;

        //Bulldozer
        bulldozerPriceText.text = GameManager.instance.GetComponent<UpgradesContainer>().bulldozerPrice.ToString();
        bulldozerSmashTimesIconText.text = GameManager.instance.GetComponent<UpgradesContainer>().bulldozerSmashTimes.ToString();

        if (GameManager.instance.GetComponent<Crystals>().crystalAmount >= GameManager.instance.GetComponent<UpgradesContainer>().bulldozerPrice)
            bulldozerButton.interactable = true;
        else 
            bulldozerButton.interactable = false;

        //Bulldozer smashed times
        bulldozerSmashTimesPriceText.text = GameManager.instance.GetComponent<UpgradesContainer>().bulldozerSmashTimesPrice.ToString();

        if (GameManager.instance.GetComponent<Crystals>().crystalAmount >= GameManager.instance.GetComponent<UpgradesContainer>().bulldozerSmashTimesPrice)
            bulldozerSmashTimesButton.interactable = true;
        else 
            bulldozerSmashTimesButton.interactable = false;
    }

    public Color bulldozerDeployed, bulldozerDeployed2;
    Color baseColor, baseColor2;
    void ChangeColorOfLVLui () {
        if (GameManager.instance.Player.GetComponent<Player>().bulldozerDeployed) {
            levelBar.color = bulldozerDeployed; levelBar.transform.GetChild(1).GetComponent<Image>().color = bulldozerDeployed2;
        } else {
            levelBar.color = baseColor; levelBar.transform.GetChild(1).GetComponent<Image>().color = baseColor2;
        } 
    }
 
    void DrawAmounts() {
        if (GameManager.instance.GetComponent<Crystals>().crystalAmount >= 0 && GameManager.instance.GetComponent<Crystals>().crystalAmount <= 9)
        {
            crystalAmountText.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 60);
        }
        else if(GameManager.instance.GetComponent<Crystals>().crystalAmount >= 10 && GameManager.instance.GetComponent<Crystals>().crystalAmount <= 99)
        {
            crystalAmountText.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 60);
        } else if (GameManager.instance.GetComponent<Crystals>().crystalAmount >= 100 && GameManager.instance.GetComponent<Crystals>().crystalAmount <= 999)
        {
            crystalAmountText.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 60);
        }
        else if (GameManager.instance.GetComponent<Crystals>().crystalAmount >= 1000 && GameManager.instance.GetComponent<Crystals>().crystalAmount <= 9999)
        {
            crystalAmountText.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 60);
        }
        
        if (GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount >= 0 && GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount <= 9)
        {
            bulldozerAmountText.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 60);
        }
        else if(GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount >= 10 && GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount <= 99)
        {
            bulldozerAmountText.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 60);
        } else if (GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount >= 100 && GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount <= 999)
        {
            bulldozerAmountText.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 60);
        }
        else if (GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount >= 1000 && GameManager.instance.GetComponent<UpgradesContainer>().bulldozerAmount <= 9999)
        {
            bulldozerAmountText.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 60);
        }
    }
 #endregion
}
