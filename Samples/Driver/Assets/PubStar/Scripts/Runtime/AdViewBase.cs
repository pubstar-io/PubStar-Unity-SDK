
// using System;
// using UnityEngine;

// namespace PubStar.Io
// {
    
//         /// <summary>
//         /// Base class for view-based ads (Banner/Native) that share the same lifecycle:
//         /// CreateAdView -> ShowXxxInView -> DestroyAdView.
//         /// </summary>
//         public abstract class AdViewBase
//         {
//             private readonly string _viewId;
//             protected readonly string PlacementId;
//             protected readonly AdSize AdSize;
//             protected readonly AdPosition AdPosition;

//             private bool _isShown;

//             public event Action OnLoaded;
//             public event Action<int> OnLoadError;
//             public event Action OnShowed;
//             public event Action<string> OnHidden;
//             public event Action<int> OnShowError;

//             protected AdViewBase(string viewId, string placementId, AdSize adSize, AdPosition adPosition)
//             {
//                 _viewId = viewId ?? throw new ArgumentNullException(nameof(viewId));
//                 PlacementId = placementId;
//                 AdSize = adSize;
//                 AdPosition = adPosition;
//             }

//             public string ViewId => _viewId;

//             public void Show()
//             {
//                 if (_isShown)
//                 {
//                     Debug.Log($"[{GetLogTag()}] Already shown. viewId={_viewId}");
//                     return;
//                 }

//                 CreateNativeAdView();
//                 ShowAdsInViewInternal();

//                 _isShown = true;
//             }

//             public void Destroy()
//             {
//                 if (!_isShown) return;

// #if UNITY_IOS && !UNITY_EDITOR
//                 Debug.Log($"[{GetLogTag()}] DestroyAdView running on iOS. viewId={_viewId}");
//                 PubStarIosBridge.DestroyAdView(_viewId);
// #elif UNITY_ANDROID && !UNITY_EDITOR
//                 Debug.Log($"[{GetLogTag()}] DestroyAdView running on Android. viewId={_viewId}");
//                 PubStarAndroidBridge.DestroyAdView(_viewId);
// #else
//                 Debug.Log($"[{GetLogTag()}] DestroyAdView - running in Editor/non-mobile, no-op.");
// #endif

//                 _isShown = false;
//             }

//             private void CreateNativeAdView()
//             {
// #if UNITY_IOS && !UNITY_EDITOR
//                 Debug.Log($"[{GetLogTag()}] CreateAdView iOS. viewId={_viewId} preset={AdPosition.Preset}");
//                 PubStarIosBridge.CreateAdView(
//                     _viewId,
//                     AdPosition.X,
//                     AdPosition.Y,
//                     AdSize.Width,
//                     AdSize.Height,
//                     AdPosition.Preset
//                 );
// #elif UNITY_ANDROID && !UNITY_EDITOR
//                 Debug.Log($"[{GetLogTag()}] CreateAdView Android. viewId={_viewId} preset={AdPosition.Preset}");
//                 PubStarAndroidBridge.CreateAdView(
//                     _viewId,
//                     AdPosition.X,
//                     AdPosition.Y,
//                     AdSize.Width,
//                     AdSize.Height,
//                     AdPosition.Preset
//                 );
// #else
//                 Debug.Log($"[{GetLogTag()}] CreateAdView - running in Editor/non-mobile, no-op.");
// #endif
//             }

//             private void ShowAdsInViewInternal()
//             {
//                 Debug.Log($"[{GetLogTag()}] ShowAdsInView called. viewId={_viewId} placementId={PlacementId} size={AdSize.SizeValue}");
//                 ShowAdsInView(
//                     _viewId,
//                     PlacementId,
//                     AdSize.SizeValue,
//                     OnLoaded,
//                     OnLoadError,
//                     OnShowed,
//                     OnHidden,
//                     OnShowError
//                 );
//             }

//             protected abstract string GetLogTag();

//             /// <summary>
//             /// Derived classes decide which native bridge method to call.
//             /// </summary>
//             protected abstract void ShowAdsInView(
//                 string viewId,
//                 string placementId,
//                 string sizeValue,
//                 Action onLoaded,
//                 Action<int> onLoadError,
//                 Action onShowed,
//                 Action<string> onHidden,
//                 Action<int> onShowError
//             );
//         }

// }