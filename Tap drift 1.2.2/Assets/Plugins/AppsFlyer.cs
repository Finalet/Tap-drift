using System;
using System.Collections;
using UnityEngine;
// We need this one for importing our IOS functions
using System.Runtime.InteropServices;
using System.Collections.Generic;

/*
 v4.20.3
*/
public class AppsFlyer : MonoBehaviour {

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void mSetCurrencyCode(string currencyCode);

    [DllImport("__Internal")]
    private static extern void mSetCustomerUserID(string customerUserID);
    
    [DllImport("__Internal")]
    private static extern void mSetAppsFlyerDevKey(string devKey);
    
    [DllImport("__Internal")]
    private static extern void mTrackAppLaunch();
    
    [DllImport("__Internal")]
    private static extern void mSetAppID(string appleAppId);
    
    [DllImport("__Internal")]
    private static extern void mTrackRichEvent(string eventName, string eventValues);
    
    [DllImport("__Internal")]
    private static extern void mValidateReceipt(string productIdentifier, string price, string currency, string transactionId ,string additionalParams);
    
    [DllImport("__Internal")]
    private static extern void mSetIsDebug(bool isDebug);
    
    [DllImport("__Internal")]
    private static extern void mSetIsSandbox(bool isSandbox);
    
    [DllImport("__Internal")]
    private static extern void mGetConversionData();
    
    [DllImport("__Internal")]
    private static extern void mHandleOpenUrl(string url, string sourceApplication, string annotation);
    
    [DllImport("__Internal")]
    private static extern string mGetAppsFlyerId();

    [DllImport("__Internal")]
    private static extern void mHandlePushNotification(string payload);

    [DllImport("__Internal")]
    private static extern void mRegisterUninstall(byte[] pushToken);

    [DllImport("__Internal")]
    private static extern void mSetShouldCollectDeviceName(bool shouldCollectDeviceName);

    [DllImport("__Internal")]
    private static extern void mSetDeviceTrackingDisabled(bool state);

    [DllImport("__Internal")]
    private static extern void mIsStopTracking(bool isStopTracking);

    [DllImport("__Internal")]
    public static extern void mSetAdditionalData(string extraData);

    [DllImport("__Internal")]
    public static extern void mSetAppInviteOneLinkID(string oneLinkID);
    
    [DllImport("__Internal")]
    public static extern void mGenerateUserInviteLink(string parameters);

    [DllImport("__Internal")]
    public static extern void mTrackCrossPromoteImpression(string appId, string campaign);

    [DllImport("__Internal")]
    public static extern void mTrackAndOpenStore(string promotedAppId, string campaign, string userParams);

    [DllImport("__Internal")]
    public static extern void mSetMinTimeBetweenSessions(int seconds);

    [DllImport("__Internal")]
    public static extern void mSetHost(string hostName, string hostPrefix);

    [DllImport("__Internal")]
    public static extern string mGetHost();

    [DllImport("__Internal")]
    public static extern void mSetUserEmails(EmailCryptType cryptType, int length, params string[] userEmails);

    [DllImport("__Internal")]
    public static extern void mSetResolveDeepLinkURLs(int length, params string[] userEmails);

    [DllImport("__Internal")]
    public static extern void msetOneLinkCustomDomain(int length, params string[] domains);

    [DllImport("__Internal")]
    public static extern void mSetValue(string value);

    public static void setCurrencyCode(string currencyCode){
        mSetCurrencyCode(currencyCode);
    }
    
    public static void setCustomerUserID(string customerUserID){
        mSetCustomerUserID(customerUserID);
    }
    
    public static void setAppsFlyerKey(string key){
        mSetAppsFlyerDevKey(key);
    }
    
    public static void trackAppLaunch(){
        mTrackAppLaunch();
    }
    
    public static void setAppID(string appleAppId){
        mSetAppID(appleAppId);
    }
    
    public static void trackRichEvent(string eventName, Dictionary<string, string> eventValues) {
        
        string attributesString = "";
        foreach(KeyValuePair<string, string> kvp in eventValues)
        {
            attributesString += kvp.Key + "=" + kvp.Value + "\n";
        }
        
        mTrackRichEvent (eventName, attributesString);
    }
    
    public static void validateReceipt(string productIdentifier, string price, string currency, string transactionId, Dictionary<string,string> additionalParametes) {
        string attributesString = "";
        foreach(KeyValuePair<string, string> kvp in additionalParametes)
        {
            attributesString += kvp.Key + "=" + kvp.Value + "\n";
        }
        mValidateReceipt (productIdentifier, price, currency, transactionId, attributesString);
    }
    
