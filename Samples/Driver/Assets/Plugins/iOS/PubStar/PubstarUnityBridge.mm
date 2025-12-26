#import "PSAdPreset.h"
#import "UnityFramework/UnityFramework-Swift.h"
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

extern void UnitySendMessage(const char *obj, const char *method,
                             const char *msg);

static PubstarImpl *moduleImpl = nil;
static NSString *sUnityGameObjectName = nil;

static PubstarImpl *GetModuleImpl(void) {
    if (!moduleImpl) {
        moduleImpl = [PubstarImpl new];
    }
    return moduleImpl;
}

static void PubstarUnitySendMessage(NSString *methodName, NSString *message) {
    if (!sUnityGameObjectName || !methodName) {
        return;
    }

    const char *obj = sUnityGameObjectName.UTF8String;
    const char *method = methodName.UTF8String;
    const char *msg = message ? message.UTF8String : "";

    UnitySendMessage(obj, method, msg);
}

static NSMutableDictionary<NSString *, UIView *> *sAdContainers = nil;
static UIView *PubstarGetRootView(void) {
    UIWindow *keyWindow = [UIApplication sharedApplication].keyWindow;
    if (keyWindow) {
        return keyWindow.rootViewController.view;
    }

    for (UIWindow *window in [UIApplication sharedApplication].windows) {
        if (window.rootViewController) {
            return window.rootViewController.view;
        }
    }
    return nil;
}

static NSMutableDictionary<NSString *, UIView *> *PubstarGetAdContainers(void) {
    if (!sAdContainers) {
        sAdContainers = [NSMutableDictionary new];
    }
    return sAdContainers;
}

