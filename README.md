# Pubstar

PubStar Mobile AD SDK is a comprehensive software development kit designed to empower developers with robust tools and functionalities for integrating advertisements seamlessly into mobile applications. Whether you're a seasoned developer or a newcomer to the world of app monetization, our SDK offers a user-friendly solution to maximize revenue streams while ensuring a non-intrusive and engaging user experience.

## TOC

- [Features](#features)
- [Platform Support](#platform-support)
- [Requirements](#requirements)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [API](#api)
- [Release Notes](#release-notes)
- [ID Test Ad](#id-test-ad)
- [Support](#support)

## Features

- ✅ **Display native ads** on Android & iOS with Pubstar API.
- ✅ Full-featured API: Laod, Show, LoadAndShow, and handle ad events.
- ✅ Easy-to-use: `BannerView` and `NativeView`.
- ✅ Structured ad event callback for type-safe event handling.

## Platform Support

| Android | iOS |
| ------- | --- |
| ✔       | ✔   |

## Requirements

- iOS >= 13.0
- Android >= 26

## Installation

### Import from GitHub

1. Download the latest [`.unitypackage`](https://github.com/pubstar-io/PubStar-Unity-SDK/releases) release from GitHub.
2. Import the `.unitypackage` file by selecting the Unity menu option **Assets > Import package > Custom Package** and importing all items.

## Configuration

### iOS

#### 1. Configure Pod.

1. Navigate to your iOS project directory.

2. Install the dependencies using pod install.

    ```bash
    pod install
    ```

3. Open your project in Xcode with the .xcworkspace file.

#### 2. Update your Info.plist

Update your app's Info.plist file to add several keys:

- A GADApplicationIdentifier key with a string value of your AdMob app ID [found in the AdMob UI](https://support.google.com/admob/answer/7356431).

- A `io.pubstar.key` key with a string value of your Pubstar ad ID [found in the Pubstar Dashboard](https://pubstar.io/).

- SKAdNetworkItems in Google AdMob refers to the necessary configuration within your iOS app's Info.plist file to support Apple's SKAdNetwork for conversion tracking, particularly when using the Google Mobile Ads SDK for AdMob [found in the AdMob privacy](https://developers.google.com/admob/ios/privacy/strategies).

```xml
<key>GADApplicationIdentifier</key>
<string>Your AdMob app ID</string>
<key>SKAdNetworkItems</key>
	<array>
		<dict>
			<key>SKAdNetworkIdentifier</key>
			<string>cstr6suwn9.skadnetwork</string>
		</dict>

        ...

        <dict>
			<key>SKAdNetworkIdentifier</key>
			<string>3qcr597p9d.skadnetwork</string>
		</dict>
    </array>
<key>NSUserTrackingUsageDescription</key>
<string>We use your data to show personalized ads and improve your experience.</string>
<key>io.pubstar.key</key>
<string>Your PubStar app ID</string>
```

### Android

#### 1. Add PubStar Key to AndroidManifest.

Open `AndroidManifest.xml` and add inside `<application>`:

```bash
<meta-data
  android:name="io.pubstar.key"
  android:value="pub-app-id-XXXX" />
```

Replace pub-app-id-XXXX with your actual [PubStar App ID](https://pubstar.io/).

## Usage

### Initialize the SDK

Initializes the PubStar IO Ads SDK.

Must be called **once** before loading or showing any ad.

```C#
using PubStar.Io;

PubStar.Initialize(
    onDone: () =>
    {
        Debug.Log("Success");
    },
    onError: code =>
    {
        Debug.LogError($"Failed: {code}");
  }
);
```

## API

The example app in this repository shows an example usage of every single API, consult the example app if you have questions, and if you think you see a problem make sure you can reproduce it using the example app before reporting it, thank you.

| Method                            | Return Type      |
| --------------------------------- | ---------------- |
| [Initialize()](#init)             | `Funtion<void>`  |
| [Load()](#loadad)                 | `Function<void>` |
| [Show()](#showad)                 | `Function<void>` |
| [LoadAndShow()](#loadandshow)     | `Function<void>` |
| [BannerView](#pubstaradview)      | `Class`          |
| [NativeView](#pubstarvideoadview) | `Class`          |

### Initialize()

Initialization PubStar SDK.

#### Event

| Callback  | Function                               |
| --------- | -------------------------------------- |
| `onDone`  | call when initialization is successful |
| `onError` | call when ad initialization fails      |

```C#
PubStar.Initialize(
    onDone: () =>
    {
        Debug.Log("Success");
    },
    onError: code =>
    {
        Debug.LogError($"Failed: {code}");
  }
);
```

### Load()

Load Pubstar ads by adId to application.

#### Event

`LoadListener`

| Callback   | Function                                    |
| ---------- | ------------------------------------------- |
| `onError`  | call when load ad failed. return Error code |
| `onLoaded` | call when ad loaded                         |

#### Example

```C#
PubStar.Load(
  "Interstitial Ad ID",
  onLoaded: () => Debug.Log($"[GAME] Interstitial Loaded"),
  onError: err => Debug.LogError($"[GAME] Interstitial Load error: {err}")
);
```

### Show()

Show ad had loaded before.

#### Event

`ShowListener`

| Callback   | Function                                                                                                        |
| ---------- | --------------------------------------------------------------------------------------------------------------- |
| `onHidden`   | call when ad hidden/closed (supports rewarded ads). Returns detailed `PubstarReward` object (`type`, `amount`). |
| `onShowed` | call when ad showed                                                                                             |
| `onError`  | call when show ad failed. return Error code                                                                     |

#### Example

```C#
PubStar.Show(
  "Interstitial Ad ID",
  onShowed: () => Debug.Log($"[GAME] Interstitial Showed"),
  onHidden: payloadJson => Debug.Log($"[GAME] Interstitial Hidden payload: {payloadJson}"),
  onError: err => Debug.LogError($"[GAME] Interstitial Show error: {err}")
);
```

### LoadAndShow()

Load and immediately show an ad by ID.

#### Event

| Callback      | Function                                                                                                             |
| ------------- | -------------------------------------------------------------------------------------------------------------------- |
| `onLoaded`    | call when ad loaded                                                                                                  |
| `onLoadError` | call when load ad failed. return Error code                                                                          |
| `onShowed`    | call when ad showed                                                                                                  |
| `onHidden`    | call when ad hidden/closed (supports rewarded ads). Returns detailed `PubstarReward` JSON string (`type`, `amount`). |
| `onShowError` | call when show ad failed. return Error code                                                                          |

#### Example

```C#
PubStar.LoadAndShow(
  "Interstitial Ad ID",
  onLoaded: () => Debug.Log($"[GAME] Interstitial Loaded"),
  onLoadError: err => Debug.LogError($"[GAME] Interstitial Load error: {err}"),
  onShowed: () => Debug.Log($"[GAME] Interstitial Showed"),
  onHidden: payloadJson => Debug.Log($"[GAME] Interstitial Hidden payload: {payloadJson}"),
  onShowError: err => Debug.LogError($"[GAME] Interstitial Show error: {err}")
);
```

### PubStar Banner Ads

Load ad then show ad, using for Banner ad and Native ad.

#### API

| Props      | Function                                                                                                             |
| ------------- | -------------------------------------------------------------------------------------------------------------------- |
| `onLoaded`    | call when ad loaded                                                                                                  |
| `onLoadError` | call when load ad failed. return Error code                                                                          |
| `onShowed`    | call when ad showed                                                                                                  |
| `onHidden`    | call when ad hidden/closed (supports rewarded ads). Returns detailed `PubstarReward` JSON string (`type`, `amount`). |
| `onShowError` | call when show ad failed. return Error code                                                                          |

#### Example

```C#
BannerView banner = new BannerView(
    bannerAdID,
    AdSize.Medium,
    AdPosition.Center);
banner.OnLoaded += () =>
{
    Debug.Log("[GAME] Banner Ad was Loaded");
};
banner.OnShowed += () =>
{
    Debug.Log("[GAME] Banner Ad was Showed");
};
banner.Show();
```

### PubStar Native Ads

Load video ad then show video ad, using for Video ad.

#### API

| Props      | Function                                                                                                             |
| ------------- | -------------------------------------------------------------------------------------------------------------------- |
| `onLoaded`    | call when ad loaded                                                                                                  |
| `onLoadError` | call when load ad failed. return Error code                                                                          |
| `onShowed`    | call when ad showed                                                                                                  |
| `onHidden`    | call when ad hidden/closed (supports rewarded ads). Returns detailed `PubstarReward` JSON string (`type`, `amount`). |
| `onShowError` | call when show ad failed. return Error code                                                                          |

#### Example

```C#
NativeView native = new NativeView(
    nativeAdID,
    AdSize.Medium,
    AdPosition.Bottom);
native.OnLoaded += () =>
{
    Debug.Log("[GAME] Native Ad was Loaded");
};
native.OnShowed += () =>
{
    Debug.Log("[GAME] Native Ad was Showed");
};
native.Show();
```

## Release Notes

See the [CHANGELOG.md](https://github.com/pubstar-io/PubStar-Unity-SDK/blob/main/CHANGELOG.md).

## ID Test AD

```C#
App ID : pub-app-id-1233
Banner Id : 1233/99228313580
Native ID : 1233/99228313581
Interstitial ID : 1233/99228313582
Open ID : 1233/99228313583
Rewarded ID : 1233/99228313584
Video ID : 1233/99228313585
```

## Support

Email: developer@tqcsolution.com

Raise an issue on GitHub for bugs or feature requests.

## License

Pubstar is released under the [Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/).

License agreement is available at [LICENSE](LICENSE).