    public static void setIsDebug(bool isDebug){
        mSetIsDebug(isDebug);
    }
    
    public static void setIsSandbox(bool isSandbox){
        mSetIsSandbox(isSandbox);
    }
    
    public static void getConversionData () {
        mGetConversionData ();
    }

    public static string getAppsFlyerId () {
        return mGetAppsFlyerId ();
    }

    public static void handleOpenUrl(string url, string sourceApplication, string annotation) {
        
        mHandleOpenUrl (url, sourceApplication, annotation);
    }

    public static void handlePushNotification(Dictionary<string, string> payload) {
        string attributesString = "";
        foreach(KeyValuePair<string, string> kvp in payload) {
            attributesString += kvp.Key + "=" + kvp.Value + "\n";
        }
        mHandlePushNotification(attributesString);
    }

    public static void registerUninstall(byte[] token) {
        mRegisterUninstall(token);
    }

    public static void setShouldCollectDeviceName(bool shouldCollectDeviceName) {
        mSetShouldCollectDeviceName(shouldCollectDeviceName);
    }

     public static void setDeviceTrackingDisabled(bool state){
        mSetDeviceTrackingDisabled(state);
    }

    public static void setAdditionalData(Dictionary<string, string> extraData) {    
        string extraDataString = "";
        foreach(KeyValuePair<string, string> kvp in extraData)
        {
            extraDataString += kvp.Key + "=" + kvp.Value + "\n";
        }
        
        mSetAdditionalData(extraDataString);
    }

    public static void stopTracking(bool isStopTracking) {
        mIsStopTracking(isStopTracking);
    }

        public static void setAppInviteOneLinkID(string oneLinkID) {
        mSetAppInviteOneLinkID(oneLinkID);
    }

    public static void generateUserInviteLink(Dictionary<string,string> parameters, string callbackObject,string callbackMethod, string callbackFailedMethod) {
        string parametersString = "";
        foreach(KeyValuePair<string, string> kvp in parameters) {
            parametersString += kvp.Key + "=" + kvp.Value + "\n";
        }
        
        mGenerateUserInviteLink(parametersString);
    }
    
    public static void trackCrossPromoteImpression(string appId, string campaign) {
        mTrackCrossPromoteImpression(appId,campaign);
    }

    public static void trackAndOpenStore(string promotedAppId, string campaign, Dictionary<string,string> customParams) {
        string userParamsString = "";
        if (customParams != null) {
            foreach(KeyValuePair<string, string> kvp in customParams) {
                userParamsString += kvp.Key + "=" + kvp.Value + "\n";
            }
        }
        mTrackAndOpenStore(promotedAppId,campaign,userParamsString);
    }

    public static void setMinTimeBetweenSessions(int seconds) {
        mSetMinTimeBetweenSessions(seconds);
    }

    public static void setHost(string hostPrefixName, string hostName) {
        mSetHost(hostPrefixName, hostName);
    }

    public static string getHost(){
        return mGetHost();
    }

    public static void setUserEmails(EmailCryptType cryptType, params string[] userEmails) {
        mSetUserEmails(cryptType, userEmails.Length, userEmails);
    }

    public static void setResolveDeepLinkURLs(params string[] domainArray) {
        mSetResolveDeepLinkURLs(domainArray.Length, domainArray);
    }

    public static void setOneLinkCustomDomain(params string[] domains) {
        msetOneLinkCustomDomain(domains.Length, domains);
    }

    public static void setValue(string value) {
        mSetValue(value);
    }



#elif UNITY_ANDROID && !UNITY_EDITOR

    private static AndroidJavaClass obj = new AndroidJavaClass ("com.appsflyer.AppsFlyerLib");
    private static AndroidJavaObject cls_AppsFlyer = obj.CallStatic<AndroidJavaObject>("getInstance");
    private static AndroidJavaClass propertiesClass = new AndroidJavaClass ("com.appsflyer.AppsFlyerProperties");
    // private static AndroidJavaObject afPropertiesInstance = propertiesClass.CallStatic<AndroidJavaObject>("getInstance");
    private static AndroidJavaClass cls_AppsFlyerHelper = new AndroidJavaClass("com.appsflyer.AppsFlyerUnityHelper");
    private static string devKey;

    private static AndroidJavaClass cls_UnityShareHelper = new AndroidJavaClass("com.appsflyer.UnityShareHelper");
    private static AndroidJavaObject ShareHelperInstance = cls_UnityShareHelper.CallStatic<AndroidJavaObject>("getInstance");

    private static AndroidJavaClass cls_AndroidShare = new AndroidJavaClass("com.appsflyer.share.CrossPromotionHelper");
    
