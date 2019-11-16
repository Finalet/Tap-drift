//
//  AppsFlyerDelegate.mm
//  
//
//  Created by Golan on 6/21/15.
//  Edited by Shachar on 5/18/17.
//
//

#import "AppsFlyerDelegate.h"
#import "UnityAppController.h"


@interface AppsFlyerDelegate() {
    BOOL didEnteredBackGround;
}

@end

@implementation AppsFlyerDelegate

- (instancetype)init
{
    if([AppsFlyerTracker sharedTracker].isDebug){
        NSLog(@"initializing AppsFlyerDelegate");
    }
    
    self = [super init];
    if (self) {
    }
    return self;
}

// AppsFlyer Callbacks

- (void) onConversionDataReceived:(NSDictionary*) installData {
    
    NSString *conversionDataResult = [self getJsonStringFromDictionary:installData];
    if([AppsFlyerTracker sharedTracker].isDebug){
        NSLog (@"AppsFlyerDelegate onConversionDataReceived = %@", conversionDataResult);
    }
    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_CONVERSION, [conversionDataResult UTF8String]);
}

- (void) onConversionDataRequestFailure:(NSError *)error {
    
    NSString *errDesc = [error localizedDescription];
    if([AppsFlyerTracker sharedTracker].isDebug){
        NSLog (@"AppsFlyerDelegate onConversionDataRequestFailure = %@", errDesc);
    }
    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_CONVERSION_ERROR, [errDesc UTF8String]);
}

- (void) onAppOpenAttribution:(NSDictionary*) attributionData {
    NSString *attrData = [self getJsonStringFromDictionary:attributionData];
    if([AppsFlyerTracker sharedTracker].isDebug){
        NSLog (@"AppsFlyerDelegate onAppOpenAttribution = %@", attrData);
    }
    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_RETARGETTING, [attrData UTF8String]);
}

- (void) onAppOpenAttributionFailure:(NSError *)error{
    NSString *errDesc = [error localizedDescription];
    if([AppsFlyerTracker sharedTracker].isDebug){
        NSLog (@"AppsFlyerDelegate onAppOpenAttributionFailure = %@", errDesc);
    }
    UnitySendMessage(UNITY_SENDMESSAGE_CALLBACK_MANAGER, UNITY_SENDMESSAGE_CALLBACK_RETARGETTING_ERROR, [errDesc UTF8String]);
    
}

- (NSString *) getJsonStringFromDictionary:(NSDictionary *)dict {
    NSError *error;
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict
                                                       options:0
                                                         error:&error];
    
    if (!jsonData) {
        return nil;
    } else {
        
        NSString *JSONString = [[NSString alloc] initWithBytes:[jsonData bytes] length:[jsonData length] encoding:NSUTF8StringEncoding];
        if([AppsFlyerTracker sharedTracker].isDebug){
            NSLog(@"JSON OUTPUT: %@", JSONString);
        }
        return JSONString;
    }
}


@end

