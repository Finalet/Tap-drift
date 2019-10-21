//
//  ISN_NativeCore.m
//  Unity-iPhone
//
//  Created by lacost on 9/6/15.
//
//

#import <Foundation/Foundation.h>

#import "ISN_NativeCore.h"
#import "AppDelegateListener.h"
#if UNITY_VERSION < 450
#include "iPhone_View.h"
#endif



NSString * const UNITY_SPLITTER = @"|";
NSString * const UNITY_EOF = @"endofline";






@implementation ISN_DataConvertor


+(NSString *) charToNSString:(char *)value {
    if (value != NULL) {
        return [NSString stringWithUTF8String: value];
    } else {
        return [NSString stringWithUTF8String: ""];
    }
}

+(const char *)NSIntToChar:(NSInteger)value {
    NSString *tmp = [NSString stringWithFormat:@"%d", value];
    return [tmp UTF8String];
}

+ (const char *) NSStringToChar:(NSString *)value {
    return [value UTF8String];
}

+ (NSArray *)charToNSArray:(char *)value {
    NSString* strValue = [ISN_DataConvertor charToNSString:value];
    
    NSArray *array;
    if([strValue length] == 0) {
        array = [[NSArray alloc] init];
    } else {
        array = [strValue componentsSeparatedByString:@"|"];
    }
    
    return array;
}

+ (const char *) NSStringsArrayToChar:(NSArray *) array {
    return [ISN_DataConvertor NSStringToChar:[ISN_DataConvertor serializeNSStringsArray:array]];
}

+ (NSString *) serializeNSStringsArray:(NSArray *) array {
    
    NSMutableString * data = [[NSMutableString alloc] init];
    
    
    for(NSString* str in array) {
        [data appendString:str];
        [data appendString: UNITY_SPLITTER];
    }
    
    [data appendString: UNITY_EOF];
    
    NSString *str = [data copy];
#if UNITY_VERSION < 500
    [str autorelease];
#endif
    
    return str;
}


+ (NSString *)serializeErrorToNSString:(NSError *)error {
    NSString* description = @"";
    if(error.description != nil) {
        description = error.description;
    }
    return  [self serializeErrorWithDataToNSString:description code:error.code];
}

+ (NSString *)serializeErrorWithDataToNSString:(NSString *)description code:(int)code {
    NSMutableString * data = [[NSMutableString alloc] init];
    
    
    [data appendString: [NSString stringWithFormat:@"%d", code]];
    [data appendString: UNITY_SPLITTER];
    [data appendString: description];
    
    
    NSString *str = [data copy];
#if UNITY_VERSION < 500
    [str autorelease];
#endif
    
    return  str;
}



+ (const char *) serializeErrorWithData:(NSString *)description code: (int) code {
    NSString *str = [ISN_DataConvertor serializeErrorWithDataToNSString:description code:code];
    return [ISN_DataConvertor NSStringToChar:str];
}

+ (const char *) serializeError:(NSError *)error  {
    NSString *str = [ISN_DataConvertor serializeErrorToNSString:error];
    return [ISN_DataConvertor NSStringToChar:str];
}

@end














@implementation ISN_NativeUtility

static ISN_NativeUtility * na_sharedInstance;
static NSString* templateReviewURLIOS7  = @"itms-apps://itunes.apple.com/app/idAPP_ID";
NSString *templateReviewURL = @"itms-apps://ax.itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id=APP_ID";

+ (id)sharedInstance {
    
    if (na_sharedInstance == nil)  {
        na_sharedInstance = [[self alloc] init];
    }
    
    return na_sharedInstance;
}

+ (int) majorIOSVersion {
    NSArray *vComp = [[UIDevice currentDevice].systemVersion componentsSeparatedByString:@"."];
    return [[vComp objectAtIndex:0] intValue];
}

-(void) redirectToRatingPage:(NSString *)appId {
#if TARGET_IPHONE_SIMULATOR
    NSLog(@"NOTE: iTunes App Store is not supported on the iOS simulator. Unable to open App Store page.");
#else
    
    
    NSString *reviewURL;
    NSArray *vComp = [[UIDevice currentDevice].systemVersion componentsSeparatedByString:@"."];
    
    
    if ([[vComp objectAtIndex:0] intValue] >= 7) {
        reviewURL = [templateReviewURLIOS7 stringByReplacingOccurrencesOfString:@"APP_ID" withString:[NSString stringWithFormat:@"%@", appId]];
    }  else {
        reviewURL = [templateReviewURL stringByReplacingOccurrencesOfString:@"APP_ID" withString:[NSString stringWithFormat:@"%@", appId]];
    }
    
    NSLog(@"redirecting to iTunes page, iOS version: %i", [[vComp objectAtIndex:0] intValue]);
    NSLog(@"redirect URL: %@", reviewURL);
    
    
    
    [[UIApplication sharedApplication] openURL:[NSURL URLWithString:reviewURL]];
#endif
}


-(void) setApplicationBagesNumber:(int) count {
    [UIApplication sharedApplication].applicationIconBadgeNumber = count;
}



