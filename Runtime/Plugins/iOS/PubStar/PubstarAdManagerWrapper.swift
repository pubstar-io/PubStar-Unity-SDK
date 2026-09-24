//
//  PubstarAdManagerWrapper.swift
//  Pubstar-Unity-SDK
//
//  Created by Mobile  on 10/6/25.
//

import AVFoundation
import Pubstar

@available(iOS 13.0, *)
public final class PubstarAdManagerWrapper: NSObject {
    private static let _pubStarAdManager = PubStarAdManager.getInstance()
    private static let _pubStarAdController = PubStarAdManager.getAdController()
    private static var _cachedContext: UIViewController?

    /// Resolved lazily and retried until found. The old `static var _context =
    /// PubStarUtils.getHostingViewController()` ran its initializer exactly once, on
    /// first access — `pubstar_initialize`, usually reached from a script's Awake/Start. If the scene was
    /// not foreground-active yet, the lookup returned nil and `_context` stayed nil
    /// for the life of the process: init failed with NO_INIT (-7) and every
    /// load/show returned silently. Same defect, and same fix, as the React Native
    /// bridge, where it was reproduced on device. Must be read on the main thread.
    private static var _context: UIViewController? {
        if _cachedContext == nil {
            _cachedContext = PubStarUtils.getHostingViewController()
        }
        return _cachedContext
    }

    private override init() {
        super.init()
    }

    public static func initPubstar(
        onDone: @escaping () -> Void,
        onError: @escaping (ErrorCode) -> Void
    ) {
        // The scene and window lookup behind `_context` is UIKit and has to run on
        // main. Unity calls into the bridge from its player loop, which is the main
        // thread on iOS, so this is normally a no-op — it guards other callers.
        guard Thread.isMainThread else {
            DispatchQueue.main.async { initPubstar(onDone: onDone, onError: onError) }
            return
        }
        guard let context = _context else {
            // No foreground-active scene yet. Wait for one instead of failing for good.
            waitForActiveScene {
                if _context != nil {
                    initPubstar(onDone: onDone, onError: onError)
                } else {
                    onError(ErrorCode.NO_INIT)
                }
            }
            return
        }

        PubStarAdManager.gatherConsent(
            from: context,
            listener: ConsentGatheringCompleteHandler(onComplete: { error in
                PubStarAdManager.getInstance()
                    // isDebug = true khiến SDK bỏ qua `io.pubstar.key` trong Info.plist
                    // và dùng App ID debug dựng sẵn -> lệch với ad unit của app.
                    .setIsDebug(isDebug: false)
                    .setInitAdListener(
                        InitAdListenerHandler(
                            onDone: {
                                onDone()
                            },
                            onError: { errorCode in
                                onError(errorCode)
                            }
                        )
                    )
                    .initAd()

            })
        )
    }

    /// Calls `then` once a scene is foreground-active, or after 20 s at the latest.
    private static func waitForActiveScene(_ then: @escaping () -> Void) {
        var token: NSObjectProtocol?
        var finished = false
        let finish = {
            guard !finished else { return }
            finished = true
            if let token = token { NotificationCenter.default.removeObserver(token) }
            then()
        }
        token = NotificationCenter.default.addObserver(
            forName: UIScene.didActivateNotification, object: nil, queue: .main
        ) { _ in finish() }
        // The scene may have activated between the failed lookup and registering the
        // observer; that notification would never arrive.
        DispatchQueue.main.async {
            if PubStarUtils.getHostingViewController() != nil { finish() }
        }
        DispatchQueue.main.asyncAfter(deadline: .now() + 20) { finish() }
    }

    public static func loadAd(
        adId: String,
        onLoaded: @escaping () -> Void,
        onError: @escaping (ErrorCode) -> Void
    ) {
        if _context == nil {
            return
        }

        let adNetLoaderListener: AdLoaderListener = AdLoaderHandler {
            onLoaded()
        } onError: { errorCode in
            onError(errorCode)
        }

        _pubStarAdController.load(
            context: _context!,
            key: adId,
            adLoaderListener: adNetLoaderListener
        )
    }

    public static func showAd(
        adId: String,
        view: UIView? = nil,
        onHide: @escaping (RewardModel?) -> Void,
        onShowed: @escaping () -> Void,
        onError: @escaping (ErrorCode) -> Void,
    ) {
        if _context == nil {
            return
        }

        let adShowedListener: AdShowedListener = AdShowedHandler {
            onShowed()
        } onHide: { state in
            onHide(state)
        } onError: { errorCode in
            onError(errorCode)
        }

        _pubStarAdController.show(
            context: _context!,
            key: adId,
            view: view,
            adShowedListener: adShowedListener,
        )
    }