    public static void setCurrencyCode(string currencyCode){
        cls_AppsFlyer.Call("setCurrencyCode", currencyCode);
    }
    
    public static void  setCustomerUserID(string customerUserID){
        cls_AppsFlyer.Call("setAppUserId", customerUserID);
    }


    public static void loadConversionData(string callbackObject){
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) 
        {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                cls_AppsFlyerHelper.CallStatic("createConversionDataListener", cls_Activity, callbackObject);    
            }
        }
    }

    [System.Obsolete("Use loadConversionData(string callbackObject)")]
    public static void loadConversionData(string callbackObject, string callbackMethod, string callbackFailedMethod){
        loadConversionData(callbackObject);
    }
                                        

    public static void setCollectIMEI (bool shouldCollect) {
        cls_AppsFlyer.Call("setCollectIMEI", shouldCollect);
    }
    
    public static void setCollectAndroidID (bool shouldCollect) {
        print("AF.cs setCollectAndroidID");
        cls_AppsFlyer.Call("setCollectAndroidID", shouldCollect);
    }

    /**
    *  This method initializes AppsFlyer SDK with getConversionData callback
    */
    public static void init(string key, string callbackObject){
        AppsFlyer.init(key);

        if(callbackObject != null){
          AppsFlyer.loadConversionData(callbackObject);
        }
    }

    public static void init(string key){
        print("AF.cs init");
        devKey = key;
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass ("com.unity3d.player.UnityPlayer")) {
            using (AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject> ("currentActivity")) {
                cls_Activity.Call("runOnUiThread", new AndroidJavaRunnable(init_cb));
            }
        }
    }



    static void init_cb() {

        print("AF.cs start tracking");
        trackAppLaunch ();
    }

    
    public static void setAppsFlyerKey(string key){
        print("AF.cs setAppsFlyerKey");
    }
    
    public static void trackAppLaunch(){
        print("AF.cs trackAppLaunch");
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject cls_Application = cls_Activity.Call<AndroidJavaObject>("getApplication");
                cls_AppsFlyer.Call("startTracking", cls_Application, devKey);
                cls_AppsFlyer.Call("trackAppLaunch",cls_Activity, devKey);
            }
        }        
    }

    public static void setAppID(string packageName){
        // In Android we take the package name
        cls_AppsFlyer.Call("setAppId", packageName);
    }
    
    public static void createValidateInAppListener(string aObject, string callbackMethod, string callbackFailedMethod) {
        print ("AF.cs createValidateInAppListener called");
        
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                cls_AppsFlyerHelper.CallStatic("createValidateInAppListener", cls_Activity, aObject, callbackMethod, callbackFailedMethod);
            }
        }        
    }
    

    public static void validateReceipt(string publicKey, string purchaseData, string signature, string price, string currency, Dictionary<string,string> extraParams) {
        print ("AF.cs validateReceipt pk = " + publicKey + " data = " + purchaseData + "sig = " + signature);
        
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject convertedDict = null;
                if (extraParams != null) {
                    convertedDict = ConvertHashMap (extraParams);
                }
                print ("inside cls_activity");
                cls_AppsFlyer.Call("validateAndTrackInAppPurchase",cls_Activity, publicKey, signature, purchaseData, price, currency, convertedDict);
            }
        }        
    }
    
    
    public static void trackRichEvent(string eventName, Dictionary<string, string> eventValues){
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject convertedDict = ConvertHashMap (eventValues);
                cls_AppsFlyer.Call("trackEvent",cls_Activity, eventName, convertedDict);
            }
        }    
    }
    
    //turn a dictionary into hashmap, to pass it in JNI
    private static AndroidJavaObject ConvertHashMap(Dictionary<string,string> dict)
    {
        AndroidJavaObject obj_HashMap = new AndroidJavaObject("java.util.HashMap");
        
        IntPtr method_Put = AndroidJNIHelper.GetMethodID(obj_HashMap.GetRawClass(), "put", 
                                                         "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
        
        if(dict==null){
            return null;
        }

        object[] args = new object[2];
        foreach(KeyValuePair<string, string> kvp in dict)
        {
            using(AndroidJavaObject k = new AndroidJavaObject("java.lang.String", kvp.Key))
            {
                using(AndroidJavaObject v = new AndroidJavaObject("java.lang.String", kvp.Value))
                {
                    args[0] = k;
                    args[1] = v;
                    AndroidJNI.CallObjectMethod(obj_HashMap.GetRawObject(), 
                                                method_Put, AndroidJNIHelper.CreateJNIArgArray(args));
                }
            }
        }
        return obj_HashMap;
    }

    public static void setImeiData(string imeiData) {
        print("AF.cs setImeiData");
        cls_AppsFlyer.Call("setImeiData", imeiData);
    }

    public static void setAndroidIdData(string androidIdData) {
        print("AF.cs setImeiData");
        cls_AppsFlyer.Call("setAndroidIdData", androidIdData);
    }



    public static void setIsDebug(bool isDebug) {
        print("AF.cs setDebugLog");
        cls_AppsFlyer.Call("setDebugLog", isDebug);
    }
    
    public static void setIsSandbox(bool isSandbox){
    }
    
    public static void getConversionData () {
    }
    
    public static void handleOpenUrl(string url, string sourceApplication, string annotation) {
    }

    public static string getAppsFlyerId () {

        string appsFlyerId;
        using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using (AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                appsFlyerId = cls_AppsFlyer.Call <string> ("getAppsFlyerUID", cls_Activity);
            }
        }
        return appsFlyerId;
    }

    public static void setGCMProjectNumber(string googleGCMNumber) {
        cls_AppsFlyer.Call("setGCMProjectNumber", googleGCMNumber);
    }

    public static void updateServerUninstallToken(string token) {
        AndroidJavaClass obj = new AndroidJavaClass ("com.appsflyer.AppsFlyerLib");
        AndroidJavaObject cls_AppsFlyer = obj.CallStatic<AndroidJavaObject>("getInstance");

        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) 
        {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) 
            {
                cls_AppsFlyer.Call("updateServerUninstallToken", cls_Activity, token);
            }
        }
    }

    public static void enableUninstallTracking(string senderId) {
        AndroidJavaClass obj = new AndroidJavaClass ("com.appsflyer.AppsFlyerLib");
        AndroidJavaObject cls_AppsFlyer = obj.CallStatic<AndroidJavaObject>("getInstance");
        cls_AppsFlyer.Call("enableUninstallTracking", senderId);
    }

    public static void setDeviceTrackingDisabled(bool state){
        cls_AppsFlyer.Call("setDeviceTrackingDisabled", state);
    }

    public static void setAdditionalData(Dictionary<string, string> extraData){
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject convertedDict = ConvertHashMap (extraData);
                cls_AppsFlyer.Call("setAdditionalData", convertedDict);
            }
        }   
    }

    public static void stopTracking(bool isStopTracking) {
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject cls_Application = cls_Activity.Call<AndroidJavaObject>("getApplication");
                cls_AppsFlyer.Call("stopTracking", isStopTracking, cls_Application);    
            }
        }
    }


    public static void setAppInviteOneLinkID(string oneLinkID) {
        cls_AppsFlyer.Call("setAppInviteOneLink", oneLinkID);
    }

    public static void generateUserInviteLink(Dictionary<string,string> parameters, string callbackObject, string callbackMethod, string callbackFailedMethod) {
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject cls_Application = cls_Activity.Call<AndroidJavaObject>("getApplication");
                AndroidJavaObject convertedDict = ConvertHashMap (parameters);
                ShareHelperInstance.Call("createOneLinkInviteListener", cls_Application, convertedDict, callbackObject, callbackMethod, callbackFailedMethod);
            }
        }
    }

    public static void trackCrossPromoteImpression(string appId, string campaign) {
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject cls_Application = cls_Activity.Call<AndroidJavaObject>("getApplication");
                    cls_AndroidShare.CallStatic("trackCrossPromoteImpression", cls_Application, appId, campaign);    
            }
        }
    }

    public static void trackAndOpenStore(string promotedAppId, string campaign, Dictionary<string,string> customParams) { 
        using(AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) {
            using(AndroidJavaObject cls_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) {
                AndroidJavaObject cls_Application = cls_Activity.Call<AndroidJavaObject>("getApplication");
                AndroidJavaObject convertedDict = null;
                if (customParams != null) {
                    convertedDict = ConvertHashMap (customParams);
                } 
                ShareHelperInstance.Call("trackAndOpenStore", cls_Application, promotedAppId, campaign, convertedDict);
            }
        }
    }

    public static void setPreinstallAttribution(string mediaSource, string campaign, string siteId) {
        cls_AppsFlyer.Call("setPreinstallAttribution", mediaSource, campaign, siteId);
    }
   
   public static void setMinTimeBetweenSessions(int seconds) {
        cls_AppsFlyer.Call("setMinTimeBetweenSessions", seconds);
    }

    public static void setHost(string hostPrefixName, string hostName) {
        cls_AppsFlyer.Call("setHost", hostPrefixName, hostName);
    }

    public static string getHost(){
        return cls_AppsFlyer.Call<string>("getHost");
    }

    public static void setUserEmails(EmailCryptType cryptType, params string[] userEmails)
    {
        AndroidJavaClass emailsCryptTypeEnum = new AndroidJavaClass("com.appsflyer.AppsFlyerProperties$EmailsCryptType");
        AndroidJavaObject emailsCryptType;

        switch (cryptType)
        {
            case EmailCryptType.EmailCryptTypeSHA1:
                emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("SHA1");
                break;
            case EmailCryptType.EmailCryptTypeMD5:
                emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("MD5");
                break;
            case EmailCryptType.EmailCryptTypeSHA256:
                emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("SHA256");
                break;
            default:
                emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("NONE");
                break;
        }

        cls_AppsFlyer.Call("setUserEmails", emailsCryptType, (object)userEmails);
    }

    public static void setResolveDeepLinkURLs(params string[] userEmails) {
        cls_AppsFlyer.Call("setResolveDeepLinkURLs", (object)userEmails);
    }

    public static void setOneLinkCustomDomain(params string[] domains) {
        cls_AppsFlyer.Call("setOneLinkCustomDomain", (object)domains);
    }