- (void) ShowSpinner {
    
    [[UIApplication sharedApplication] beginIgnoringInteractionEvents];
    
    if([self spinner] != nil) {
        return;
    }
    
    UIViewController *vc =  UnityGetGLViewController();
    
    
    [self setSpinner:[[UIActivityIndicatorView alloc] initWithActivityIndicatorStyle:UIActivityIndicatorViewStyleWhiteLarge]];
    
    
    [[UIDevice currentDevice] beginGeneratingDeviceOrientationNotifications];
    
    NSArray *vComp = [[UIDevice currentDevice].systemVersion componentsSeparatedByString:@"."];
    if ([[vComp objectAtIndex:0] intValue] >= 8) {
        NSLog(@"iOS 8 detected");
        [[self spinner] setFrame:CGRectMake(0,0, vc.view.frame.size.width, vc.view.frame.size.height)];
    } else {
        if([[UIDevice currentDevice] orientation] == UIDeviceOrientationPortrait || [[UIDevice currentDevice] orientation] == UIDeviceOrientationPortraitUpsideDown) {
            NSLog(@"portrait detected");
            [[self spinner] setFrame:CGRectMake(0,0, vc.view.frame.size.width, vc.view.frame.size.height)];
            
        } else {
            NSLog(@"landscape detected");
            [[self spinner] setFrame:CGRectMake(0,0, vc.view.frame.size.height, vc.view.frame.size.width)];
        }
        
    }
    
    
    
    
    
    [self spinner].opaque = NO;
    [self spinner].backgroundColor = [UIColor colorWithWhite:0.0f alpha:0.0f];
    
    
    [UIView animateWithDuration:0.8 animations:^{
        [self spinner].backgroundColor = [UIColor colorWithWhite:0.0f alpha:0.5f];
    }];
    
    
    
    
    [vc.view addSubview:[self spinner]];
    [[self spinner] startAnimating];
    
    //  [[self spinner] retain];
    
}



- (void) HideSpinner {
    
    if([self spinner] != nil) {
        [[UIApplication sharedApplication] endIgnoringInteractionEvents];
        
        [self spinner].backgroundColor = [UIColor colorWithWhite:0.0f alpha:0.5f];
        [UIView animateWithDuration:0.8 animations:^{
            [self spinner].backgroundColor = [UIColor colorWithWhite:0.0f alpha:0.0f];
            
        } completion:^(BOOL finished) {
            [[self spinner] removeFromSuperview];
#if UNITY_VERSION < 500
            [[self spinner] release];
#endif
            
            [self setSpinner:nil];
        }];
        
        
    }
    
}

- (CGFloat) GetW {
    
    UIViewController *vc =  UnityGetGLViewController();
    
    bool IsLandscape;
    UIInterfaceOrientation orientation = [UIApplication sharedApplication].statusBarOrientation;
    if(orientation == UIInterfaceOrientationLandscapeLeft || orientation == UIInterfaceOrientationLandscapeRight) {
        IsLandscape = true;
    } else {
        IsLandscape = false;
    }
    
    CGFloat w;
    if(IsLandscape) {
        w = vc.view.frame.size.height;
    } else {
        w = vc.view.frame.size.width;
    }
    
    
    NSArray *vComp = [[UIDevice currentDevice].systemVersion componentsSeparatedByString:@"."];
    if ([[vComp objectAtIndex:0] intValue] >= 8) {
        w = vc.view.frame.size.width;
    }
    
    
    return w;
}


- (void)DP_changeDate:(UIDatePicker *)sender {
    
    NSDateFormatter *dateFormatter = [[NSDateFormatter alloc] init];
#if UNITY_VERSION < 500
    [dateFormatter autorelease];
#endif
    
    [dateFormatter setDateFormat: @"yyyy-MM-dd HH:mm:ss"];
    NSString *dateString = [dateFormatter stringFromDate:sender.date];
    
    NSLog(@"DateChangedEvent: %@", dateString);
    
    UnitySendMessage("IOSDateTimePicker", "DateChangedEvent", [ISN_DataConvertor NSStringToChar:dateString]);
}

-(void) DP_PickerClosed:(UIDatePicker *)sender {
    NSDateFormatter *dateFormatter = [[NSDateFormatter alloc] init];
#if UNITY_VERSION < 500
    [dateFormatter autorelease];
#endif
    [dateFormatter setDateFormat: @"yyyy-MM-dd HH:mm:ss"];
    NSString *dateString = [dateFormatter stringFromDate:sender.date];
    
    NSLog(@"DateChangedEvent: %@", dateString);
    
    UnitySendMessage("IOSDateTimePicker", "PickerClosed", [ISN_DataConvertor NSStringToChar:dateString]);
    
}



UIDatePicker *datePicker;

- (void) DP_show:(int)mode {
    UIViewController *vc =  UnityGetGLViewController();
    
    
    
    
    
    
    CGRect toolbarTargetFrame = CGRectMake(0, vc.view.bounds.size.height-216-44, [self GetW], 44);
    CGRect datePickerTargetFrame = CGRectMake(0, vc.view.bounds.size.height-216, [self GetW], 216);
    CGRect darkViewTargetFrame = CGRectMake(0, vc.view.bounds.size.height-216-44, [self GetW], 260);
    
    UIView *darkView = [[UIView alloc] initWithFrame:CGRectMake(0, vc.view.bounds.size.height, [self GetW], 260)];
    darkView.alpha = 1;
    darkView.backgroundColor = [UIColor whiteColor];
    darkView.tag = 9;
    
    UITapGestureRecognizer *tapGesture = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(DP_dismissDatePicker:)];
    [darkView addGestureRecognizer:tapGesture];
    [vc.view addSubview:darkView];
    
    
    datePicker = [[UIDatePicker alloc] initWithFrame:CGRectMake(0, vc.view.bounds.size.height+44, [self GetW], 216)];
    datePicker.tag = 10;
    
    
#if UNITY_VERSION < 500
    [darkView autorelease];
    [tapGesture autorelease];
    [datePicker autorelease];
