﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dreamteck.Splines;
using UnityEngine.Advertisements;
using UnityEngine.Analytics;

public class GameManager : MonoBehaviour, IUnityAdsListener
{
    public static GameManager instance = null;

    public GameObject Player;
    public GameObject CitizenCarPool;
    public GameObject Canvas;

    public int numberOfPointsInRoadSegment; //Number of points in one road segment

    [Space]
    public float score;
    public float maxScore;
    public int scoreMultBonus;

    [Header("Road Generator Parametrs")]
    public int distanceMultiplier;
    public int sidesMultiplier;
    public int heightMultiplier;

    [Header("Citizen Cars")]
    [Tooltip("Take into account that some cars will be destroyed by collision checkers")] public int numberOfCitizenCars;
    public int speedOfCitizenCars;

    [Header("Drift Fuel")]
    public int driftFuelAddition;
    public int numberOfDriftFuels;

    [Header("Boosters")]
    public int numberOfBoosters;

    [Header("Bonuses")]
    public int numberOfScoreMultipliers;
    public int numberOfCrystals;

    [Space]
    public SplinePoint lastPoint;
    [Space]
    public bool lost;

    public GameObject roadGenerator;

    [Header("Unlocked Cars")]
    public bool mustangUn;
    public bool sportsUn;
    public bool hotrodUn;
    public bool suvUn;
    public bool jeepUn;
    public GameObject unlockedScreen;


    public float maxDrift = 0;
    public float overallDriftScore = 0;
    public int crashTimes = 0;
    public int dayNumber = 0;
    public float OverallAppRuntimeInSeconds;

    [Space]
    public bool isPaused;
    public bool soundOn;
    public bool noAds;

#if UNITY_IOS
    private string gameId = "3266964";
#elif UNITY_ANDROID
    private string gameId = "3266965";
#endif
    public bool testMode;
    public float timerForAds;
    public float adFrequencyInSeconds;
    bool regualAdReady;

    public int lastDay;
    [System.NonSerialized] public bool iPad; 

    void Awake()
    {
        if (ES3.KeyExists("OverallAppRuntimeInSeconds"))
        {
            OverallAppRuntimeInSeconds = (float)ES3.Load<int>("OverallAppRuntimeInSeconds");
        }

        if (ES3.KeyExists("sound"))
        {
            soundOn = ES3.Load<bool>("sound");
        } else
        {
            soundOn = true;
        }

        if (ES3.KeyExists("noAds"))
        {
            noAds = ES3.Load<bool>("noAds");
        }
        else
        {
            noAds = false;
        }

        stopScore = 0;
        if (ES3.KeyExists("maxScore")) //Load max score
            maxScore = ES3.Load<float>("maxScore");


        if (ES3.KeyExists("maxDrift")) // Load max drift
            maxDrift = ES3.Load<float>("maxDrift");

        if (ES3.KeyExists("overallDriftScore")) // Load overall dritft score
            overallDriftScore = ES3.Load<float>("overallDriftScore");

        if (ES3.KeyExists("crashTimes")) // Load times crashed
            crashTimes = ES3.Load<int>("crashTimes");

        if (ES3.KeyExists("dayNumber")) //Load number of days login in a row
            dayNumber = ES3.Load<int>("dayNumber");

        if (ES3.KeyExists("lastDay")) //Load last day played
            lastDay = ES3.Load<int>("lastDay");

        if (ES3.KeyExists("mustang"))
            mustangUn = true;
        if (ES3.KeyExists("sports"))
            sportsUn = true;
        if (ES3.KeyExists("hotrod"))
            hotrodUn = true;
        if (ES3.KeyExists("suv"))
            suvUn = true;
        if (ES3.KeyExists("jeep"))
            jeepUn = true;


        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        lastPoint.SetPosition(Vector3.zero);
    }
    void Start()
    {
        AppsFlyer.setAppsFlyerKey ("zcKrZYJWnrWWctCxcLNnyT");


        Advertisement.AddListener(this);
        Advertisement.Initialize(gameId, testMode);
        Canvas.GetComponent<CanvasScript>().continueButton.GetComponent<Button>().interactable = Advertisement.IsReady("rewardedVideo");
        Canvas.GetComponent<CanvasScript>().videoForCrystalsButton.GetComponent<Button>().interactable = Advertisement.IsReady("rewardedVideoCrystals");

        if (soundOn)
            GetComponent<AudioSource>().Play();
        else
            GetComponent<AudioSource>().Stop();

#if UNITY_IOS
        if (UnityEngine.iOS.Device.generation.ToString().Contains("iPad"))
        {
            Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
            iPad = true;
        }
        else
        {
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        }

        AppsFlyer.setAppID ("1479272430");
        AppsFlyer.trackAppLaunch ();

#else
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);

