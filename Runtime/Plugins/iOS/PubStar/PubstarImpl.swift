//
//  PubstarImpl.swift
//  Pubstar-Unity-SDK
//
//  Created by Mobile  on 10/6/25.
//

import Foundation
import Pubstar

extension String {
    func toNativeAdTypeSize() -> NativeAdRequest.TypeSize {
        switch self.lowercased() {
        case "small":
            return .Small
        case "medium":
            return .Medium
        case "large":
            return .Big
        default:
            return .Small
        }
    }

    func toBannerAdSize() -> BannerAdRequest.AdTag {
        switch self.lowercased() {
        case "small":
            return .small
        case "medium":
            return .medium
        case "large":
            return .big
        default:
            return .small
        }
    }
}

@objc public class PubstarImpl: NSObject {
    @objc public func initialization(
        onDone: @escaping () -> Void,
        onError: @escaping (Int) -> Void
    ) {
        PubstarAdManagerWrapper.initPubstar(
            onDone: {
                onDone()
            },
            onError: { errorCode in
                onError(errorCode.rawValue)
            }
        )
    }

    @objc public func loadAd(
        adId: String,
        onLoaded: @escaping () -> Void,
        onError: @escaping (Int) -> Void
    ) {
        PubstarAdManagerWrapper.loadAd(
            adId: adId,
            onLoaded: {
                onLoaded()
            },
            onError: { errorCode in
                onError(errorCode.rawValue)
            }
        )
    }

    @objc public func showAd(
        adId: String,
        onHide: @escaping ([String: Any]?) -> Void,
        onShowed: @escaping () -> Void,
        onError: @escaping (Int) -> Void
    ) {
        PubstarAdManagerWrapper
            .showAd(
                adId: adId,
                onHide: { reward in
                    if let reward = reward {
                        onHide([
                            "type": reward.type,
                            "amount": NSNumber(value: reward.amount),
                        ])
                    } else {
                        onHide(nil)
                    }
                },
                onShowed: {
                    onShowed()
                },
                onError: { errorCode in
                    onError(errorCode.rawValue)
                }
            )
    }

    @objc public func loadAndShow(
        adId: String,
        onLoadedError: @escaping (Int) -> Void,
        onLoaded: @escaping () -> Void,
        onHide: @escaping ([String: Any]?) -> Void,
        onShowed: @escaping () -> Void,
        onShowedError: @escaping (Int) -> Void
    ) {
        PubstarAdManagerWrapper.loadAndShowAd(
            adId: adId,
            onLoadedError: { errorCode in
                onLoadedError(errorCode.rawValue)
            },
            onLoaded: {
                onLoaded()
            },
            onHide: { reward in
                if let reward = reward {
                    onHide([
                        "type": reward.type,
                        "amount": NSNumber(value: reward.amount),
                    ])
                } else {
                    onHide(nil)
                }
            },
            onShowed: {
                onShowed()
            },
            onShowedError: { errorCode in
                onShowedError(errorCode.rawValue)
            },
        )
    }

    @objc public func loadAndShowNativeAd(
        adId: String,
        view: UIView? = nil,
        size: String,
        onLoaderError: @escaping (Int) -> Void,
        onLoaded: @escaping () -> Void,
        onHide: @escaping ([String: Any]?) -> Void,
        onShowed: @escaping () -> Void,
        onShowedError: @escaping (Int) -> Void,
        customConfig: String? = nil
    ) {
        print(
            "[TEST][PubstarImpl][loadAndShowNativeAd] customConfig is \(dump(customConfig))"
        )
        PubstarAdManagerWrapper.loadAndShowNativeAd(
            adId: adId,
            view: view,
            size: size.toNativeAdTypeSize(),
            isAllowLoadNext: false,
            onLoaderError: { errorCode in
                onLoaderError(errorCode.rawValue)
            },
            onLoaded: {
                onLoaded()
            },
            onHide: { reward in
                if let reward = reward {
                    onHide([
                        "type": reward.type,
                        "amount": NSNumber(value: reward.amount),
                    ])
                } else {
                    onHide(nil)
                }
            },
            onShowed: {
                onShowed()
            },
            onShowedError: { errorCode in
                onShowedError(errorCode.rawValue)
            },
            customConfig: buildCustomConfig(configString: customConfig)
        )
    }

    private func buildCustomConfig(configString: String?) -> NativeAdViewBinder?
    {
        guard let data = configString?.data(using: .utf8) else { return nil }

        do {
            if let dict = try JSONSerialization.jsonObject(
                with: data,
                options: []
            ) as? [String: Any],
                let layoutName = dict["layoutName"] as? String
            {

                let builder = NativeAdViewBinder.Builder(layoutId: layoutName)

                func getInt(from key: String) -> Int? {
                    if let intVal = dict[key] as? Int { return intVal }
                    if let strVal = dict[key] as? String { return Int(strVal) }
                    return nil
                }

                if let id = getInt(from: "titleTextViewId") {
                    _ = builder.setTitleTextViewId(id)
                }
                if let id = getInt(from: "bodyTextViewId") {
                    _ = builder.setBodyTextViewId(id)
                }
                if let id = getInt(from: "advertiserTextViewId") {
                    _ = builder.setAdvertiserTextViewId(id)
                }
                if let id = getInt(from: "iconImageViewId") {
                    _ = builder.setIconImageViewId(id)
                }
                if let id = getInt(from: "mediaContentViewGroupId") {
                    _ = builder.setMediaContentViewGroupId(id)
                }
                if let id = getInt(from: "callToActionButtonId") {
                    _ = builder.setCallToActionButtonId(id)
                }

                if let xibName = dict["loadingViewId"] as? String {
                    let pathExists =
                        Bundle.main.path(forResource: xibName, ofType: "nib")
                        != nil

                    if pathExists {
                        let nib = UINib(nibName: xibName, bundle: Bundle.main)
                        if let loadingView = nib.instantiate(
                            withOwner: nil,
                            options: nil
                        ).first as? UIView {
                            _ = builder.setLoadingView(loadingView)
                        }
                    }
                }

                return builder.build()
            }

        } catch {
            print("Error parsing JSON: \(error)")
            return nil
        }

        return nil
    }

    @objc public func loadAndShowBannerAd(
        adId: String,
        view: UIView? = nil,
        size: String,
        onLoaderError: @escaping (Int) -> Void,
        onLoaded: @escaping () -> Void,
        onHide: @escaping ([String: Any]?) -> Void,
        onShowed: @escaping () -> Void,
        onShowedError: @escaping (Int) -> Void
    ) {
        PubstarAdManagerWrapper.loadAndShowBannerAd(
            adId: adId,
            view: view,
            tag: size.toBannerAdSize(),
            isAllowLoadNext: false,
            onLoaderError: { errorCode in
                onLoaderError(errorCode.rawValue)
            },
            onLoaded: {
                onLoaded()
            },
            onHide: { reward in
                if let reward = reward {
                    onHide([
                        "type": reward.type,
                        "amount": NSNumber(value: reward.amount),
                    ])
                } else {
                    onHide(nil)
                }
            },
            onShowed: {
                onShowed()
            },
            onShowedError: { errorCode in
                onShowedError(errorCode.rawValue)
            },
        )
    }

}