#endif
    
    
    [datePicker addTarget:self action:@selector(DP_changeDate:) forControlEvents:UIControlEventValueChanged];
    switch (mode) {
        case 1:
            datePicker.datePickerMode = UIDatePickerModeTime;
            break;
            
        case 2:
            datePicker.datePickerMode = UIDatePickerModeDate;
            break;
            
        case 3:
            datePicker.datePickerMode = UIDatePickerModeDateAndTime;
            break;
            
        case 4:
            datePicker.datePickerMode = UIDatePickerModeCountDownTimer;
            break;
            
        default:
            break;
    }
    
    // NSLog(@"dtp mode: %ld", (long)datePicker.datePickerMode);
    
    
    [vc.view addSubview:datePicker];
    
    UIToolbar *toolBar = [[UIToolbar alloc] initWithFrame:CGRectMake(0, vc.view.bounds.size.height, [self GetW], 44)];
    
    toolBar.tag = 11;
    toolBar.barStyle = UIBarStyleDefault;
    UIBarButtonItem *spacer = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil];
    UIBarButtonItem *doneButton = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemDone target:self action:@selector(DP_dismissDatePicker:)];
    
#if UNITY_VERSION < 500
    [toolBar autorelease];
    [spacer autorelease];
    [doneButton autorelease];
#endif
    
    [toolBar setItems:[NSArray arrayWithObjects:spacer, doneButton, nil]];
    [vc.view addSubview:toolBar];
    
    [UIView beginAnimations:@"MoveIn" context:nil];
    toolBar.frame = toolbarTargetFrame;
    datePicker.frame = datePickerTargetFrame;
    darkView.frame = darkViewTargetFrame;
    
    [UIView commitAnimations];
    
}

- (void)DP_removeViews:(id)object {
    UIViewController *vc =  UnityGetGLViewController();
    
    [[vc.view viewWithTag:9] removeFromSuperview];
    [[vc.view viewWithTag:10] removeFromSuperview];
    [[vc.view viewWithTag:11] removeFromSuperview];
}

- (void)DP_dismissDatePicker:(id)sender {
    UIViewController *vc =  UnityGetGLViewController();
    
    [self DP_PickerClosed:datePicker];
    
    CGRect toolbarTargetFrame = CGRectMake(0, vc.view.bounds.size.height, [self GetW], 44);
    CGRect datePickerTargetFrame = CGRectMake(0, vc.view.bounds.size.height+44, [self GetW], 216);
    CGRect darkViewTargetFrame = CGRectMake(0, vc.view.bounds.size.height, [self GetW], 260);
    
    
    [UIView beginAnimations:@"MoveOut" context:nil];
    [vc.view viewWithTag:9].frame = darkViewTargetFrame;
    [vc.view viewWithTag:10].frame = datePickerTargetFrame;
    [vc.view viewWithTag:11].frame = toolbarTargetFrame;
    [UIView setAnimationDelegate:self];
    [UIView setAnimationDidStopSelector:@selector(DP_removeViews:)];
    [UIView commitAnimations];
}


- (void) GetIFA {
    
#if UNITY_VERSION < 500
    NSString* ifa = [[[NSClassFromString(@"ASIdentifierManager") sharedManager] advertisingIdentifier] UUIDString];
    ifa = [[ifa stringByReplacingOccurrencesOfString:@"-" withString:@""] lowercaseString];
    NSLog(@"IFA: %@",ifa);
    UnitySendMessage("IOSSharedApplication", "OnAdvertisingIdentifierLoaded", [ISN_DataConvertor NSStringToChar:ifa]);
#endif
    
    
    UnitySendMessage("IOSSharedApplication", "OnAdvertisingIdentifierLoaded", [ISN_DataConvertor NSStringToChar:@""]);
    
    
}


@end








@implementation CloudManager
static CloudManager * cm_sharedInstance;


+ (id)sharedInstance {
    
    if (cm_sharedInstance == nil)  {
        cm_sharedInstance = [[self alloc] init];
    }
    
    return cm_sharedInstance;
}


-(void) initialize {
    
    [[NSNotificationCenter defaultCenter]
     addObserver: self
     selector: @selector (iCloudAccountAvailabilityChanged:)
     name: NSUbiquityIdentityDidChangeNotification
     object: nil];
    
    
    
    NSFileManager*  fileManager = [NSFileManager defaultManager];
    id currentToken = [fileManager ubiquityIdentityToken];
    bool isSignedIntoICloud = (currentToken!=nil);
    
    if(isSignedIntoICloud) {
        NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
        [[NSNotificationCenter defaultCenter] addObserver:self
                                                 selector:@selector(storeDidChange:)
                                                     name:NSUbiquitousKeyValueStoreDidChangeExternallyNotification
                                                   object:store];
        [store synchronize];
        
        UnitySendMessage("iCloudManager", "OnCloudInit", [ISN_DataConvertor NSStringToChar:@""]);
        
    } else {
        UnitySendMessage("iCloudManager", "OnCloudInitFail", [ISN_DataConvertor NSStringToChar:@""]);
    }
    
    /*
     
     NSURL *documentsDirectory = [[[NSFileManager defaultManager] URLsForDirectory:NSDocumentDirectory inDomains:NSUserDomainMask] lastObject];
     NSURL *storeURL = [documentsDirectory URLByAppendingPathComponent:@"CoreData.sqlite"];
     NSError *error = nil;
     NSPersistentStoreCoordinator *coordinator = [[NSPersistentStoreCoordinator alloc] initWithManagedObjectModel:<# your managed object model #>];
     NSDictionary *storeOptions =
     @{NSPersistentStoreUbiquitousContentNameKey: @"MyAppCloudStore"};
     NSPersistentStore *store = [coordinator addPersistentStoreWithType:NSSQLiteStoreType
     configuration:nil
     URL:storeURL
     options:storeOptions
     error:&error];
     
     NSURL *finaliCloudURL = [store URL];
     */
    
    
    NSLog(@"initialize");
    
}