    public static func loadAndShowAd(
        adId: String,
        view: UIView? = nil,
        onLoadedError: @escaping (ErrorCode) -> Void,
        onLoaded: @escaping () -> Void,
        onHide: @escaping (RewardModel?) -> Void,
        onShowed: @escaping () -> Void,
        onShowedError: @escaping (ErrorCode) -> Void
    ) {
        if _context == nil {
            return
        }

        let adNetLoaderListener: AdLoaderListener = AdLoaderHandler {
            onLoaded()
        } onError: { code in
            onLoadedError(code)
        }

        let adNetShowListener: AdShowedListener = AdShowedHandler {
            onShowed()
        } onHide: { state in
            onHide(state)
        } onError: { errorCode in
            onShowedError(errorCode)
        }

        _pubStarAdController
            .loadAndShow(
                context: _context!,
                key: adId,
                view: view,
                adLoaderListener: adNetLoaderListener,
                adShowedListener: adNetShowListener
            )
    }

    public static func loadAndShowNativeAd(
        adId: String,
        view: UIView? = nil,
        size: NativeAdRequest.TypeSize,
        isAllowLoadNext: Bool = true,
        onLoaderError: @escaping (ErrorCode) -> Void,
        onLoaded: @escaping () -> Void,
        onHide: @escaping (RewardModel?) -> Void,
        onShowed: @escaping () -> Void,
        onShowedError: @escaping (ErrorCode) -> Void
    ) {
        if _context == nil {
            return
        }

        let adNetLoaderListener: AdLoaderListener = AdLoaderHandler {
            onLoaded()
        } onError: { code in
            onLoaderError(code)
        }

        let adNetShowListener: AdShowedListener = AdShowedHandler {
            onShowed()
        } onHide: { state in
            onHide(state)
        } onError: { errorCode in
            onShowedError(errorCode)
        }

        let request = NativeAdRequest.Builder(context: _context!)
            .isAllowLoadNext(isAllowLoadNext)
            .withView(view)
            .sizeType(size)
            .adLoaderListener(adNetLoaderListener)
            .adShowedListener(adNetShowListener)
            .build()

        _pubStarAdController
            .loadAndShow(
                key: adId,
                adRequest: request
            )
    }

    public static func loadAndShowBannerAd(
        adId: String,
        view: UIView? = nil,
        tag: BannerAdRequest.AdTag,
        isAllowLoadNext: Bool = true,
        onLoaderError: @escaping (ErrorCode) -> Void,
        onLoaded: @escaping () -> Void,
        onHide: @escaping (RewardModel?) -> Void,
        onShowed: @escaping () -> Void,
        onShowedError: @escaping (ErrorCode) -> Void
    ) {
        if _context == nil {
            return
        }

        let adNetLoaderListener: AdLoaderListener = AdLoaderHandler {
            onLoaded()
        } onError: { code in
            onLoaderError(code)
        }

        let adNetShowListener: AdShowedListener = AdShowedHandler {
            onShowed()
        } onHide: { state in
            onHide(state)
        } onError: { errorCode in
            onShowedError(errorCode)
        }

        let request = BannerAdRequest.Builder(context: _context!)
            .isAllowLoadNext(isAllowLoadNext)
            .withView(view)
            .tag(tag)
            .adLoaderListener(adNetLoaderListener)
            .adShowedListener(adNetShowListener)
            .build()

        _pubStarAdController
            .loadAndShow(
                key: adId,
                adRequest: request
            )
    }

    public static func loadAndShowVideoAd(
        adId: String,
        view: UIView? = nil,
        media: String,
        onLoaderError: @escaping (ErrorCode) -> Void,
        onLoaded: @escaping () -> Void,
        onHide: @escaping (RewardModel?) -> Void,
        onShowed: @escaping () -> Void,
        onShowedError: @escaping (ErrorCode) -> Void

    ) {
        guard _context != nil else {
            onLoaderError(ErrorCode.INIT_ERROR)
            return
        }

        let adNetLoaderListener: AdLoaderListener = AdLoaderHandler {
            onLoaded()
        } onError: { code in
            onLoaderError(code)
        }

        let adNetShowListener: AdShowedListener = AdShowedHandler {
            onShowed()
        } onHide: { state in
            onHide(state)
        } onError: { errorCode in
            onShowedError(errorCode)
        }

        let request = IMARequest.Builder(context: _context!)
            .withView(view)
            .adLoaderListener(adNetLoaderListener)
            .adShowedListener(adNetShowListener)

        if media != "" {
            let player = self.createPlayerVideo(url: media)
            let _ = request.withMedia(player).withType(
                IMARequest.IMAType.inStream
            )
        } else {
            let _ = request.withType(IMARequest.IMAType.outStream)
                .withSize(IMARequest.IMASize.medium)
        }

        _pubStarAdController.loadAndShow(
            key: adId,
            adRequest: request.build()
        )
    }

    private static func createPlayerVideo(url: String) -> AVPlayer? {
        guard let url = URL(string: url) else {
            return nil
        }

        let player = AVPlayer(url: url)
        player.isMuted = true
        player.actionAtItemEnd = .none
        player.play()

        return player
    }
}