extern "C" {

void pubstar_initialize(const char *gameObjectName) {
    @autoreleasepool {
        NSLog(@"[PubstarNative] initialize called");
    }

    PubstarImpl *impl = GetModuleImpl();
    if (!impl) {
        return;
    }

    NSString *goName = nil;
    if (gameObjectName != NULL) {
        goName = [NSString stringWithUTF8String:gameObjectName];
    }

    sUnityGameObjectName = goName;

    __block BOOL isCalled = NO;

    [impl
        initializationOnDone:^{
          if (isCalled)
              return;
          isCalled = YES;

          NSLog(@"[PubstarNative] initialization was successfully called");
          PubstarUnitySendMessage(@"OnPubstarInitDone", @"");
        }
        onError:^(NSInteger errorCode) {
          if (isCalled)
              return;
          isCalled = YES;

          NSString *msg = [NSString stringWithFormat:@"%ld", (long)errorCode];
          NSLog(@"[PubstarNative] initialization was called with failure, %@",
                msg);
          PubstarUnitySendMessage(@"OnPubstarInitError", msg);
        }];
}

void pubstar_load(const char *placementId) {
    @autoreleasepool {
        NSString *nsPlacement =
            placementId ? [NSString stringWithUTF8String:placementId]
                        : @"(null)";
        NSLog(@"[PubstarNativeFake] loadAd called with placementId = %@",
              nsPlacement);
    }
}

void pubstar_show(const char *placementId) {
    @autoreleasepool {
        NSString *nsPlacement =
            placementId ? [NSString stringWithUTF8String:placementId]
                        : @"(null)";
        NSLog(@"[PubstarNativeFake] showAd called with placementId = %@",
              nsPlacement);

        dispatch_after(
            dispatch_time(DISPATCH_TIME_NOW, (int64_t)(2 * NSEC_PER_SEC)),
            dispatch_get_main_queue(), ^{
              NSLog(@"[PubstarNativeFake] pretend ad finished showing for %@",
                    nsPlacement);
            });
    }
}

void pubstar_load_and_show(const char *placementId) {
    if (!placementId)
        return;
    NSString *adId = [NSString stringWithUTF8String:placementId];

    PubstarImpl *impl = GetModuleImpl();
    if (!impl)
        return;

    [impl loadAndShowWithAdId:adId
        onLoadedError:^(NSInteger errorCode) {
          NSString *msg = [NSString stringWithFormat:@"%ld", (long)errorCode];
          NSLog(@"[PubstarNative] pubstar_load_and_show onLoadedError called "
                @"mesage %s",
                msg.UTF8String);

          PubstarUnitySendMessage(@"OnPubstarLoadError", msg);
        }
        onLoaded:^{
          NSLog(@"[PubstarNative] pubstar_load_and_show onLoaded called");

          PubstarUnitySendMessage(@"OnPubstarLoaded", adId);
        }
        onHide:^(NSDictionary<NSString *, id> *_Nullable payload) {
          NSLog(@"[PubstarNative] pubstar_load_and_show onHide called");

          NSString *payloadString = @"";
          if (payload && [NSJSONSerialization isValidJSONObject:payload]) {
              NSError *error = nil;
              NSData *jsonData =
                  [NSJSONSerialization dataWithJSONObject:payload
                                                  options:0
                                                    error:&error];
              if (!error && jsonData) {
                  payloadString =
                      [[NSString alloc] initWithData:jsonData
                                            encoding:NSUTF8StringEncoding];
              }
          }

          PubstarUnitySendMessage(@"OnPubstarAdHidden", payloadString);
        }
        onShowed:^{
          NSLog(@"[PubstarNative] pubstar_load_and_show onShowed called");

          PubstarUnitySendMessage(@"OnPubstarAdShowed", adId);
        }
        onShowedError:^(NSInteger errorCode) {
          NSString *msg = [NSString stringWithFormat:@"%ld", (long)errorCode];
          NSLog(@"[PubstarNative] pubstar_load_and_show onShowedError called "
                @"message %s",
                msg.UTF8String);

          PubstarUnitySendMessage(@"OnPubstarShowError", msg);
        }];
}

static CGRect PubstarCalculateAdFrame(CGRect bounds, float xDp, float yDp,
                                      float widthDp, float heightDp,
                                      NSString *positionString) {
    CGFloat screenWidth = bounds.size.width;
    CGFloat screeHeight = bounds.size.height;

    CGFloat width = widthDp;
    if (widthDp == -1.0f) {
        width = screenWidth;
    } else if (widthDp > 0.0f && widthDp < 1.0f) {
        width = screenWidth * widthDp;
    }

    CGFloat height = heightDp;
    if (heightDp == -1.0f) {
        height = screeHeight;
    } else if (heightDp > 0.0f && heightDp < 1.0f) {
        height = screeHeight * heightDp;
    }

    CGFloat x = xDp;
    CGFloat y = yDp;

    if ([positionString isEqualToString:PSAdPresetBottom]) {
        y = screeHeight - height - yDp;

    } else if ([positionString isEqualToString:PSAdPresetTop]) {
        y = yDp;

    } else if ([positionString isEqualToString:PSAdPresetCenter]) {
        x = (screenWidth - width) / 2.0 + xDp;
        y = (screeHeight - height) / 2.0 + yDp;

    } else if ([positionString isEqualToString:PSAdPresetTopLeft]) {
        x = xDp;
        y = yDp;

    } else if ([positionString isEqualToString:PSAdPresetTopRight]) {
        x = screenWidth - width - xDp;
        y = yDp;

    } else if ([positionString isEqualToString:PSAdPresetBottomLeft]) {
        x = xDp;
        y = screeHeight - height - yDp;

    } else if ([positionString isEqualToString:PSAdPresetBottomRight]) {
        x = screenWidth - width - xDp;
        y = screeHeight - height - yDp;
    }

    return CGRectMake(x, y, width, height);
}

void pubstar_create_ad_view(const char *viewId, float xDp, float yDp,
                            float widthDp, float heightDp,
                            const char *position) {
    if (!viewId)
        return;

    NSString *vid = [NSString stringWithUTF8String:viewId];
    NSString *positionString =
        position ? [NSString stringWithUTF8String:position] : PSAdPresetNone;

    dispatch_async(dispatch_get_main_queue(), ^{
      UIView *rootView = PubstarGetRootView();
      if (!rootView) {
          NSLog(@"[PubstarNative] pubstar_create_ad_view: rootView is nil");
          return;
      }

      CGRect bounds = rootView.bounds;

      CGRect frame = PubstarCalculateAdFrame(bounds, xDp, yDp, widthDp,
                                             heightDp, positionString);

      NSLog(@"[PubstarNative] Created ad view xDp=%f yDp=%f widthDp=%f hDp=%f "
            @"position=%s",
            xDp, yDp, widthDp, heightDp, position);
      NSLog(@"[PubstarNative] Created ad view frame=%@ position=%@",
            NSStringFromCGRect(frame), positionString);

      NSMutableDictionary *containers = PubstarGetAdContainers();
      UIView *container = containers[vid];

      if (!container) {
          container = [[UIView alloc] initWithFrame:frame];
          container.backgroundColor = UIColor.clearColor;
          container.userInteractionEnabled = YES;

          [rootView addSubview:container];
          containers[vid] = container;

          NSLog(@"[PubstarNative] Created ad view '%@' frame = %@", vid,
                NSStringFromCGRect(frame));
          return;
      }

      container.frame = frame;
      if (!container.superview) {
          [rootView addSubview:container];
      }
      NSLog(@"[PubstarNative] Updated ad view '%@' frame = %@", vid,
            NSStringFromCGRect(frame));
    });
}

void pubstar_destroy_ad_view(const char *viewId) {
    if (!viewId)
        return;

    NSString *vid = [NSString stringWithUTF8String:viewId];

    dispatch_async(dispatch_get_main_queue(), ^{
      NSMutableDictionary *containers = PubstarGetAdContainers();
      UIView *container = containers[vid];
      if (!container) {
          NSLog(@"[PubstarNative] pubstar_destroy_ad_view: no container for id "
                @"'%@'",
                vid);
          return;
      }

      [container removeFromSuperview];
      [containers removeObjectForKey:vid];

      NSLog(@"[PubstarNative] Destroyed ad view '%@'", vid);
    });
}

void pubstar_show_banner_in_view(const char *viewId, const char *placementId, const char *size) {
    if (!viewId || !placementId)
        return;

    NSString *vid = [NSString stringWithUTF8String:viewId];
    NSString *adId = [NSString stringWithUTF8String:placementId];
    NSString *sizeString = [NSString stringWithUTF8String:size];

    PubstarImpl *impl = GetModuleImpl();
    if (!impl) {
        NSLog(@"[PubstarNative] pubstar_show_banner_in_view: impl is nil");
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
      NSMutableDictionary *containers = PubstarGetAdContainers();
      UIView *container = containers[vid];
      if (!container) {
          NSLog(@"[PubstarNative] pubstar_show_banner_in_view: no container "
                @"for id '%@'",
                vid);
          return;
      }

      NSLog(@"[PubstarNative] Show banner adId=%@ in viewId=%@", adId, vid);

      [impl loadAndShowBannerAdWithAdId:adId
          view:container
          size:sizeString
          onLoaderError:^(NSInteger errorCode) {
            NSString *msg = [NSString stringWithFormat:@"%ld", (long)errorCode];
            NSLog(@"[PubstarNative] pubstar_show_banner_in_view onLoadedError "
                  @"called mesage %s",
                  msg.UTF8String);

            PubstarUnitySendMessage(@"OnPubstarLoadError", msg);
          }
          onLoaded:^{
            NSLog(
                @"[PubstarNative] pubstar_show_banner_in_view onLoaded called");

            PubstarUnitySendMessage(@"OnPubstarLoaded", vid);
          }
          onHide:^(NSDictionary<NSString *, id> *_Nullable payload) {
            NSLog(@"[PubstarNative] pubstar_show_banner_in_view onHide called");

            NSString *payloadString = @"";
            if (payload && [NSJSONSerialization isValidJSONObject:payload]) {
                NSError *error = nil;
                NSData *jsonData =
                    [NSJSONSerialization dataWithJSONObject:payload
                                                    options:0
                                                    error:&error];
                if (!error && jsonData) {
                    payloadString =
                        [[NSString alloc] initWithData:jsonData
                                            encoding:NSUTF8StringEncoding];
                }
            }
            PubstarUnitySendMessage(@"OnPubstarAdHidden", payloadString);
          }
          onShowed:^{
            NSLog(
                @"[PubstarNative] pubstar_show_banner_in_view onShowed called");

            PubstarUnitySendMessage(@"OnPubstarAdShowed", vid);
          }
          onShowedError:^(NSInteger errorCode) {
            NSString *msg = [NSString stringWithFormat:@"%ld", (long)errorCode];
            NSLog(@"[PubstarNative] pubstar_show_banner_in_view onShowedError "
                  @"called message %s",
                  msg.UTF8String);

            PubstarUnitySendMessage(@"OnPubstarShowError", msg);
          }];
    });
}