-(void)setString:(NSString *)val key:(NSString *)key {
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    [store setString:val forKey:key];
    
    [store synchronize];
}

-(void) setData:(NSData *)val key:(NSString *)key {
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    [store setData:val forKey:key];
    
    [store synchronize];
}

-(void) setDouble:(double)val key:(NSString *)key {
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    [store setDouble:val forKey:key];
    
    [store synchronize];
    
}


-(void) requestDataForKey:(NSString *)key {
    NSUbiquitousKeyValueStore *store = [NSUbiquitousKeyValueStore defaultStore];
    
    id data = [store objectForKey:key];
    
    
    
    NSMutableString * array = [[NSMutableString alloc] init];
    [array appendString:key];
    [array appendString:@"|"];
    
    
    NSString* stringData;
    
    if(data != nil) {
        if([data isKindOfClass:[NSString class]]) {
            stringData = (NSString*) data;
        }
        
        if([data isKindOfClass:[NSData class]]) {
            
            NSData *b = (NSData*) data;
            
            NSMutableString *str = [[NSMutableString alloc] init];
            const char *db = (const char *) [b bytes];
            for (int i = 0; i < [b length]; i++) {
                if(i != 0) {
                    [str appendFormat:@","];
                }
                
                [str appendFormat:@"%i", (unsigned char)db[i]];
            }
            
            stringData = str;
            
        }
        
        if([data isKindOfClass:[NSNumber class]]) {
            NSNumber* n = (NSNumber*) data;
            stringData = [n stringValue];
        }
        
    } else {
        stringData = @"null";
    }
    
    
    [array appendString:stringData];
    
    NSLog(@"data: %@", stringData);
    
    
    NSString *package = [array copy];
#if UNITY_VERSION < 500
    [package autorelease];
#endif
    
    if(data == nil) {
        UnitySendMessage("iCloudManager", "OnCloudDataEmpty", [ISN_DataConvertor NSStringToChar:package]);
    } else {
        UnitySendMessage("iCloudManager", "OnCloudData", [ISN_DataConvertor NSStringToChar:package]);
        
    }
    
    
    
}



- (void)storeDidChange:(NSNotification *)notification {
    UnitySendMessage("iCloudManager", "OnCloudDataChanged", [ISN_DataConvertor NSStringToChar:@""]);
}

-(void) iCloudAccountAvailabilityChanged {
    NSLog(@"iCloudAccountAvailabilityChanged:");
}

@end








@implementation AppEventListener

static AppEventListener *ael_sharedInstance;


+ (id)sharedInstance {
    
    if (ael_sharedInstance == nil)  {
        ael_sharedInstance = [[self alloc] init];
    }
    
    return ael_sharedInstance;
}


- (void) subscribe {
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidBecomeActive:)   name:UIApplicationDidBecomeActiveNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationWillResignActive:) name:UIApplicationWillResignActiveNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidEnterBackground:) name:UIApplicationDidEnterBackgroundNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationWillTerminate:) name:UIApplicationWillTerminateNotification object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(applicationDidReceiveMemoryWarning:) name:UIApplicationDidReceiveMemoryWarningNotification object:nil];
    
}


+ (void) sendEvent: (NSString* ) event {
    UnitySendMessage("IOSNativeAppEvents", [ISN_DataConvertor NSStringToChar:event], [ISN_DataConvertor NSStringToChar:@"null"]);
}


- (void)applicationDidBecomeActive:(NSNotification *)notification {
    [AppEventListener sendEvent:@"applicationDidBecomeActive"];
}


- (void) applicationWillResignActive:(NSNotification *)notification {
    [AppEventListener sendEvent:@"applicationWillResignActive"];
}

- (void) applicationDidEnterBackground:(NSNotification *)notification {
    [AppEventListener sendEvent:@"applicationDidEnterBackground"];
}

- (void) applicationWillTerminate:(NSNotification *)notification {
    [AppEventListener sendEvent:@"applicationWillTerminate"];
}

- (void) applicationDidReceiveMemoryWarning:(NSNotification *)notification {
    [AppEventListener sendEvent:@"applicationDidReceiveMemoryWarning"];
}

@end






@implementation ISN_NativePopUpsManager

static UIAlertController* _currentAlert =  nil;


static ISN_NativePopUpsManager *_sharedInstance;

+ (id)sharedInstance {
    if (_sharedInstance == nil)  {
        _sharedInstance = [[self alloc] init];
    }
    
    return _sharedInstance;
}



+(void) dismissCurrentAlert {
    if(_currentAlert != nil) {
        [_currentAlert dismissViewControllerAnimated:true completion:^{
            UnitySendMessage("IOSPopUp", "onPopUpCallBack", [ISN_DataConvertor NSStringToChar:@"0"]);
            UnitySendMessage("IOSRateUsPopUp", "onPopUpCallBack", [ISN_DataConvertor NSStringToChar:@"0"]);
        }];
        
        
#if UNITY_VERSION < 500
        [_currentAlert release];
#endif
        _currentAlert = nil;
    }
}

