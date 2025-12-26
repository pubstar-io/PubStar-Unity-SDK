#if UNITY_ANDROID
using UnityEngine;

namespace PubStar.Io
{
    internal sealed class AndroidNativeInvoker : INativeInvoker
    {
        private const string NATIVE_BRIDGE_CLASS = "io.pubstar.unity.PubStarUnityBridge";
        private AndroidJavaClass _cls;

        private AndroidJavaClass Cls => _cls ??= new AndroidJavaClass(NATIVE_BRIDGE_CLASS);

        public void Initialize(string gameObjectName)
        {
#if !UNITY_EDITOR
            Cls.CallStatic("initialize", gameObjectName);
#endif
        }

        public void Load(string placementId)
        {
#if !UNITY_EDITOR
            Cls.CallStatic("load", placementId);
#endif
        }

        public void Show(string placementId)
        {
#if !UNITY_EDITOR
            Cls.CallStatic("show", placementId);
#endif
        }

        public void LoadAndShow(string placementId)
        {
#if !UNITY_EDITOR
            Cls.CallStatic("loadAndShow", placementId);
#endif
        }

        public void CreateAdView(string viewId, float x, float y, float width, float height, string position)
        {
#if !UNITY_EDITOR
            Cls.CallStatic("createAdView", viewId, x, y, width, height, position);
#endif
        }

        public void DestroyAdView(string viewId)
        {
#if !UNITY_EDITOR
            Cls.CallStatic("destroyAdView", viewId);
#endif
        }

        public void ShowBannerInView(string viewId, string placementId, string size)
        {
#if !UNITY_EDITOR
            Cls.CallStatic("showBannerInView", viewId, placementId, size);
#endif
        }

        public void ShowNativeInView(string viewId, string placementId, string size)
        {
#if !UNITY_EDITOR
            Cls.CallStatic("showNativeInView", viewId, placementId, size);
#endif
        }
    }
}
#endif
