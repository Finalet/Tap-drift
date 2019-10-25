using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using Unity.Notifications.iOS;
#else
using Unity.Notifications.Android;
#endif
using UnityEngine;

public class Notifications : MonoBehaviour
{

    void Start()
    {
        RemoveNotifications();

#if UNITY_IOS
        StartCoroutine(RequestAuthorization());
#else
        Invoke("AndroidNotif", 3);
#endif
    }


#if UNITY_IOS
    IEnumerator RequestAuthorization()
    {
        yield return new WaitForSeconds(3);
        using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
        {
            while (!req.IsFinished)
            {
                yield return null;
            };

            string res = "\n RequestAuthorization: \n";
            res += "\n finished: " + req.IsFinished;
            res += "\n granted :  " + req.Granted;
            res += "\n error:  " + req.Error;
            res += "\n deviceToken:  " + req.DeviceToken;
            Debug.Log(res);
        }
        yield return new WaitForSeconds(1);
        TriggerNotification1();
        TriggerNotification2();
        TriggerNotification3();
        TriggerNotification4();
        TriggerNotification5();
    }
    void TriggerNotification1()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(24, 0, 0),

            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can optionally specify a custom identifier which can later be 
            // used to cancel the notification, if you don't set one, a unique 
            // string will be generated automatically.
            Identifier = "24 hour notif",
            Title = "Login for bonuses",
            Body = "Open the game to get daily helpful bonuses!",
            Subtitle = "",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
    void TriggerNotification2()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(48, 0, 0),

            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can optionally specify a custom identifier which can later be 
            // used to cancel the notification, if you don't set one, a unique 
            // string will be generated automatically.
            Identifier = "48 hour notif",
            Title = "New cars!",
            Body = "Login to unlock new exciting cars!",
            Subtitle = "",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
    void TriggerNotification3()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(72, 0, 0),

            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can optionally specify a custom identifier which can later be 
            // used to cancel the notification, if you don't set one, a unique 
            // string will be generated automatically.
            Identifier = "72 hour notif",
            Title = "Upgrades",
            Body = "Login and unlock more advanced upgrades!",
            Subtitle = "",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
    void TriggerNotification4()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(96, 0, 0),

            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can optionally specify a custom identifier which can later be 
            // used to cancel the notification, if you don't set one, a unique 
            // string will be generated automatically.
            Identifier = "96 hour notif",
            Title = "Challange!",
            Body = "We challange you! Try beating your current best score. Check the leaderboard to find friends to compete with.",
            Subtitle = "",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
    void TriggerNotification5()
    {
        var timeTrigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(120, 0, 0),

            Repeats = false
        };

        var notification = new iOSNotification()
        {
            // You can optionally specify a custom identifier which can later be 
            // used to cancel the notification, if you don't set one, a unique 
            // string will be generated automatically.
            Identifier = "120 hour notif",
            Title = "Drifting misses you",
            Body = "You have not visited us in a while. Come back to get special bonuses.",
            Subtitle = "",
            ShowInForeground = true,
            ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
            CategoryIdentifier = "category_a",
            ThreadIdentifier = "thread1",
            Trigger = timeTrigger,
        };

        iOSNotificationCenter.ScheduleNotification(notification);
    }
#else
    void AndroidNotif ()
    {
        var c = new AndroidNotificationChannel()
        {
            Id = "channel_id",
            Name = "Default Channel",
            Importance = Importance.High,
            Description = "Generic notifications",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(c);

        SendAndroidNotif48(); 
        SendAndroidNotif96(); 
        SendAndroidNotif144();
    }
    void SendAndroidNotif48 ()
    {
        var notification = new AndroidNotification();
        notification.Title = "Challange!";
        notification.Text = "We challange you! Try beating your current best score. Check the leaderboard to find friends to compete with.";
        notification.FireTime = DateTime.Now.AddHours(48);
        notification.LargeIcon = "icon_0";

        AndroidNotificationCenter.SendNotification(notification, "channel_id");
    }
    void SendAndroidNotif96()
    {
        var notification = new AndroidNotification();
        notification.Title = "Drifting misses you!";
        notification.Text = "You have not drifted in a while. Catch up to your friends on the leaderboard!";
        notification.FireTime = DateTime.Now.AddHours(96);
        notification.LargeIcon = "icon_0";

        AndroidNotificationCenter.SendNotification(notification, "channel_id");
    }
    void SendAndroidNotif144()
    {
        var notification = new AndroidNotification();
        notification.Title = "Challange!";
        notification.Text = "We challange you! Try beating your current best score. Check the leaderboard to find friends to compete with.";
        notification.FireTime = DateTime.Now.AddHours(144);
        notification.LargeIcon = "icon_0";

        AndroidNotificationCenter.SendNotification(notification, "channel_id");
    }
#endif

    void RemoveNotifications ()
    {
#if UNITY_IOS
        iOSNotificationCenter.RemoveScheduledNotification("24 hour notif");
        iOSNotificationCenter.RemoveScheduledNotification("48 hour notif");
        iOSNotificationCenter.RemoveScheduledNotification("72 hour notif");
        iOSNotificationCenter.RemoveScheduledNotification("96 hour notif");
        iOSNotificationCenter.RemoveScheduledNotification("120 hour notif");
#else
        AndroidNotificationCenter.CancelAllScheduledNotifications();
#endif
    } 
}