+(void) showRateUsPopUp: (NSString *) title message: (NSString*) msg b1: (NSString*) b1 b2: (NSString*) b2 b3: (NSString*) b3 {
    
    UIAlertController* alert = [UIAlertController alertControllerWithTitle:title  message:msg  preferredStyle:UIAlertControllerStyleAlert];
    
    
    
    UIAlertAction* rateAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
        UnitySendMessage("IOSRateUsPopUp", "onPopUpCallBack", [ISN_DataConvertor NSStringToChar:@"0"]);
        _currentAlert = nil;
    }];
    
    
    UIAlertAction* laterAction = [UIAlertAction actionWithTitle:b2 style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
        UnitySendMessage("IOSRateUsPopUp", "onPopUpCallBack", [ISN_DataConvertor NSStringToChar:@"1"]);
        _currentAlert = nil;
    }];
    
    
    UIAlertAction* declineAction = [UIAlertAction actionWithTitle:b3 style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
        UnitySendMessage("IOSRateUsPopUp", "onPopUpCallBack", [ISN_DataConvertor NSStringToChar:@"2"]);
        _currentAlert = nil;
    }];
    
    
    [alert addAction:rateAction];
    [alert addAction:laterAction];
    [alert addAction:declineAction];
    
    _currentAlert = alert;
    
    
    UIViewController *vc =  UnityGetGLViewController();
    [vc presentViewController:alert animated:YES completion:nil];
    
    
    
}




+ (void) showDialog: (NSString *) title message: (NSString*) msg yesTitle:(NSString*) b1 noTitle: (NSString*) b2{
    
    UIAlertController* alert = [UIAlertController alertControllerWithTitle:title  message:msg  preferredStyle:UIAlertControllerStyleAlert];
    
    UIAlertAction* okAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
        UnitySendMessage("IOSPopUp", "onPopUpCallBack", [ISN_DataConvertor NSStringToChar:@"0"]);
        _currentAlert = nil;
    }];
    
    
    UIAlertAction* yesAction = [UIAlertAction actionWithTitle:b2 style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
        UnitySendMessage("IOSPopUp", "onPopUpCallBack", [ISN_DataConvertor NSStringToChar:@"1"]);
        _currentAlert = nil;
    }];
    
    [alert addAction:yesAction];
    [alert addAction:okAction];
    
    _currentAlert = alert;
    
    
    UIViewController *vc =  UnityGetGLViewController();
    [vc presentViewController:alert animated:YES completion:nil];
    
}


+(void) showMessage: (NSString *) title message: (NSString*) msg okTitle:(NSString*) b1 {
    
    
    UIAlertController* alert = [UIAlertController alertControllerWithTitle:title  message:msg  preferredStyle:UIAlertControllerStyleAlert];
    
    UIAlertAction* defaultAction = [UIAlertAction actionWithTitle:b1 style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
        UnitySendMessage("IOSPopUp", "onPopUpCallBack", [ISN_DataConvertor NSStringToChar:@"0"]);
        _currentAlert = nil;
    }];
    
    
    [alert addAction:defaultAction];
    _currentAlert = alert;
    
    
    UIViewController *vc =  UnityGetGLViewController();
    [vc presentViewController:alert animated:YES completion:nil];
    
}

//--------------------------------------
//  IOS 6,7 implementation
//--------------------------------------

static UIAlertView* _currentAllert =  nil;

+ (void) unregisterAllertView_old {
    if(_currentAllert != nil) {
#if UNITY_VERSION < 500
        [_currentAlert release];
#endif
        _currentAllert = nil;
    }
}

+(void) dismissCurrentAlert_old {
    if(_currentAllert != nil) {
        [_currentAllert dismissWithClickedButtonIndex:0 animated:YES];
#if UNITY_VERSION < 500
        [_currentAlert release];
#endif
        _currentAllert = nil;
    }
}

+(void) showRateUsPopUp_old: (NSString *) title message: (NSString*) msg b1: (NSString*) b1 b2: (NSString*) b2 b3: (NSString*) b3 {
    
    UIAlertView *alert = [[UIAlertView alloc] init];
    [alert setTitle:title];
    [alert setMessage:msg];
    [alert setDelegate: [ISN_NativePopUpsManager sharedInstance]];
    
    [alert addButtonWithTitle:b1];
    [alert addButtonWithTitle:b2];
    [alert addButtonWithTitle:b3];
    
    [alert show];
    
    _currentAllert = alert;
    
}




+ (void) showDialog_old: (NSString *) title message: (NSString*) msg yesTitle:(NSString*) b1 noTitle: (NSString*) b2{
    
    UIAlertView *alert = [[UIAlertView alloc] init];
    [alert setTitle:title];
    [alert setMessage:msg];
    [alert setDelegate: [ISN_NativePopUpsManager sharedInstance]];
    [alert addButtonWithTitle:b1];
    [alert addButtonWithTitle:b2];
    [alert show];
    
    _currentAllert = alert;
    
}


+(void) showMessage_old: (NSString *) title message: (NSString*) msg okTitle:(NSString*) b1 {
    
    UIAlertView *alert = [[UIAlertView alloc] init];
    [alert setTitle:title];
    [alert setMessage:msg];
    [alert setDelegate: [ISN_NativePopUpsManager sharedInstance]];
    [alert addButtonWithTitle:b1];
    [alert show];
    
    _currentAllert = alert;
}





- (void)alertView:(UIAlertView *)alertView clickedButtonAtIndex:(NSInteger)buttonIndex {
    [ISN_NativePopUpsManager unregisterAllertView_old];
    UnitySendMessage("IOSPopUp", "onPopUpCallBack",  [ISN_DataConvertor NSIntToChar:buttonIndex]);
    UnitySendMessage("IOSRateUsPopUp", "onPopUpCallBack",  [ISN_DataConvertor NSIntToChar:buttonIndex]);
}


@end












@implementation IOSNativeNotificationCenter


static IOSNativeNotificationCenter *sharedHelper = nil;

