using UnityEngine;

public enum HapticTypes { Selection, Success, Warning, Failure, LightImpact, MediumImpact, HeavyImpact }
public class AndroidTaptic {

    public static long LightDuration = 20;
    public static long MediumDuration = 40;
    public static long HeavyDuration = 80;
    public static int LightAmplitude = 40;
    public static int MediumAmplitude = 120;
    public static int HeavyAmplitude = 255;
    private static int _sdkVersion = -1;
    private static long[] _successPattern = { 0, LightDuration, LightDuration, HeavyDuration };
    private static int[] _successPatternAmplitude = { 0, LightAmplitude, 0, HeavyAmplitude };
    private static long[] _warningPattern = { 0, HeavyDuration, LightDuration, MediumDuration };
    private static int[] _warningPatternAmplitude = { 0, HeavyAmplitude, 0, MediumAmplitude };
    private static long[] _failurePattern = { 0, MediumDuration, LightDuration, MediumDuration, LightDuration, HeavyDuration, LightDuration, LightDuration };
    private static int[] _failurePatternAmplitude = { 0, MediumAmplitude, 0, MediumAmplitude, 0, HeavyAmplitude, 0, LightAmplitude };

    void Vib() {
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }

    public static void Vibrate() {
        AndroidVibrate(MediumDuration);
    }

    public static void Haptic(HapticTypes type) {
        try {
            switch (type) {
                case HapticTypes.Selection:
                    AndroidVibrate(LightDuration, LightAmplitude);
                    break;

                case HapticTypes.Success:
                    AndroidVibrate(_successPattern, _successPatternAmplitude, -1);
                    break;

                case HapticTypes.Warning:
                    AndroidVibrate(_warningPattern, _warningPatternAmplitude, -1);
                    break;

                case HapticTypes.Failure:
                    AndroidVibrate(_failurePattern, _failurePatternAmplitude, -1);
                    break;

                case HapticTypes.LightImpact:
                    AndroidVibrate(LightDuration, LightAmplitude);
                    break;

                case HapticTypes.MediumImpact:
                    AndroidVibrate(MediumDuration, MediumAmplitude);
                    break;

                case HapticTypes.HeavyImpact:
                    AndroidVibrate(HeavyDuration, HeavyAmplitude);
                    break;
            }
        } catch (System.NullReferenceException e) {
            Debug.Log(e.StackTrace);
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    private static AndroidJavaObject CurrentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    private static AndroidJavaObject AndroidVibrator = CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
    private static AndroidJavaClass VibrationEffectClass;
    private static AndroidJavaObject VibrationEffect;
    private static int DefaultAmplitude;
#else
    private static AndroidJavaClass UnityPlayer;
    private static AndroidJavaObject CurrentActivity;
    private static AndroidJavaObject AndroidVibrator = null;
    private static AndroidJavaClass VibrationEffectClass = null;
    private static AndroidJavaObject VibrationEffect;
    private static int DefaultAmplitude;
#endif

    public static void AndroidVibrate(long milliseconds) {
        if (AndroidVibrator != null) {
            AndroidVibrator.Call("vibrate", milliseconds);
        }
    }

    public static void AndroidVibrate(long milliseconds, int amplitude) {
        if ((AndroidSDKVersion() < 26)) {
            AndroidVibrate(milliseconds);
        } else {
            VibrationEffectClassInitialization();
            if (VibrationEffectClass != null) {
                VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", new object[] { milliseconds, amplitude });
                if (VibrationEffect != null && AndroidVibrator != null) {
                    AndroidVibrator.Call("vibrate", VibrationEffect);
                } else {
                    AndroidVibrate(milliseconds);
                }
            } else {
                AndroidVibrate(milliseconds);
            }
        }
    }

    public static void AndroidVibrate(long[] pattern, int repeat) {
        if ((AndroidSDKVersion() < 26)) {
            if (AndroidVibrator != null) {
                AndroidVibrator.Call("vibrate", pattern, repeat);
            }
        } else {
            VibrationEffectClassInitialization();
            if (VibrationEffectClass != null) {
                VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", new object[] { pattern, repeat });
                if (VibrationEffect != null && AndroidVibrator != null) {
                    AndroidVibrator.Call("vibrate", VibrationEffect);
                }
            }
        }
    }

    public static void AndroidVibrate(long[] pattern, int[] amplitudes, int repeat) {
        if ((AndroidSDKVersion() < 26)) {
            if (AndroidVibrator != null) {
                AndroidVibrator.Call("vibrate", pattern, repeat);
            }
        } else {
            VibrationEffectClassInitialization();
            if (VibrationEffectClass != null) {
                VibrationEffect = VibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", new object[] { pattern, amplitudes, repeat });
                if (VibrationEffect != null && AndroidVibrator != null) {
                    AndroidVibrator.Call("vibrate", VibrationEffect);
                }
            }
        }
    }

    public static void AndroidCancelVibrations() {
        AndroidVibrator.Call("cancel");
    }

    private static void VibrationEffectClassInitialization() {
        if (VibrationEffectClass == null) { VibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect"); }
    }

    public static int AndroidSDKVersion() {
        if (_sdkVersion == -1) {
            int apiLevel = int.Parse(SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.IndexOf("-") + 1, 3));
            _sdkVersion = apiLevel;
            return apiLevel;
        } else {
            return _sdkVersion;
        }
    }

}