using UnityEngine;
using System.Collections;
#if UNITY_IOS
using UnityEngine.iOS;
#else
using GooglePlayGames;
using UnityEngine.SocialPlatforms;
#endif

public class Leaderboard : MonoBehaviour
{

    public bool loginSuccessful;
#if UNITY_IOS
    readonly string leaderboardID = "tapDriftBestScore";
#else
    readonly string leaderboardID = "CgkI8brBmrMQEAIQAQ";
#endif

    void Start()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Activate();
#endif

        AuthenticateUser();
    }
    void AuthenticateUser()
    {
        Social.localUser.Authenticate((bool success) => {
            if (success)
            {
                loginSuccessful = true;
                Debug.Log("successful");
            }
            else
            {
                Debug.Log("unsuccessful");
            }
        });
    }


    public void PostScoreOnLeaderBoard(int myScore)
    {
        if (loginSuccessful)
        {
            Social.ReportScore(myScore, leaderboardID, (bool success) => {
                if (success)
                    Debug.Log("Successfully uploaded");
            });
        }
    }
    public void ShowLeaderBoard()
    {
        Social.ShowLeaderboardUI();
        Taptic.Selection();
    }
}