+ (IOSNativeNotificationCenter *) sharedInstance {
    if (!sharedHelper) {
        sharedHelper = [[IOSNativeNotificationCenter alloc] init];
        
        
    }
    return sharedHelper;
}

- (id)init {
    if ((self = [super init])) {
        NSLog(@"Subscibing...");
        NSNotificationCenter *notificationCenter = [NSNotificationCenter defaultCenter];
        [notificationCenter addObserver: self
                               selector: @selector (handle_NotificationEvent:)
                                   name: kUnityDidReceiveLocalNotification
                                 object: nil];
        
    }
    
    return self;
}

#pragma mark Music notification handlers


- (void) handle_NotificationEvent: (NSNotification *) receivedNotification {
    
    NSLog(@"ISN: handle_NotificationEvent");
    UILocalNotification* notification = (UILocalNotification*) receivedNotification.userInfo;
    
    
    NSMutableString * data = [[NSMutableString alloc] init];
    
    [data appendString:notification.alertBody];
    [data appendString:@"|"];
    [data appendString:[notification.userInfo objectForKey:@"AlarmKey"]];
    [data appendString:@"|"];
    [data appendString:[notification.userInfo objectForKey:@"data"]];
    [data appendString:@"|"];
    [data appendString: [NSString stringWithFormat:@"%d", notification.applicationIconBadgeNumber]];
    
    
    NSString *str = [data copy];
    
#if UNITY_VERSION < 500
    [str autorelease];
#endif
    
    
    UnitySendMessage("IOSNotificationController", "OnLocalNotificationReceived_Event", [ISN_DataConvertor NSStringToChar:str]);
    
}

- (void) RegisterForNotifications {
    
    NSArray *vComp = [[UIDevice currentDevice].systemVersion componentsSeparatedByString:@"."];
    if ([[vComp objectAtIndex:0] intValue] >= 8) {
        [[UIApplication sharedApplication] registerUserNotificationSettings:[UIUserNotificationSettings settingsForTypes:UIUserNotificationTypeAlert|UIUserNotificationTypeBadge|UIUserNotificationTypeSound categories:nil]];
        
    }
}

-(void) requestNotificationSettings {
    UIUserNotificationSettings* NotificationSettings = [[UIApplication sharedApplication] currentUserNotificationSettings];
    
    NSString *str = [NSString stringWithFormat:@"%d", NotificationSettings.types];
    
    UnitySendMessage("IOSNotificationController", "OnNotificationSettingsInfoRetrived", [ISN_DataConvertor NSStringToChar:str]);
    
    
}


-(void) scheduleNotification:(int)time message:(NSString *)message sound:(bool *)sound alarmID:(NSString *)alarmID badges:(int)badges notificationData:(NSString *)notificationData {
    
    
    
    NSArray *vComp = [[UIDevice currentDevice].systemVersion componentsSeparatedByString:@"."];
    if ([[vComp objectAtIndex:0] intValue] >= 8) {
        UIUserNotificationSettings* NotificationSettings = [[UIApplication sharedApplication] currentUserNotificationSettings];
        
        if((NotificationSettings.types & UIUserNotificationTypeAlert) == 0) {
            NSLog(@"ISN: user disabled local notification for this app, sending fail event.");
            
            NSMutableString * data = [[NSMutableString alloc] init];
            [data appendString: @"0" ];
            [data appendString:@"|"];
            [data appendString:  [NSString stringWithFormat:@"%u",[[UIApplication sharedApplication] currentUserNotificationSettings].types]];
            
            UnitySendMessage("IOSNotificationController", "OnNotificationScheduleResultAction", [ISN_DataConvertor NSStringToChar:data]);
            
            [self RegisterForNotifications];
            return;
        }
        
        if((NotificationSettings.types & UIUserNotificationTypeBadge) == 0) {
            
            if(badges > 0) {
                NSLog(@"ISN: no badges allowed for this user. Notification badge disabled.");
                badges = 0;
            }
            
            
        }
        
        if((NotificationSettings.types & UIUserNotificationTypeSound) == 0) {
            if(sound) {
                NSLog(@"ISN: no sound allowed for this user. Notification sound disabled.");
#if UNITY_VERSION < 500
                sound = false;
#endif
            }
            
        }
    }
    
    
    UILocalNotification* localNotification = [[UILocalNotification alloc] init];
    localNotification.fireDate = [NSDate dateWithTimeIntervalSinceNow:time];
    localNotification.alertBody = message;
    localNotification.timeZone = [NSTimeZone defaultTimeZone];
    
    
    
    if (badges > 0)
        localNotification.applicationIconBadgeNumber = badges;
    
    if(sound) {
        localNotification.soundName = UILocalNotificationDefaultSoundName;
    }
    
    
    
    NSMutableDictionary *userInfo = [NSMutableDictionary dictionary];
    [userInfo setObject:alarmID forKey:@"AlarmKey"];
    [userInfo setObject:notificationData forKey:@"data"];
    
    // Set some extra info to your alarm
    localNotification.userInfo = userInfo;
    
    NSLog(@"ISN: scheduleNotification AlarmKey: %@", alarmID);
    
    [[UIApplication sharedApplication] scheduleLocalNotification:localNotification];
    
    
    NSMutableString * data = [[NSMutableString alloc] init];
    [data appendString: @"1" ];
    [data appendString:@"|"];
    
    if ([[vComp objectAtIndex:0] intValue] >= 8) {
        [data appendString:  [NSString stringWithFormat:@"%u",[[UIApplication sharedApplication] currentUserNotificationSettings].types]];
    } else {
        [data appendString:@"7"];
    }
    
    
    UnitySendMessage("IOSNotificationController", "OnNotificationScheduleResultAction", [ISN_DataConvertor NSStringToChar:data]);
    
    
}

