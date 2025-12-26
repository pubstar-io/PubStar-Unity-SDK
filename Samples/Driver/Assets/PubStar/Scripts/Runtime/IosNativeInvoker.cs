#if UNITY_IOS
using System.Runtime.InteropServices;

namespace PubStar.Io
{
    internal sealed class IosNativeInvoker : INativeInvoker
    {
        [DllImport("__Internal")] private static extern void pubstar_initialize(string gameObjectName);
        [DllImport("__Internal")] private static extern void pubstar_load(string placementId);
        [DllImport("__Internal")] private static extern void pubstar_show(string placementId);
        [DllImport("__Internal")] private static extern void pubstar_load_and_show(string placementId);

        [DllImport("__Internal")] private static extern void pubstar_create_ad_view(
            string viewId, float x, float y, float width, float height, string position);

        [DllImport("__Internal")] private static extern void pubstar_destroy_ad_view(string viewId);
        [DllImport("__Internal")] private static extern void pubstar_show_banner_in_view(string viewId, string placementId, string size);
        [DllImport("__Internal")] private static extern void pubstar_show_native_in_view(string viewId, string placementId, string size);

        public void Initialize(string gameObjectName) => pubstar_initialize(gameObjectName);
        public void Load(string placementId) => pubstar_load(placementId);
        public void Show(string placementId) => pubstar_show(placementId);
        public void LoadAndShow(string placementId) => pubstar_load_and_show(placementId);

        public void CreateAdView(string viewId, float x, float y, float width, float height, string position)
            => pubstar_create_ad_view(viewId, x, y, width, height, position);

        public void DestroyAdView(string viewId) => pubstar_destroy_ad_view(viewId);

        public void ShowBannerInView(string viewId, string placementId, string size)
            => pubstar_show_banner_in_view(viewId, placementId, size);

        public void ShowNativeInView(string viewId, string placementId, string size)
            => pubstar_show_native_in_view(viewId, placementId, size);
    }
}
#endif
