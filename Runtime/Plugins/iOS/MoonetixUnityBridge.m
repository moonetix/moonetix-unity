// Unity side of the Moonetix offerwall plugin: C functions called from C# (IosNative.cs) that wrap the
// Moonetix iOS SDK. The SDK framework is added to the Xcode project by MoonetixIosBuild.cs.
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <MoonetixOfferwall/MoonetixOfferwall-Swift.h>

typedef void (*MoonetixUnityEventCallback)(int type, const char *code, const char *message);

static MoonetixUnityEventCallback moonetixUnityCallback = NULL;

static void MoonetixUnitySend(int type, NSString *code, NSString *message)
{
    if (moonetixUnityCallback != NULL) {
        moonetixUnityCallback(type, (code ?: @"").UTF8String, (message ?: @"").UTF8String);
    }
}

@interface MoonetixUnityDelegate : NSObject <MoonetixDelegate>
@end

@implementation MoonetixUnityDelegate

- (void)moonetixDidOpen
{
    MoonetixUnitySend(1, nil, nil);
}

- (void)moonetixDidClose
{
    MoonetixUnitySend(2, nil, nil);
}

- (void)moonetixDidFailWithError:(MoonetixError *)error
{
    MoonetixUnitySend(3, error.codeName, error.message);
}

@end

// Moonetix.delegate is weak, so the plugin keeps its delegate alive here.
static MoonetixUnityDelegate *moonetixUnityDelegate = nil;

static NSString *MoonetixUnityString(const char *value)
{
    if (value == NULL) {
        return @"";
    }
    NSString *s = [NSString stringWithUTF8String:value];
    return s ?: @"";
}

static void MoonetixUnityAttach(void)
{
    if (moonetixUnityDelegate == nil) {
        moonetixUnityDelegate = [[MoonetixUnityDelegate alloc] init];
    }
    Moonetix.delegate = moonetixUnityDelegate;
}

void MoonetixUnity_SetCallback(MoonetixUnityEventCallback callback)
{
    moonetixUnityCallback = callback;
}

void MoonetixUnity_Init(const char *appId)
{
    NSString *value = MoonetixUnityString(appId);
    dispatch_block_t work = ^{
        MoonetixUnityAttach();
        [Moonetix configureWithAppId:value];
    };
    if ([NSThread isMainThread]) {
        work();
    } else {
        dispatch_async(dispatch_get_main_queue(), work);
    }
}

void MoonetixUnity_Open(const char *token, const char *title, int hasColor, int argb, int showLogo)
{
    NSString *tokenValue = MoonetixUnityString(token);
    NSString *titleValue = MoonetixUnityString(title);
    dispatch_block_t work = ^{
        MoonetixUnityAttach();
        UIColor *color = nil;
        if (hasColor) {
            uint32_t v = (uint32_t)argb;
            color = [UIColor colorWithRed:((v >> 16) & 0xFF) / 255.0 green:((v >> 8) & 0xFF) / 255.0 blue:(v & 0xFF) / 255.0 alpha:1.0];
        }
        NSString *cleanTitle = [titleValue stringByTrimmingCharactersInSet:[NSCharacterSet whitespaceAndNewlineCharacterSet]].length > 0 ? titleValue : nil;
        MoonetixOptions *options = [[MoonetixOptions alloc] initWithTitle:cleanTitle toolbarColor:color showLogo:(showLogo != 0) presentation:MoonetixPresentationFullScreen];
        [Moonetix openWithToken:tokenValue from:nil options:options];
#if !__has_feature(objc_arc)
        [options release];
#endif
    };
    if ([NSThread isMainThread]) {
        work();
    } else {
        dispatch_async(dispatch_get_main_queue(), work);
    }
}