- (UILocalNotification *)existingNotificationWithAlarmID:(NSString *)alarmID {
    for (UILocalNotification *notification in [[UIApplication sharedApplication] scheduledLocalNotifications]) {
        if ([[notification.userInfo objectForKey:@"AlarmKey"] isEqualToString:alarmID]) {
            return notification;
        }
    }
    
    return nil;
}

- (void)cleanUpLocalNotificationWithAlarmID:(NSString *)alarmID {
    NSLog(@"cleanUpLocalNotificationWithAlarmID AlarmKey: %@", alarmID);
    
    UILocalNotification *notification = [self existingNotificationWithAlarmID:alarmID];
    if (notification) {
        NSLog(@"notification canceled");
        [[UIApplication sharedApplication] cancelLocalNotification:notification];
    }
}






- (void) cancelNotifications {
    [[UIApplication sharedApplication] cancelAllLocalNotifications];
}

- (void) applicationIconBadgeNumber:(int) badges {
    [UIApplication sharedApplication].applicationIconBadgeNumber = badges;
}


@end


@implementation ISNSharedApplication

static ISNSharedApplication *sha_sharedInstance;


+ (id)sharedInstance {
    
    if (sha_sharedInstance == nil)  {
        sha_sharedInstance = [[self alloc] init];
    }
    
    return sha_sharedInstance;
}

- (void) checkUrl:(NSString *)url {
    NSURL *uri = [NSURL URLWithString:url];
    BOOL canOpenURL = [[UIApplication sharedApplication] canOpenURL:uri];
    
    if(canOpenURL) {
        UnitySendMessage("IOSSharedApplication", "UrlCheckSuccess", [ISN_DataConvertor NSStringToChar:url]);
    } else {
        UnitySendMessage("IOSSharedApplication", "UrlCheckFailed", [ISN_DataConvertor NSStringToChar:url]);
    }
    
}

-(void) openUrl:(NSString *)url {
    [[UIApplication sharedApplication] openURL:[NSURL URLWithString:url]];
}


@end