        //AppsFlyer.setAppID ("YOUR_ANDROID_PACKAGE_NAME_HERE");
        //AppsFlyer.init ("YOUR_APPSFLYER_DEV_KEY","AppsFlyerTrackerCallbacks");
#endif


        StartCoroutine(ShowBannerWhenReady());
        RecordDay();
    }

    void Update()
    {
        ScoreMultiplier();

        Canvas.GetComponent<CanvasScript>().continueButton.GetComponent<Button>().interactable = Advertisement.IsReady("rewardedVideo");

        if (!watched)
            Canvas.GetComponent<CanvasScript>().videoForCrystalsButton.GetComponent<Button>().interactable = Advertisement.IsReady("rewardedVideoCrystals");
        else 
            Canvas.GetComponent<CanvasScript>().videoForCrystalsButton.GetComponent<Button>().interactable = false;

        CalculateScoreSeperate();
        if (land)
            LandCar();

        OverallAppRuntimeInSeconds += Time.deltaTime;

        timerForAds += Time.deltaTime;
        if (timerForAds >= adFrequencyInSeconds && !noAds && !Player.GetComponent<Player>().gameStarted && !Canvas.GetComponent<CanvasScript>().inGarage && !Canvas.GetComponent<CanvasScript>().inUpgrades)
        {
            if (regualAdReady)
            {
                timerForAds = 0;
                ShowRegularAd();
            }
        }
    }

    void RecordDay () {
        if (lastDay != System.DateTime.Now.DayOfYear)
        {
            if (System.DateTime.Now.DayOfYear - lastDay == 1)
            {
                dayNumber++;
                ES3.Save<int>("dayNumber", dayNumber);
            }
            else
            {
                dayNumber = 1;
                ES3.Save<int>("dayNumber", dayNumber);
            }
            DailyChallanges.instance.GenerateArray();

            lastDay = System.DateTime.Now.DayOfYear;
            ES3.Save<int>("lastDay", lastDay);

            Canvas.GetComponent<CanvasScript>().dailyBonusPanel.SetActive(true);
            Canvas.GetComponent<CanvasScript>().dailyBlockPanel.gameObject.SetActive(true);
        } else {
            DailyChallanges.instance.LoadArray();
        }
    }

    [System.NonSerialized] public float keepDriftScore;
    [System.NonSerialized] public int stopScore = 1;
    void CalculateScoreSeperate ()
    {
        float regularScore = Time.deltaTime * 20 * stopScore * scoreMultBonus * GetComponent<UpgradesContainer>().upgradeMultiplier;
        float driftScore = regularScore * 10;

        score += regularScore;
        if (Player.GetComponent<Player>().boosted)
            score += regularScore * (Player.GetComponent<Player>().currentSpeed - 20) * 0.5f;
 
        if (Player.GetComponent<Player>().driftLeft || Player.GetComponent<Player>().driftRight)
        {
            keepDriftScore += driftScore;
        }
    }

    public void AddDriftScore ()
    {
        score += keepDriftScore;
        overallDriftScore += keepDriftScore;

        CheckDriftScoreToUnlockSports(keepDriftScore);
        if (keepDriftScore > maxDrift)
        {
            ES3.Save<float>("maxDrift", keepDriftScore);
            maxDrift = keepDriftScore;
        }
        DailyChallanges.instance.AddDriftScore(Mathf.RoundToInt(keepDriftScore));

        keepDriftScore = 0;
    }

    public void Loose ()
    {
        Taptic.Default();

        scoreMultTimer = 0;

        AnalyticsEvent.Custom("Player lost", new Dictionary<string, object>
        {
            { "Score", Mathf.Round(score)},
            { "Time since load", Time.timeSinceLevelLoad },
            { "Overall app runtime in minutes", Mathf.Round(OverallAppRuntimeInSeconds/60) },
        });

        lost = true;
        keepDriftScore = 0;
        Player.GetComponent<SplineFollower>().enabled = false;
        Player.GetComponent<Player>().carModel.SetActive(false);
        Player.GetComponent<Player>().boosted = false;
        Player.GetComponent<Player>().HideDriftTip();
        Player.GetComponent<Player>().boostedMultiplier = 0;
        Player.GetComponent<Player>().explosion.Play();
        if (Player.GetComponent<Player>().driftLeft || Player.GetComponent<Player>().driftRight)
            Player.GetComponent<Player>().ExitDrift();
        stopScore = 0;
        if (score > maxScore)
        {
            ES3.Save<float>("maxScore", score);
            maxScore = score;
            GetComponent<Leaderboard>().PostScoreOnLeaderBoard(Mathf.RoundToInt(score));
        }

        GetComponent<LevelManager>().SaveLevel();

        crashTimes++;

        StartCoroutine(Canvas.GetComponent<CanvasScript>().ShowLostScreen());

        CheckScoreToUnlockMustang(score);
        CheckOverallDriftScoreToUnlockHotrod();
        CheckCrashTimesToUnlockSUV();
        //CheckNumberNumberOfDaysToUnlockJeep();

        Image[] fuel = Canvas.GetComponent<CanvasScript>().fullFuelBar.GetComponentsInChildren<Image>();
        foreach (Image img in fuel)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);
        }

        Image[] levelBar = Canvas.GetComponent<CanvasScript>().levelBar.gameObject.GetComponentsInChildren<Image>();
        foreach (Image img in levelBar)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.5f);
        }

        Player.GetComponent<Player>().HideTutorial();

        DailyChallanges.instance.AddCrashTimes(1);
        DailyChallanges.instance.AddScore(Mathf.RoundToInt(score));
    }

    void CheckScoreToUnlockMustang(float scr)
    {
        if (scr >= 80000 && mustangUn == false)
        {
            mustangUn = true;
            ES3.Save<int>("mustang", 1);
            unlockedScreen.SetActive(true);
            unlockedScreen.transform.GetChild(0).GetComponent<UnlockedCars>().mustang = true;

            AnalyticsEvent.Custom("Mustang unlocked", new Dictionary<string, object>
            {
                { "Overall app runtime in minutes", Mathf.Round(OverallAppRuntimeInSeconds/60) },
            });
        }
    }
    void CheckDriftScoreToUnlockSports(float scr)
    {
        ES3.Save<float>("maxDrift", scr);

        if (scr >= 30000 && sportsUn == false)
        {
            sportsUn = true;
            ES3.Save<int>("sports", 1);
            unlockedScreen.SetActive(true);
            unlockedScreen.transform.GetChild(0).GetComponent<UnlockedCars>().sports = true;

            AnalyticsEvent.Custom("Sports car unlocked", new Dictionary<string, object>
            {
                { "Overall app runtime in minutes", Mathf.Round(OverallAppRuntimeInSeconds/60) },
            });
        }
    }
    void CheckOverallDriftScoreToUnlockHotrod()
    {
        ES3.Save<float>("overallDriftScore", overallDriftScore);

        if (overallDriftScore >= 500000 && hotrodUn == false)
        {
            hotrodUn = true;
            ES3.Save<int>("hotrod", 1);
            unlockedScreen.SetActive(true);
            unlockedScreen.transform.GetChild(0).GetComponent<UnlockedCars>().hotrod = true;

            AnalyticsEvent.Custom("Hotrod unlocked", new Dictionary<string, object>
            {
                { "Overall app runtime in minutes", Mathf.Round(OverallAppRuntimeInSeconds/60) },
            });
        }
    }
    void CheckCrashTimesToUnlockSUV()
    {
        ES3.Save<int>("crashTimes", crashTimes);

        if (crashTimes >= 60 && suvUn == false)
        {
            suvUn = true;
            ES3.Save<int>("suv", 1);
            unlockedScreen.SetActive(true);
            unlockedScreen.transform.GetChild(0).GetComponent<UnlockedCars>().suv = true;

            AnalyticsEvent.Custom("SUV unlocked", new Dictionary<string, object>
            {
                { "Overall app runtime in minutes", Mathf.Round(OverallAppRuntimeInSeconds/60) },
            });
        }
    }
    public void UnlockJeep ()
    {
        jeepUn = true;
        ES3.Save<int>("jeep", 1);

        AnalyticsEvent.Custom("Jeep unlocked", new Dictionary<string, object>
        {
            { "Overall app runtime in minutes", Mathf.Round(OverallAppRuntimeInSeconds/60) },
        });
    }

    void ShowRegularAd()
    {
        Advertisement.Show("video");

        AnalyticsEvent.Custom("Showed regual ad", new Dictionary<string, object>
        {
            { "Time since load", Time.timeSinceLevelLoad }
        });
    }
    public void ShowRewardedVideoContinue ()
    {
        Taptic.Selection();
        Advertisement.Show("rewardedVideo");

        AnalyticsEvent.Custom("Showed rewarded ad", new Dictionary<string, object>
        {
            { "Score", Mathf.Round(score)},
            { "Time since load", Time.timeSinceLevelLoad }
        });
    }
    bool watched;
    public void ShowRewardedVideoCrystals ()
    {
        Taptic.Selection();
        watched = true;
        Advertisement.Show("rewardedVideoCrystals");

        AnalyticsEvent.Custom("Showed rewarded crystal ad", new Dictionary<string, object>
        {
            { "Score", Mathf.Round(score)},
            { "Time since load", Time.timeSinceLevelLoad }
        });
    }
    IEnumerator ShowBannerWhenReady()
    {
        if (!noAds)
        {
            while (!Advertisement.IsReady("bannerPlacement"))
            {
                yield return new WaitForSeconds(0.5f);
            }
            Advertisement.Banner.Show("bannerPlacement");
        }
    }
    public void StopShowingBanner ()
    {
        Advertisement.Banner.Hide();
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        // Define conditional logic for each ad completion status:
        if (showResult == ShowResult.Finished)
        {
            Time.timeScale = 1;
            Canvas.GetComponent<CanvasScript>().play.SetActive(true);
            if (placementId == "rewardedVideo")
                Continue();
            
            if (placementId == "rewardedVideoCrystals")
                GetComponent<Crystals>().AddCrystal(20);
        }
        else if (showResult == ShowResult.Skipped)
        {
            Time.timeScale = 1;
            Canvas.GetComponent<CanvasScript>().play.SetActive(true);
        }
        else if (showResult == ShowResult.Failed)
        {
            Time.timeScale = 1;
            Debug.LogWarning("The ad did not finish due to an error.");
        }
    }
    public void OnUnityAdsReady(string placementId)
    {
        if (placementId == "rewardedVideo")
        {
            Canvas.GetComponent<CanvasScript>().continueButton.GetComponent<Button>().interactable = true;
        }
        if (placementId == "rewardedVideoCrystals")
        {
            Canvas.GetComponent<CanvasScript>().videoForCrystalsButton.GetComponent<Button>().interactable = true;
        }
        if (placementId == "video")
            regualAdReady = true;
    }
    public void OnUnityAdsDidError(string message)
    {
        // Log the error.
    }
    public void OnUnityAdsDidStart(string placementId)
    {
        Time.timeScale = 0;
        Canvas.GetComponent<CanvasScript>().play.SetActive(false);
    }

    public void Continue ()
    {
        lost = false;
        desiredY = Player.GetComponent<Player>().carModel.transform.localPosition.y;
        Invoke("LateStart", 1.5f);
        StartCoroutine(modelBlick());
        Player.GetComponent<Player>().carModel.SetActive(true);
        Player.GetComponent<Player>().driftTimer = 20;
        Player.GetComponent<Player>().carModel.GetComponent<BoxCollider>().enabled = false;
        Player.GetComponent<Player>().carModel.transform.localPosition += new Vector3(0, 2, 0);
        y = Player.GetComponent<Player>().carModel.transform.localPosition.y;

        Canvas.GetComponent<CanvasScript>().HideLostScreen();
        Canvas.GetComponent<CanvasScript>().pauseButton.gameObject.SetActive(true);

        unlockedScreen.SetActive(false);

        Image[] fuel = Canvas.GetComponent<CanvasScript>().fullFuelBar.GetComponentsInChildren<Image>();
        foreach (Image img in fuel)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        }

        Image[] levelBar = Canvas.GetComponent<CanvasScript>().levelBar.gameObject.GetComponentsInChildren<Image>();
        foreach (Image img in levelBar)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1f);
        }

        DailyChallanges.instance.AddScore(Mathf.RoundToInt(-score));
    }
    public void Restart ()
    {
        Taptic.Selection();

        Destroy(Player.GetComponent<Player>().nextSegment);
        Destroy(Player.GetComponent<Player>().previousSegment);
        Destroy(Player.GetComponent<Player>().previousSegment2);
        Destroy(Player.GetComponent<Player>().currentSegment);
        Destroy(Player.GetComponent<Player>().carModel);

        Player.GetComponent<Player>().SpawnNewModel();
        Player.GetComponent<Player>().gameStarted = false;
        Player.GetComponent<Player>().boostedMultiplier = 1;
        Player.GetComponent<Player>().directionalLight.transform.rotation = Quaternion.Euler(50, -180, 0);

        Canvas.GetComponent<CanvasScript>().RestartGame();

        score = 0;

        lastPoint.SetPosition(Vector3.zero);
        roadGenerator.GetComponent<RoadGenerator>().GenerateSegment();
        Player.GetComponent<Player>().currentSegment = Player.GetComponent<Player>().nextSegment;
        Player.GetComponent<Player>().nextSegment = null;

        Player.GetComponent<SplineFollower>().enabled = false;
        Player.GetComponent<SplineFollower>().autoFollow = false;
        Player.GetComponent<SplineFollower>().computer = Player.GetComponent<Player>().currentSegment.GetComponent<SplineComputer>();
        Player.GetComponent<SplineFollower>().SetDistance(0);
        Player.GetComponent<Player>().driftFuel = 100;
        StartCoroutine(Player.GetComponent<Player>().GenerateSecondSegment());
        Player.transform.position = new Vector3(0, 5, -20);
        Player.transform.rotation = Quaternion.Euler(0, 180, 0);

        Camera.main.transform.localPosition = new Vector3(0, 1.65f, -4.5f);
        Camera.main.transform.rotation = Quaternion.Euler(9.475f, -180, 0);
        Camera.main.rect = new Rect(0, 0.3f, 1, 1);
        Camera.main.GetComponent<BackgroundColor>().gameStarted = false;

        lost = false;

        Canvas.GetComponent<CanvasScript>().HideLostScreen();
        unlockedScreen.SetActive(false);

        GetComponent<LevelManager>().Lost();

        DailyChallanges.instance.CheckDailys();
    }

    IEnumerator modelBlick ()
    {
        int blinkTimes = 13;
        MeshRenderer[] CarMesh = Player.GetComponent<Player>().carModel.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i <= blinkTimes; i ++)
        {
            foreach (MeshRenderer me in CarMesh)
            {
                me.enabled = false;
            }
            yield return new WaitForSeconds(0.15f);
            foreach (MeshRenderer me in CarMesh)
            {
                me.enabled = true;
            }
            yield return new WaitForSeconds(0.15f);
        }
        Player.GetComponent<Player>().carModel.GetComponent<BoxCollider>().enabled = true;
    }

    void LateStart ()
    {
        Player.GetComponent<SplineFollower>().enabled = true;
        Player.GetComponent<Player>().boostedMultiplier = 1;
        stopScore = 1;
        land = true;
    }

    float y;
    float desiredY;
    bool land;
    void LandCar ()
    {
        Invoke("StopLanding", 1);
        Player.GetComponent<Player>().carModel.transform.localPosition = new Vector3(Player.GetComponent<Player>().carModel.transform.localPosition.x, y, Player.GetComponent<Player>().carModel.transform.localPosition.z);
        y = Mathf.Lerp(y, desiredY, 0.25f);
    }
    void StopLanding ()
    {
        land = false;
    }

    void OnApplicationPause(bool pause)
    {
        ES3.Save<int>("OverallAppRuntimeInSeconds", Mathf.RoundToInt(OverallAppRuntimeInSeconds));
    } 
    void OnApplicationQuit ()
    {
        ES3.Save<int>("OverallAppRuntimeInSeconds", Mathf.RoundToInt(OverallAppRuntimeInSeconds));
    }

    [System.NonSerialized] public float scoreMultTimer = 0;
    bool one;
    public void ScoreMultiplier ()
    {
        if (scoreMultTimer > 0)
        {
            Canvas.GetComponent<CanvasScript>().multBar.gameObject.SetActive(true);
            scoreMultBonus = 2;
            Canvas.GetComponent<CanvasScript>().multBar.GetComponent<Image>().fillAmount = scoreMultTimer / 10;
            scoreMultTimer -= Time.deltaTime;

            if (scoreMultTimer <= 3 && !one) StartCoroutine(MultBonusBlink());
        }
        else
        {
            Canvas.GetComponent<CanvasScript>().multBar.gameObject.SetActive(false);
            scoreMultBonus = 1;
        }
    }

    IEnumerator MultBonusBlink () {
        one = true;
        Image[] bars = Canvas.GetComponent<CanvasScript>().multBar.GetComponentsInChildren<Image>();

        for (int i = 0; i < 6; i++)
        {
            if (scoreMultTimer > 3) break;
            foreach (Image img in bars) { img.enabled = false;  Canvas.GetComponent<CanvasScript>().multBar.transform.GetChild(0).GetChild(0).gameObject.SetActive(false); } 
            yield return new WaitForSeconds(0.25f);
            foreach (Image img in bars) { img.enabled = true; Canvas.GetComponent<CanvasScript>().multBar.transform.GetChild(0).GetChild(0).gameObject.SetActive(true); }
            yield return new WaitForSeconds(0.25f);
        }
        one = false;
    }
}
