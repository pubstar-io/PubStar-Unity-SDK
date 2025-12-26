using UnityEngine;
using System;
using System.Collections.Generic;

namespace PubStar.Io
{
    public sealed class PubStar : MonoBehaviour
    {
        private static PubStar _instance;

        private static PubStar Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = new GameObject(nameof(PubStar));
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<PubStar>();
                return _instance;
            }
        }

        private static INativeInvoker _native;
        private static INativeInvoker Native
        {
            get
            {
                if (_native != null) return _native;

#if UNITY_ANDROID && !UNITY_EDITOR
                _native = new AndroidNativeInvoker();
#elif UNITY_IOS && !UNITY_EDITOR
                _native = new IosNativeInvoker();
#else
                _native = new NoopNativeInvoker();
#endif
                return _native;
            }
        }

        // ===== Shared state =====
        private static readonly Dictionary<string, AdCallbacks> _adCallbacksByViewId = new();

        private static Action _onInitDone;
        private static Action<int> _onInitError;

        private static Action _onLoaded;
        private static Action<int> _onLoadError;
        private static Action _onAdShowed;
        private static Action<string> _onAdHidden;
        private static Action<int> _onShowError;

        // ===== Native -> Unity callbacks =====
        public void OnPubstarInitDone(string _)
        {
            Debug.Log("[PubStarBridge] OnPubstarInitDone");
            _onInitDone?.Invoke();
        }

        public void OnPubstarInitError(string errorCodeString)
        {
            Debug.Log($"[PubStarBridge] OnPubstarInitError: {errorCodeString}");
            _onInitError?.Invoke(ParseIntSafe(errorCodeString));
        }

        public void OnPubstarLoaded(string viewId)
        {
            Debug.Log($"[PubStarBridge] OnPubstarLoaded viewId={viewId}");
            if (_adCallbacksByViewId.TryGetValue(viewId, out var cb))
                cb.OnLoaded?.Invoke();

            _onLoaded?.Invoke();
        }

        public void OnPubstarLoadError(string errorCodeString)
        {
            var code = ParseIntSafe(errorCodeString);
            Debug.Log($"[PubStarBridge] OnPubstarLoadError: {code}");
            _onLoadError?.Invoke(code);
        }

        public void OnPubstarAdShowed(string viewId)
        {
            Debug.Log($"[PubStarBridge] OnPubstarAdShowed viewId={viewId}");
            if (_adCallbacksByViewId.TryGetValue(viewId, out var cb))
                cb.OnShowed?.Invoke();

            _onAdShowed?.Invoke();
        }

        public void OnPubstarAdHidden(string payloadJson)
        {
            Debug.Log($"[PubStarBridge] OnPubstarAdHidden payload={payloadJson}");
            _onAdHidden?.Invoke(payloadJson);
        }

        public void OnPubstarShowError(string errorCodeString)
        {
            var code = ParseIntSafe(errorCodeString);
            Debug.Log($"[PubStarBridge] OnPubstarShowError: {code}");
            _onShowError?.Invoke(code);
        }

        // ===== Public APIs =====
        internal static void Initialize(Action onDone, Action<int> onError)
        {
            _onInitDone = onDone;
            _onInitError = onError;

            _ = Instance.gameObject.name;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Native.Initialize(Instance.gameObject.name);
#else
            Debug.Log("[PubStarBridge] Initialize (Editor/Non-mobile) simulate success");
            onDone?.Invoke();
#endif
        }

        internal static void LoadAndShow(
            string placementId,
            Action onLoaded,
            Action<int> onLoadError,
            Action onShowed,
            Action<string> onHidden,
            Action<int> onShowError)
        {
            _onLoaded = onLoaded;
            _onLoadError = onLoadError;
            _onAdShowed = onShowed;
            _onAdHidden = onHidden;
            _onShowError = onShowError;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Native.LoadAndShow(placementId);
#else
            Debug.Log($"[PubStarBridge] LoadAndShow simulate in editor. placementId={placementId}");
            onLoaded?.Invoke();
            onShowed?.Invoke();
            onHidden?.Invoke("{}");
#endif
        }

        private static void CreateAdView(string viewId, float x, float y, float width, float height, string position)
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Debug.Log($"[PubStarBridge] CreateAdView is called viewId={viewId}");
            Native.CreateAdView(viewId, x, y, width, height, position);
#else
            Debug.Log($"[PubStarBridge] CreateAdView (Editor) viewId={viewId}");