extern "C" {
    
    
    //--------------------------------------
    //  Date Time Picker
    //--------------------------------------
    
    void _ISN_ShowDP(int mode) {
        [[ISN_NativeUtility sharedInstance] DP_show:mode];
    }
    
    
    
    
    //--------------------------------------
    //  IOS Native Utility
    //--------------------------------------
    
    void _ISN_SetApplicationBagesNumber(int count) {
        [[ISN_NativeUtility sharedInstance] setApplicationBagesNumber:count];
    }
    
    
    void _ISN_RedirectToAppStoreRatingPage(char* appId) {
        [[ISN_NativeUtility sharedInstance] redirectToRatingPage: [ISN_DataConvertor charToNSString:appId ]];
    }
    
    
    void _ISN_ShowPreloader() {
        [[ISN_NativeUtility sharedInstance] ShowSpinner];
    }
    
    
    void _ISN_HidePreloader() {
        [[ISN_NativeUtility sharedInstance] HideSpinner];
    }
    
    void _ISN_GetIFA() {
        [[ISN_NativeUtility sharedInstance] GetIFA];
    }
    
    
    //--------------------------------------
    //  IOS Native Utility PopUps Plugin Section
    //--------------------------------------
    
    
    void _MNP_RedirectToAppStoreRatingPage(char* appId) {
        _ISN_RedirectToAppStoreRatingPage(appId);
    }
    
    
    void _MNP_ShowPreloader() {
        _ISN_ShowPreloader();
    }
    
    
    void _MNP_HidePreloader() {
        _ISN_HidePreloader();
    }
    
    
    //--------------------------------------
    //  IOS Native iCloud Section
    //--------------------------------------

    
    
    
    void _initCloud ()  {
        [[CloudManager sharedInstance] initialize];
    }
    
    void _setString(char* key, char* val) {
        NSString* k = [ISN_DataConvertor charToNSString:key];
        NSString* v = [ISN_DataConvertor charToNSString:val];
        
        [[CloudManager sharedInstance] setString:v key:k];
    }
    
    
    void _setDouble(char* key, float val) {
        NSString* k = [ISN_DataConvertor charToNSString:key];
        double v = (double) val;
        
        [[CloudManager sharedInstance] setDouble:v key:k];
    }
    
    void _setData(char* key, char* val) {
        NSString* k = [ISN_DataConvertor charToNSString:key];
        NSString* v = [ISN_DataConvertor charToNSString:val];
        
        NSArray *bytes = [v componentsSeparatedByString:@","];
        
        
        NSMutableData* d = [[NSMutableData alloc] init];
        for(NSString* s in bytes) {
            int v = [s intValue];
            char * c = (char*)(&v);
            [d appendBytes:c length:1];
            
        }
        
        [[CloudManager sharedInstance] setData:d key:k];
        
    }
    
    
    void _requestDataForKey(char* key) {
        NSString* k = [ISN_DataConvertor charToNSString:key];
        [[CloudManager sharedInstance] requestDataForKey:k];
    }
    
    
    
    
    //--------------------------------------
    //  IOS Native App Event Listner
    //--------------------------------------

    
    void _ISNsubscribe ()  {
        [[AppEventListener sharedInstance] subscribe];
    }
    
    
    
    //--------------------------------------
    //  IOS Native Shared App API Section
    //--------------------------------------
    
    
    
    void _ISN_checkUrl(char* url) {
        NSString *uri = [ISN_DataConvertor charToNSString:url];
        [[ISNSharedApplication sharedInstance] checkUrl:uri];
    }
    
    void _ISN_openUrl(char* url) {
        NSString *uri = [ISN_DataConvertor charToNSString:url];
        [[ISNSharedApplication sharedInstance] openUrl:uri];
    }
    
    
    
    
    //--------------------------------------
    //  IOS Native PopUps API Section
    //--------------------------------------
    
    void _ISN_ShowRateUsPopUp(char* title, char* message, char* b1, char* b2, char* b3) {
        
        if([ISN_NativeUtility majorIOSVersion] >= 8) {
            [ISN_NativePopUpsManager showRateUsPopUp:[ISN_DataConvertor charToNSString:title] message:[ISN_DataConvertor charToNSString:message] b1:[ISN_DataConvertor charToNSString:b1] b2:[ISN_DataConvertor charToNSString:b2] b3:[ISN_DataConvertor charToNSString:b3]];
        } else {
            [ISN_NativePopUpsManager showRateUsPopUp_old:[ISN_DataConvertor charToNSString:title] message:[ISN_DataConvertor charToNSString:message] b1:[ISN_DataConvertor charToNSString:b1] b2:[ISN_DataConvertor charToNSString:b2] b3:[ISN_DataConvertor charToNSString:b3]];
        }
    }
    
    
    
    void _ISN_ShowDialog(char* title, char* message, char* yes, char* no) {
        if([ISN_NativeUtility majorIOSVersion] >= 8) {
            [ISN_NativePopUpsManager showDialog:[ISN_DataConvertor charToNSString:title] message:[ISN_DataConvertor charToNSString:message] yesTitle:[ISN_DataConvertor charToNSString:yes] noTitle:[ISN_DataConvertor charToNSString:no]];
        } else {
            [ISN_NativePopUpsManager showDialog_old:[ISN_DataConvertor charToNSString:title] message:[ISN_DataConvertor charToNSString:message] yesTitle:[ISN_DataConvertor charToNSString:yes] noTitle:[ISN_DataConvertor charToNSString:no]];
        }
        
    }
    
    void _ISN_ShowMessage(char* title, char* message, char* ok) {
        if([ISN_NativeUtility majorIOSVersion] >= 8) {
            [ISN_NativePopUpsManager showMessage:[ISN_DataConvertor charToNSString:title] message:[ISN_DataConvertor charToNSString:message] okTitle:[ISN_DataConvertor charToNSString:ok]];
        } else {
            [ISN_NativePopUpsManager showMessage_old:[ISN_DataConvertor charToNSString:title] message:[ISN_DataConvertor charToNSString:message] okTitle:[ISN_DataConvertor charToNSString:ok]];
        }
    }
    
    
    
    void _ISN_DismissCurrentAlert() {
        if([ISN_NativeUtility majorIOSVersion] >= 8) {
            [ISN_NativePopUpsManager dismissCurrentAlert];
        } else {
            [ISN_NativePopUpsManager dismissCurrentAlert_old];
        }
        
    }
    
    
    //--------------------------------------
    //  Native PopUps API PopUps Plugin Section
    //--------------------------------------
    
    void _MNP_ShowRateUsPopUp(char* title, char* message, char* b1, char* b2, char* b3) {
        _ISN_ShowRateUsPopUp(title, message, b1, b2, b3);
    }
    
    
    void _MNP_ShowDialog(char* title, char* message, char* yes, char* no) {
        _ISN_ShowDialog(title, message, yes, no);
    }
    
    void _MNP_ShowMessage(char* title, char* message, char* ok) {
        _ISN_ShowMessage(title, message, ok);
    }
    
    void _MNP_DismissCurrentAlert() {
        _ISN_DismissCurrentAlert();
    }
    
    
    //--------------------------------------
    //  IOS Native Notifications API Section
    //--------------------------------------


    
    void _ISN_CancelNotifications() {
        [[IOSNativeNotificationCenter sharedInstance] cancelNotifications];
    }
    
    
    void _ISN_CancelNotificationById(char* nId) {
        NSString* alarmID = [ISN_DataConvertor charToNSString:nId];
        [[IOSNativeNotificationCenter sharedInstance] cleanUpLocalNotificationWithAlarmID:alarmID];
    }
    
    void  _ISN_RequestNotificationPermissions ()  {
        [[IOSNativeNotificationCenter sharedInstance] RegisterForNotifications];
    }
    
    
    void  _ISN_ScheduleNotification (int time, char* message, bool* sound, char* nId, int badges, char* data)  {
        NSString* alarmID = [ISN_DataConvertor charToNSString:nId];
        [[IOSNativeNotificationCenter sharedInstance] scheduleNotification:time message:[ISN_DataConvertor charToNSString:message] sound:sound alarmID:alarmID badges:badges notificationData :[ISN_DataConvertor charToNSString:data]];
    }
    
    
    
    void _ISN_ApplicationIconBadgeNumber (int badges) {
        [[IOSNativeNotificationCenter sharedInstance] applicationIconBadgeNumber:badges];
    }
    void _ISN_RequestNotificationSettings () {
        [[IOSNativeNotificationCenter sharedInstance] requestNotificationSettings];
    }
    
    
    
    void _ISN_RegisterForRemoteNotifications(int types) {
        NSLog(@"_ISN_RegisterForRemoteNotifications");
        
        UIUserNotificationSettings *settings = [UIUserNotificationSettings settingsForTypes:UIUserNotificationTypeAlert |  UIUserNotificationTypeBadge | UIUserNotificationTypeSound categories:nil];
        [[UIApplication sharedApplication] registerUserNotificationSettings:settings];
        [[UIApplication sharedApplication] registerForRemoteNotifications];
        
        
    }


    
}