#else
    // Editor (API)
    // Android & iOS
    public static void setCurrencyCode(string currencyCode){}
    public static void setCustomerUserID(string customerUserID){}
    public static void setAppsFlyerKey(string key){}
    public static void trackAppLaunch(){}
    public static void setAppID(string appleAppId){}
    public static void trackRichEvent(string eventName, Dictionary<string, string> eventValues){}
    public static void setIsDebug(bool isDebug){}
    public static void setIsSandbox(bool isSandbox){}
    public static void getConversionData (){}
    public static string getAppsFlyerId () {return null;}
    public static void handleOpenUrl(string url, string sourceApplication, string annotation) {}
    public static void setDeviceTrackingDisabled(bool state) {}
    public static void stopTracking(bool isStopTracking) {}
    public static void setAdditionalData(Dictionary<string, string> extraData) {}
    public static void setAppInviteOneLinkID(string oneLinkID) {}
    public static void generateUserInviteLink(Dictionary<string,string> parameters, string callbackObject,string callbackMethod, string callbackFailedMethod) {}
    public static void trackCrossPromoteImpression(string appId, string campaign) {}
    public static void trackAndOpenStore(string promotedAppId, string campaign, Dictionary<string,string> customParams) {}
    public static void setMinTimeBetweenSessions(int seconds) {}
    public static void setHost(string hostPrefixName, string hostName){}
    public static string getHost() { return null; }
    public static void setUserEmails(EmailCryptType cryptType, params string[] userEmails) {}
    public static void setResolveDeepLinkURLs(params string[] userEmails) {}
    public static void setOneLinkCustomDomain(params string[] domains) {}

    // Android Only 
    public static void validateReceipt(string publicKey, string purchaseData, string signature, string price, string currency, Dictionary<string, string> extraParams) { }
    public static void setCollectIMEI(bool shouldCollect) { }
    public static void setCollectAndroidID(bool shouldCollect) { }
    public static void createValidateInAppListener(string aObject, string callbackMethod, string callbackFailedMethod) { }
    public static void init(string devKey) { }
    public static void init(string devKey, string callbackObject) { }
    public static void setImeiData(string imeiData) { }
    public static void loadConversionData(string callbackObject) { }
    public static void enableUninstallTracking(string senderId) { }
    public static void updateServerUninstallToken(string token) { }
    public static void setAndroidIdData(string androidIdData) { }
    public static void setPreinstallAttribution(string mediaSource, string campaign, string siteId) { }

    // iOS Only
    public static void validateReceipt(string productIdentifier, string price, string currency, string transactionId, Dictionary<string, string> additionalParametes) { }
    public static void handlePushNotification(Dictionary<string, string> payload) { }
    public static void registerUninstall(byte[] token) { }
    public static void setValue(string value){}

    // deprecated APIs
    [System.Obsolete("Use loadConversionData(string callbackObject)")]
    public static void loadConversionData(string callbackObject, string callbackMethod, string callbackFailedMethod){}
    [System.Obsolete("Use enableUninstallTracking(string senderId)")]
    public static void setGCMProjectNumber(string googleGCMNumber){}
    public static void setShouldCollectDeviceName(bool shouldCollectDeviceName) {}
#endif


    public enum EmailCryptType
    {
        // None
        EmailCryptTypeNone = 0,
        // SHA1
        EmailCryptTypeSHA1 = 1,
        // MD5
        EmailCryptTypeMD5 = 2,
        // SHA256
        EmailCryptTypeSHA256 = 3
    }
}