#endif
        }

        private static void DestroyAdView(string viewId)
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Native.DestroyAdView(viewId);
#endif
            _adCallbacksByViewId.Remove(viewId);
        }

        internal static void ShowBannerInView(
            string viewId, string placementId, string size,
            Action onLoaded, Action<int> onLoadError,
            Action onShowed, Action<string> onHidden, Action<int> onShowError)
        {
            _adCallbacksByViewId[viewId] = new AdCallbacks
            {
                OnLoaded = onLoaded,
                OnLoadError = onLoadError,
                OnShowed = onShowed,
                OnHidden = onHidden,
                OnShowError = onShowError
            };

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Native.ShowBannerInView(viewId, placementId, size);
#else
            Debug.Log($"[PubStarBridge] ShowBannerInView (Editor) viewId={viewId}");
            onLoaded?.Invoke();
            onShowed?.Invoke();
#endif
        }

        private static void ShowNativeInView(
            string viewId, string placementId, string size,
            Action onLoaded, Action<int> onLoadError,
            Action onShowed, Action<string> onHidden, Action<int> onShowError)
        {
            _adCallbacksByViewId[viewId] = new AdCallbacks
            {
                OnLoaded = onLoaded,
                OnLoadError = onLoadError,
                OnShowed = onShowed,
                OnHidden = onHidden,
                OnShowError = onShowError
            };

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            Native.ShowNativeInView(viewId, placementId, size);
#else
            Debug.Log($"[PubStarBridge] ShowNativeInView (Editor) viewId={viewId}");
            onLoaded?.Invoke();
            onShowed?.Invoke();
#endif
        }

        private static int ParseIntSafe(string s) => int.TryParse(s, out var v) ? v : 0;

        private sealed class NoopNativeInvoker : INativeInvoker
        {
            public void Initialize(string gameObjectName) { }
            public void Load(string placementId) { }
            public void Show(string placementId) { }
            public void LoadAndShow(string placementId) { }
            public void CreateAdView(string viewId, float x, float y, float width, float height, string position) { }
            public void DestroyAdView(string viewId) { }
            public void ShowBannerInView(string viewId, string placementId, string size) { }
            public void ShowNativeInView(string viewId, string placementId, string size) { }
        }

        public abstract class AdViewBase
        {
            private readonly string _viewId;
            protected readonly string PlacementId;
            protected readonly AdSize AdSize;
            protected readonly AdPosition AdPosition;

            private bool _isShown;

            public event Action OnLoaded;
            public event Action<int> OnLoadError;
            public event Action OnShowed;
            public event Action<string> OnHidden;
            public event Action<int> OnShowError;

            protected AdViewBase(string viewId, string placementId, AdSize adSize, AdPosition adPosition)
            {
                _viewId = viewId ?? throw new ArgumentNullException(nameof(viewId));
                PlacementId = placementId;
                AdSize = adSize;
                AdPosition = adPosition;
            }

            public void Show()
            {
                if (_isShown)
                {
                    Debug.Log($"[{GetLogTag()}] Already shown. viewId={_viewId}");
                    return;
                }

                CreateNativeAdView();
                ShowAdsInViewInternal();

                _isShown = true;
            }

            public void Destroy()
            {
                if (!_isShown) return;

                DestroyAdView(_viewId);

                _isShown = false;
            }

            private void CreateNativeAdView()
            {
                PubStar.CreateAdView(
                    _viewId,
                    AdPosition.X,
                    AdPosition.Y,
                    AdSize.Width,
                    AdSize.Height,
                    AdPosition.Preset
                );
                Debug.Log($"[{GetLogTag()}] CreateAdView - running in Editor/non-mobile, no-op.");
            }

            private void ShowAdsInViewInternal()
            {
                Debug.Log($"[{GetLogTag()}] ShowAdsInView called. viewId={_viewId} placementId={PlacementId} size={AdSize.SizeValue}");
                this.ShowAdsInView(
                    _viewId,
                    PlacementId,
                    AdSize.SizeValue,
                    OnLoaded,
                    OnLoadError,
                    OnShowed,
                    OnHidden,
                    OnShowError
                );
            }

            protected abstract string GetLogTag();

            /// <summary>
            /// Derived classes decide which native bridge method to call.
            /// </summary>
            protected abstract void ShowAdsInView(
                string viewId,
                string placementId,
                string sizeValue,
                Action onLoaded,
                Action<int> onLoadError,
                Action onShowed,
                Action<string> onHidden,
                Action<int> onShowError
            );
        }

        public sealed class BannerView : AdViewBase
        {
            private static int _viewIdCounter = 0;

            public BannerView(string placementId, AdSize adSize, AdPosition adPosition)
                : base($"pubstar_banner_{_viewIdCounter++}", placementId, adSize, adPosition)
            {
            }

            protected override string GetLogTag() => "PubStar][BannerView";

            protected override void ShowAdsInView(
                string viewId,
                string placementId,
                string sizeValue,
                Action onLoaded,
                Action<int> onLoadError,
                Action onShowed,
                Action<string> onHidden,
                Action<int> onShowError
            )
            {
                PubStar.ShowBannerInView(
                    viewId,
                    placementId,
                    sizeValue,
                    onLoaded,
                    onLoadError,
                    onShowed,
                    onHidden,
                    onShowError
                );
                Debug.Log("[PubStar][BannerView][ShowNativeInView] is called");
            }
        }

        public sealed class NativeView : AdViewBase
        {
            private static int _viewIdCounter = 0;

            public NativeView(string placementId, AdSize adSize, AdPosition adPosition)
                : base($"pubstar_native_{_viewIdCounter++}", placementId, adSize, adPosition)
            {
            }

            protected override string GetLogTag() => "PubStar][NativeView";

            protected override void ShowAdsInView(
                string viewId,
                string placementId,
                string sizeValue,
                Action onLoaded,
                Action<int> onLoadError,
                Action onShowed,
                Action<string> onHidden,
                Action<int> onShowError
            )
            {
                PubStar.ShowNativeInView(
                    viewId,
                    placementId,
                    sizeValue,
                    onLoaded,
                    onLoadError,
                    onShowed,
                    onHidden,
                    onShowError
                );
                Debug.Log("[PubStar][NativeView][ShowNativeInView] is called");
            }
        }
    }
}
