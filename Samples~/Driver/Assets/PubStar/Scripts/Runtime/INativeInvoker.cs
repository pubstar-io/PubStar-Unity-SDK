namespace PubStar.Io
{
    internal interface INativeInvoker
    {
        void Initialize(string gameObjectName);

        void Load(string placementId);
        void Show(string placementId);

        void LoadAndShow(string placementId);

        void CreateAdView(string viewId, float x, float y, float width, float height, string position);
        void DestroyAdView(string viewId);
        void ShowBannerInView(string viewId, string placementId, string size);
        void ShowNativeInView(string viewId, string placementId, string size);
    }
}