void pubstar_show_native_in_view(const char *viewId, const char *placementId, const char *size) {
    if (!viewId || !placementId)
        return;
    
    NSString *vid = [NSString stringWithUTF8String:viewId];
    NSString *adId = [NSString stringWithUTF8String:placementId];
    NSString *sizeString = [NSString stringWithUTF8String:size];

    PubstarImpl *impl = GetModuleImpl();
    if (!impl) {
        NSLog(@"[PubstarNative] pubstar_show_native_in_view: impl is nil");
        return;
    }

    dispatch_async(dispatch_get_main_queue(), ^{
      NSMutableDictionary *containers = PubstarGetAdContainers();
      UIView *container = containers[vid];
      if (!container) {
          NSLog(@"[PubstarNative] pubstar_show_native_in_view: no container "
                @"for id '%@'",
                vid);
          return;
      }

      NSLog(@"[PubstarNative] Show native adId=%@ in viewId=%@", adId, vid);

      [impl loadAndShowNativeAdWithAdId:adId
          view:container
          size:sizeString
          onLoaderError:^(NSInteger errorCode) {
            NSString *msg = [NSString stringWithFormat:@"%ld", (long)errorCode];
            NSLog(@"[PubstarNative] pubstar_show_native_in_view onLoadedError "
                  @"called mesage %s",
                  msg.UTF8String);

            PubstarUnitySendMessage(@"OnPubstarLoadError", msg);
          }
          onLoaded:^{
            NSLog(
                @"[PubstarNative] pubstar_show_native_in_view onLoaded called");

            PubstarUnitySendMessage(@"OnPubstarLoaded", vid);
          }
          onHide:^(NSDictionary<NSString *, id> *_Nullable payload) {
            NSLog(@"[PubstarNative] pubstar_show_native_in_view onHide called");

            NSString *payloadString = @"";
            if (payload && [NSJSONSerialization isValidJSONObject:payload]) {
                NSError *error = nil;
                NSData *jsonData =
                    [NSJSONSerialization dataWithJSONObject:payload
                                                    options:0
                                                    error:&error];
                if (!error && jsonData) {
                    payloadString =
                        [[NSString alloc] initWithData:jsonData
                                            encoding:NSUTF8StringEncoding];
                }
            }
            PubstarUnitySendMessage(@"OnPubstarAdHidden", payloadString);
          }
          onShowed:^{
            NSLog(
                @"[PubstarNative] pubstar_show_native_in_view onShowed called");

            PubstarUnitySendMessage(@"OnPubstarAdShowed", vid);
          }
          onShowedError:^(NSInteger errorCode){
            NSString *msg = [NSString stringWithFormat:@"%ld", (long)errorCode];
            NSLog(@"[PubstarNative] pubstar_show_native_in_view onShowedError "
                  @"called message %s",
                  msg.UTF8String);

            PubstarUnitySendMessage(@"OnPubstarShowError", msg);
          }];
    });
}

} // extern "C"
