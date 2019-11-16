//
//  AppsFlyerWarpper.m
//
//
//  Created by AppsFlyer 2013
//
//

#import "AppsFlyerWrapper.h"
#import "AppsFlyerTracker.h"
#import "AppsFlyerDelegate.h"


static AppsFlyerDelegate *mAppsFlyerdelegate;
static const int kPushNotificationSize = 32;

@interface AppsFlyerWarpper () {
}

@end

@implementation AppsFlyerWarpper

+(AppsFlyerDelegate *) getAppsFlyerDelegate {
    
    if (mAppsFlyerdelegate == nil) {
        mAppsFlyerdelegate = [[AppsFlyerDelegate alloc] init];
    }
    return mAppsFlyerdelegate;
}

extern "C" {
    char* cStringAFCopy(const char* string)
    {
        if (string == NULL){
            return NULL;
        }
        
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        
        return res;
    }
    
    const void mSetUserEmails(int emailCryptTypeInt , int length, const char **userEmails){
        
        EmailCryptType emailCryptType;
        switch (emailCryptTypeInt){
            case 1:
                emailCryptType = EmailCryptTypeSHA1;
                break;
            case 2:
                emailCryptType = EmailCryptTypeMD5;
                break;
            case 3:
                emailCryptType = EmailCryptTypeSHA256;
                break;
            default:
                emailCryptType = EmailCryptTypeNone;
                break;
        }
        
        if(length > 0 && userEmails) {
            NSMutableArray<NSString *> *params = [[NSMutableArray alloc] init];
            for(int i = 0; i < length; i++) {
                if (userEmails[i]) {
                    [params addObject:[NSString stringWithUTF8String:userEmails[i]]];
                }
            }
            [[AppsFlyerTracker sharedTracker] setUserEmails:params withCryptType:emailCryptType];
        }
    }
    
    const void mSetResolveDeepLinkURLs(int length, const char **domainArray){
        
        if(length > 0 && domainArray) {
            NSMutableArray<NSString *> *params = [[NSMutableArray alloc] init];
            for(int i = 0; i < length; i++) {
                if (domainArray[i]) {
                    [params addObject:[NSString stringWithUTF8String:domainArray[i]]];
                }
            }
            [[AppsFlyerTracker sharedTracker] setResolveDeepLinkURLs:params];
        }
    }

    const void msetOneLinkCustomDomain(int length, const char **domainArray){
        
        if(length > 0 && domainArray) {
            NSMutableArray<NSString *> *params = [[NSMutableArray alloc] init];
            for(int i = 0; i < length; i++) {
                if (domainArray[i]) {
                    [params addObject:[NSString stringWithUTF8String:domainArray[i]]];
                }
            }
            [[AppsFlyerTracker sharedTracker] setOneLinkCustomDomains:params];
        }
    }
    
    const void mSetHost(const char *hostPrefixName ,const char *hostName){
        NSString *host = [NSString stringWithUTF8String:hostName];
        NSString *prefix = [NSString stringWithUTF8String:hostPrefixName];

        [[AppsFlyerTracker sharedTracker] setHost:host withHostPrefix:prefix];
    }
    
    const char* mGetHost(){
        NSString *hostName = [[AppsFlyerTracker sharedTracker] host];
        return cStringAFCopy([hostName UTF8String]);
    }
    
//    const void mTrackEvent(const char *eventName,const char *eventValue){
//        NSString *name = [NSString stringWithUTF8String:eventName];
//        NSString *value = [NSString stringWithUTF8String:eventValue];
//        [[AppsFlyerTracker sharedTracker] trackEvent:name withValue:value];
//
//    }
    
    const void mTrackRichEvent(const char *eventName, const char *eventValues){
        NSString *name = [NSString stringWithUTF8String:eventName];
        
        NSString *attris = [NSString stringWithUTF8String:eventValues];
        
        NSArray *attributesArray = [attris componentsSeparatedByString:@"\n"];
        
        NSMutableDictionary *oAttributes = [[NSMutableDictionary alloc] init];
        for (int i=0; i < [attributesArray count]; i++) {
            NSString *keyValuePair = [attributesArray objectAtIndex:i];
            NSRange range = [keyValuePair rangeOfString:@"="];
            if (range.location != NSNotFound) {
                NSString *key = [keyValuePair substringToIndex:range.location];
                NSString *value = [keyValuePair substringFromIndex:range.location+1];
                [oAttributes setObject:value forKey:key];
            }
        }
        
        
        [[AppsFlyerTracker sharedTracker] trackEvent:name withValues:oAttributes];
        
    }
    
    const void mSetCurrencyCode(const char *currencyCode){
        NSString *code = [NSString stringWithUTF8String:currencyCode];
        [[AppsFlyerTracker sharedTracker] setCurrencyCode:code];
        
    }
    
    const void mSetCustomerUserID(const char *customerUserID){
        NSString *customerUserIDString = [NSString stringWithUTF8String:customerUserID];
        [[AppsFlyerTracker sharedTracker] setCustomerUserID:customerUserIDString];
        
    }
    
    const void mSetAppsFlyerDevKey(const char *devKey){
        NSString *devKeyString = [NSString stringWithUTF8String:devKey];
        [AppsFlyerTracker sharedTracker].appsFlyerDevKey = devKeyString;
    }
    
    const void mSetMinTimeBetweenSessions(int seconds) {
        [AppsFlyerTracker sharedTracker].minTimeBetweenSessions = seconds;
    }
    
    const void mTrackAppLaunch() {
        [[AppsFlyerTracker sharedTracker] trackAppLaunch];
    }
    
    const void mSetAppID(const char *appleAppID){
        NSString *appleAppIDString = [NSString stringWithUTF8String:appleAppID];
        [AppsFlyerTracker sharedTracker].appleAppID = appleAppIDString;
    }
    
    const void mValidateReceipt(const char *productIdentifier,  const char *price, const char *currency, const char *transactionId ,const char *additionalParams) {
        
        NSString *productIdentifierString = [NSString stringWithUTF8String:productIdentifier];
        NSString *currencyString = [NSString stringWithUTF8String:currency];
        NSString *priceValue = [NSString stringWithUTF8String:price];
        NSString *transactionIdString = [NSString stringWithUTF8String:transactionId];
        
        NSString *attris = [NSString stringWithUTF8String:additionalParams];
        NSArray *attributesArray = [attris componentsSeparatedByString:@"\n"];
        
        NSMutableDictionary *customParams = [[NSMutableDictionary alloc] init];
        for (int i=0; i < [attributesArray count]; i++) {
            NSString *keyValuePair = [attributesArray objectAtIndex:i];
            NSRange range = [keyValuePair rangeOfString:@"="];
            if (range.location != NSNotFound) {
                NSString *key = [keyValuePair substringToIndex:range.location];
                NSString *value = [keyValuePair substringFromIndex:range.location+1];
                [customParams setObject:value forKey:key];
            }
        }
        
        
        [[AppsFlyerTracker sharedTracker] validateAndTrackInAppPurchase:productIdentifierString
                                                                  price:priceValue
                                                               currency:currencyString
                                                          transactionId:transactionIdString
                                                   additionalParameters:customParams
                                                                success:^(NSDictionary *result)
         {
             
             NSData *jsonData;
             
             NSError *jsonError;
             jsonData = [NSJSONSerialization dataWithJSONObject:result
                                                        options:0
                                                          error:&jsonError];
             if (jsonError)
             {
                 UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_VALIDATE_ERROR, [@"Invalid Response" UTF8String]);
             }
             else
             {
                 NSString *JSONString = [[NSString alloc] initWithBytes:[jsonData bytes] length:[jsonData length] encoding:NSUTF8StringEncoding];
                 UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_VALIDATE, [JSONString UTF8String]);
                 
             }
         }
                                                                failure:^(NSError *error, id response)
         {
             NSString *errorString = (!error) ? @"unknown" : [NSString stringWithFormat:@"error: %@", [error localizedDescription]];
             if ([response isKindOfClass:[NSDictionary class]]) {
                 if ([response objectForKey:@"error"] != nil)
                 {
                     errorString = response[@"error"];
                 }
                 else if ([response objectForKey:@"status"] != nil)
                 {
                     errorString = [NSString stringWithFormat:@"Error code = %@", response[@"status"]];
                 }
             }
             else if ([response isKindOfClass:[NSData class]]) {
                 errorString = [[NSString alloc] initWithData:response encoding:NSUTF8StringEncoding];
             }
             else if ([response isKindOfClass:[NSString class]]) {
                 errorString = response;
             }
             
             UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_VALIDATE_ERROR, [errorString UTF8String]);
         }];
    }
    
    const void mSetIsDebug(bool isDebug) {
        [AppsFlyerTracker sharedTracker].isDebug = isDebug;
    }
    
    const void mSetIsSandbox(bool isSandbox) {
        [AppsFlyerTracker sharedTracker].useReceiptValidationSandbox = isSandbox;
    }
    
    const void mIsStopTracking(bool isStopTracking) {
        [AppsFlyerTracker sharedTracker].isStopTracking = isStopTracking;
    }
    
    
    const void mGetConversionData() {
        [[AppsFlyerTracker sharedTracker] setDelegate:[AppsFlyerWarpper getAppsFlyerDelegate]];
    }
    
    
    const void mHandleOpenUrl(const char *url, const char *sourceApplication, const char *annotation) {
        [[AppsFlyerTracker sharedTracker] handleOpenURL:[NSURL URLWithString:[NSString stringWithUTF8String:url]] sourceApplication:[NSString stringWithUTF8String:sourceApplication] withAnnotation:[NSString stringWithUTF8String:annotation]];
    }
    
    const void mHandlePushNotification(const char *payloadData) {
        
        NSString *attris = [NSString stringWithUTF8String:payloadData];
        NSArray *attributesArray = [attris componentsSeparatedByString:@"\n"];
        
        NSMutableDictionary *pushPayloadDict = [[NSMutableDictionary alloc] init];
        for (int i=0; i < [attributesArray count]; i++) {
            NSString *keyValuePair = [attributesArray objectAtIndex:i];
            NSRange range = [keyValuePair rangeOfString:@"="];
            if (range.location != NSNotFound) {
                NSString *key = [keyValuePair substringToIndex:range.location];
                NSString *value = [keyValuePair substringFromIndex:range.location+1];
                [pushPayloadDict setObject:value forKey:key];
            }
        }
        [[AppsFlyerTracker sharedTracker] handlePushNotification:pushPayloadDict];
        
    }
    
    const void mRegisterUninstall (unsigned char *pushToken) {
        NSData* tokenData = [NSData dataWithBytes:(const void *)pushToken length:sizeof(unsigned char)*kPushNotificationSize];
        [[AppsFlyerTracker sharedTracker] registerUninstall:tokenData];
    }
    
    const char *mGetAppsFlyerId () {
        NSString *afid = [[AppsFlyerTracker sharedTracker] getAppsFlyerUID];
        return cStringAFCopy([afid UTF8String]);
    }
    
    const void mSetShouldCollectDeviceName (bool shouldCollectDeviceName) {
        [AppsFlyerTracker sharedTracker].shouldCollectDeviceName = shouldCollectDeviceName;
    }
    
    const void mSetDeviceTrackingDisabled(bool state) {
        [AppsFlyerTracker sharedTracker].deviceTrackingDisabled = state;
    }
    
    const void mSetAdditionalData(const char *extraData){
        NSString *extraDatais = [NSString stringWithUTF8String:extraData];
        
        NSArray *extraDataArray = [extraDatais componentsSeparatedByString:@"\n"];
        
        NSMutableDictionary *oExtraData = [[NSMutableDictionary alloc] init];
        for (int i=0; i < [extraDataArray count]; i++) {
            NSString *keyValuePair = [extraDataArray objectAtIndex:i];
            NSRange range = [keyValuePair rangeOfString:@"="];
            if (range.location != NSNotFound) {
                NSString *key = [keyValuePair substringToIndex:range.location];
                NSString *value = [keyValuePair substringFromIndex:range.location+1];
                [oExtraData setObject:value forKey:key];
            }
        }
        [[AppsFlyerTracker sharedTracker] setAdditionalData:oExtraData];
        
    }
    
    const void mSetAppInviteOneLinkID(const char *oneLinkID) {
        if (oneLinkID != NULL) {
            NSString *oneLinkIdString = [NSString stringWithUTF8String:oneLinkID];
            [AppsFlyerTracker sharedTracker].appInviteOneLinkID = oneLinkIdString;
            
            
        }
    }

    const void mSetValue(const char *value) {
        if (value != NULL) {
            [[AppsFlyerTracker sharedTracker] setValue:@YES forKey:[NSString stringWithUTF8String:value]];
        }
    }
    
    const void mGenerateUserInviteLink(const char *parameters) {
        if (parameters != NULL) {
            NSString *parametersStr = [NSString stringWithUTF8String:parameters];
            NSArray *parametersArray = [parametersStr componentsSeparatedByString:@"\n"];
            
            NSMutableDictionary *oParameters = [[NSMutableDictionary alloc] init];
            for (int i=0; i < [parametersArray count]; i++) {
                NSString *keyValuePair = [parametersArray objectAtIndex:i];
                NSRange range = [keyValuePair rangeOfString:@"="];
                if (range.location != NSNotFound) {
                    NSString *key = [keyValuePair substringToIndex:range.location];
                    NSString *value = [keyValuePair substringFromIndex:range.location+1];
                    [oParameters setObject:value forKey:key];
                }
            }
            
            NSString *channel = nil;
            NSString *campaign = nil;
            NSString *referrerName = nil;
            NSString *referrerImageUrl = nil;
            NSString *customerID = nil;
            NSString *baseDeepLink = nil;
            
            if (![oParameters isKindOfClass:[NSNull class]]) {
                channel = (NSString*)[oParameters objectForKey: @"channel"];
                campaign = (NSString*)[oParameters objectForKey: @"campaign"];
                referrerName = (NSString*)[oParameters objectForKey: @"referrerName"];
                referrerImageUrl = (NSString*)[oParameters objectForKey: @"referrerImageUrl"];
                customerID = (NSString*)[oParameters objectForKey: @"customerID"];
                baseDeepLink = (NSString*)[oParameters objectForKey: @"baseDeepLink"];
                
                [AppsFlyerShareInviteHelper generateInviteUrlWithLinkGenerator:^AppsFlyerLinkGenerator * _Nonnull(AppsFlyerLinkGenerator * _Nonnull generator) {
                    if (channel != nil && ![channel isEqualToString:@""]) {
                        [generator setChannel:channel];
                        [oParameters removeObjectForKey:@"channel"];
                    }
                    if (campaign != nil && ![campaign isEqualToString:@""]) {
                        [generator setCampaign:campaign];
                        [oParameters removeObjectForKey:@"campaign"];
                    }
                    if (referrerName != nil && ![referrerName isEqualToString:@""]) {
                        [generator setReferrerName:referrerName];
                        [oParameters removeObjectForKey:@"referrerName"];
                    }
                    if (referrerImageUrl != nil && ![referrerImageUrl isEqualToString:@""]) {
                        [generator setReferrerImageURL:referrerImageUrl];
                        [oParameters removeObjectForKey:@"referrerImageUrl"];
                    }
                    if (customerID != nil && ![customerID isEqualToString:@""]) {
                        [generator setReferrerCustomerId:customerID];
                        [oParameters removeObjectForKey:@"customerID"];
                    }
                    if (baseDeepLink != nil && ![baseDeepLink isEqualToString:@""]) {
                        [generator setDeeplinkPath:baseDeepLink];
                        [oParameters removeObjectForKey:@"baseDeepLink"];
                    }
                    
                    if([oParameters count] > 0) {
                        [generator addParameters:oParameters];
                    }
                    
                    return generator;
                } completionHandler: ^(NSURL * _Nullable url) {
                    NSString *urlString = url.absoluteString;
                    const char *cStr = [urlString UTF8String];
                    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_GENERATE_LINK, cStr);
                }
                 ];
            }
        }
    }
    
    const void mTrackAndOpenStore(const char *appId, const char *camp, const char *parameters) {
        if (parameters != NULL) {
            NSString *parametersStr = [NSString stringWithUTF8String:parameters];
            NSArray *parametersArray = [parametersStr componentsSeparatedByString:@"\n"];
            NSMutableDictionary *customParams = [[NSMutableDictionary alloc] init];
            
            
            NSString *promtAppID = [NSString stringWithUTF8String:appId];
            NSString *campaign = [NSString stringWithUTF8String:camp];
            
            if ([parametersArray count] > 2 && [parametersArray objectAtIndex:2] != [NSNull null]) {
                for (int i=2; i < [parametersArray count]; i++) {
                    NSString *keyValuePair = [parametersArray objectAtIndex:i];
                    NSRange range = [keyValuePair rangeOfString:@"="];
                    if (range.location != NSNotFound) {
                        NSString *key = [keyValuePair substringToIndex:range.location];
                        NSString *value = [keyValuePair substringFromIndex:range.location+1];
                        [customParams setObject:value forKey:key];
                    }
                }
            }
            
            if (promtAppID != nil && ![promtAppID isEqualToString:@""]) {
                [AppsFlyerCrossPromotionHelper
                 trackAndOpenStore:promtAppID
                 campaign:campaign
                 paramters:customParams
                 openStore:^(NSURLSession * _Nonnull urlSession, NSURL * _Nonnull clickURL) {
                     NSString *urlString = clickURL.absoluteString;
                     const char *cStr = [urlString UTF8String];
                     UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_OPEN_STORE_LINK, cStr);
                 }];
            }
        }
    }
    
    const void mTrackCrossPromoteImpression(const char *appId, const char *camp) {
        if (appId != NULL) {
            NSString *promtAppID = [NSString stringWithUTF8String:appId];
            NSString *campaign = [NSString stringWithUTF8String:camp];
            
            if (promtAppID != nil && ![promtAppID isEqualToString:@""]) {
                [AppsFlyerCrossPromotionHelper trackCrossPromoteImpression:promtAppID campaign:campaign];
            }
        }
    }
}
@end
