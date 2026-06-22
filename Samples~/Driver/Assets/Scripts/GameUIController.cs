using System.Threading.Tasks;
using PubStar.Io;
using UnityEngine;
using static PubStar.Io.PubStar;

public class GameUIController : MonoBehaviour
{
    private BannerView _banner;

    private string bannerAdID = "1277/99228313841";
    private string nativeAdID = "1277/99228313830";
    private string videoAdID = "1687/99228314138";
    private string interstitialAdID = "1277/99228313850";
    private string openAdID = "1277/99228313844";
    private string rewardedAdID = "1687/99228314076";


    // private string bannerAdID = "1264/99228313741";
    // private string nativeAdID = "1264/99228313724";
    // private string videoAdID = "1692/99228314124";
    // private string interstitialAdID = "1264/99228313740";
    // private string openAdID = "1264/99228313722";
    // private string rewardedAdID = "1692/99228314091";

    private void RenderBannerAds()
    {
        _banner = new BannerView(
            bannerAdID,
            AdSize.Medium,
            AdPosition.Center);
        _banner.OnLoaded += () =>
        {
            Debug.Log($"[GAME] Banner Ad was Loaded with id({bannerAdID})");
        };
        _banner.OnShowed += () =>
        {
            Debug.Log($"[GAME] Banner Ad was Showed with id({bannerAdID})");
        };
        _banner.Show();
    }

    private NativeView _native;
    private void RenderNativeAds()
    {
        var customConfig = new NativeCustomConfig.Builder("AppAdmobNativeCustom")
                .SetAdvertiserTextViewId("1")
                .SetIconImageViewId("2")
                .SetTitleTextViewId("3")
                .SetMediaContentViewGroupId("4")
                .SetBodyTextViewId("5")
                .SetCallToActionButtonId("6")
                .SetCtaColorHex("#FFFFFF")
                .SetLoadingViewName("AppShimmerBanner")
                .Build();

        Debug.Log($"[TEST][RenderNativeAds] customConfig is {customConfig.ToString()}");
        _native = new NativeView(
            nativeAdID,
            AdSize.Large,
            AdPosition.Bottom,
            customConfig
        );
        _native.OnLoaded += () =>
        {
            Debug.Log($"[GAME] Native Ad was Loaded with id({nativeAdID})");
        };
        _native.OnShowed += () =>
        {
            Debug.Log($"[GAME] Native Ad was Showed with id({nativeAdID})");
        };
        _native.Show();
    }

    private VideoView _video;
    private void RenderVideoAds()
    {
        _video = new VideoView(
            videoAdID,
            AdSize.Large,
            AdPosition.Bottom,
            "https://storage.googleapis.com/gvabox/media/samples/stock.mp4"
        );
        _video.OnLoaded += () =>
        {
            Debug.Log($"[GAME] Video Ad was Loaded with id({videoAdID})");
        };
        _video.OnShowed += () =>
        {
            Debug.Log($"[GAME] Video Ad was Showed with id({videoAdID})");
        };
        _video.Show();
    }

    private void OnDestroy()
    {
        if (_banner != null)
        {
            _banner.Destroy();
        }

        if (_native != null)
        {
            _native.Destroy();
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log("[GAME] Calling PubStar.Initialize() at startup");
        PubStar.Io.PubStar.Initialize(
            onDone: async () =>
            {
                Debug.Log("[GAME] Pubstar init success");
                await Task.Delay(3000);
                // RenderBannerAds();
                RenderNativeAds();
                RenderVideoAds();
            },
            onError: code =>
            {
                Debug.LogError($"[GAME] Pubstar init failed: {code}");
            }
        );

    }

    public void OnButtonInterstitialAdsClicked()
    {
        Debug.Log($"[GAME] Button Interstitial Ads clicked width adID: {interstitialAdID}");
        Load(
            interstitialAdID,
            onLoaded: () =>
            {
                Debug.Log($"[GAME] Interstitial Loaded: {interstitialAdID}");

                Show(
                    interstitialAdID,
                    onShowed: () => Debug.Log($"[GAME] Interstitial Showed: {interstitialAdID}"),
                    onHidden: payloadJson => Debug.Log($"[GAME] Interstitial Hidden payload: {payloadJson}"),
                    onError: err => Debug.LogError($"[GAME] Interstitial Show error: {err}")
                    );
            },
            onError: err => Debug.LogError($"[GAME] Interstitial Load error: {err}")
            );
    }

    public void OnButtonOpenAdsClicked()
    {
        Debug.Log($"[GAME] Button Open Ads clicked with adID: {openAdID}");
        LoadAndShow(
            openAdID,
            onLoaded: () => Debug.Log($"[GAME] Open Loaded: {openAdID}"),
            onLoadError: err => Debug.LogError($"[GAME] Open Load error: {err}"),
            onShowed: () => Debug.Log($"[GAME] Open Showed: {openAdID}"),
            onHidden: payloadJson => Debug.Log($"[GAME] Open Hidden payload: {payloadJson}"),
            onShowError: err => Debug.LogError($"[GAME] Open Show error: {err}")
        );
    }

    public void OnButtonRewardedAdsClicked()
    {
        Debug.Log($"[GAME] Button Rewarded Ads clicked with adID: {rewardedAdID}");
        LoadAndShow(
            rewardedAdID,
            onLoaded: () => Debug.Log($"[GAME] Rewarded Loaded: {rewardedAdID}"),
            onLoadError: err => Debug.LogError($"[GAME] Rewarded Load error: {err}"),
            onShowed: () => Debug.Log($"[GAME] Rewarded Showed: {rewardedAdID}"),
            onHidden: payloadJson => Debug.Log($"[GAME] Rewarded Hidden payload: {payloadJson}"),
            onShowError: err => Debug.LogError($"[GAME] Rewarded Show error: {err}")
        );
    }
}
