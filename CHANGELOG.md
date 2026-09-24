# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

PubStar SDK Unity

All notable changes to this project will be documented in this file.

### [1.6.2] - 2026-09-24

- **The `.unitypackage` now contains the native bridges.** Releases 1.5.0, 1.6.0 and 1.6.1 exported only `Assets/PubStar` — the C# layer — and none of the native code it calls into: no Android library (`PubStarUnityBridge.java`, and the gradle dependency on the PubStar Android SDK) and no iOS bridge (`PubstarUnityBridge.mm` and the Swift wrapper). Imported on its own, as the README describes, the package could not work: the Android bridge class did not exist and the iOS `DllImport` symbols could not link. The release now also exports `Assets/Plugins/Android/PubStar.androidlib` and `Assets/Plugins/iOS/PubStar`, and the release workflow refuses to publish a package missing them.
- Bumped the native SDKs to `1.6.2` (iOS `Pubstar ~> 1.6.2`, Android `io.pubstar.mobile:ads:1.6.2`), bringing the reporting work from that release: `app_crash` / `app_session` on their own endpoint, a `screen` dimension on every metric, the `load_time` metric, `display_time` measuring time-to-show for full-screen formats, and one `impression` per `sdk_request` per placement.
- **iOS behaviour change:** a missing `io.pubstar.key` in `Info.plist` now fails at initialization instead of silently falling back to the built-in debug App ID, matching Android.
- Fixed iOS initialization failing with `-7` (`NO_INIT`) when it runs before the scene is foreground-active. The bridge looked up the host view controller once, on the first call, and a nil result stuck for the life of the app; it is now retried, and init waits for the scene to become active instead of failing.
- The iOS build postprocess no longer overwrites `io.pubstar.key` and `GADApplicationIdentifier` in `Info.plist` on every build. It adds its placeholders only when the keys are missing, so a publisher's real values survive an "Append" build — as the Android postprocess already did.
- `NativeCustomConfig.cs` now has a committed `.meta`. Without one, Unity generated a new GUID on every build, so each release shipped the file under a different GUID (1.6.0 `0c6da0e8…`, 1.6.1 `83c196de…`) and importing a newer package over an older one could leave a duplicate. The committed GUID is 1.6.1's, so upgrading from 1.6.1 is clean.
- The UPM copy of the iOS postprocess (`Editor/`) no longer adds `pod 'Pubstar', :path => '../Frameworks'` — a local development path — nor writes the App ID `pub-app-id-1277` into `Info.plist`; it now matches the postprocess shipped in the `.unitypackage`.

### [1.6.1] - 2026-06-09

- Fixed iOS initialization ignoring the app's `io.pubstar.key` from `Info.plist`. The iOS bridge forced `setIsDebug(true)`, which made the native SDK initialize with the built-in debug App ID instead of the publisher's real App ID, so the init config never matched the app's ad unit IDs.

### [1.6.0] - 2026-06-18

- **Custom Native** — render native ads with your own layout via `NativeCustomConfig.Builder` (pass the result to `NativeView`).
- **Video (IMA)** — show video ads via `VideoView` with a `media` URL (in/out-stream through Google IMA).
- **PubStar Mediation** — run PubStar as a network inside Google AdMob and AppLovin MAX.
- **Firebase / GA4 ad-revenue reporting** — per-impression value/currency events to support ROAS campaigns (requires Firebase Analytics in the host app).
- Updated README with full Usage guide (Initialize / Load / Show / LoadAndShow, Banner, Native, Custom Native, Video IMA) aligned with the Android and iOS SDKs.
- Fixed Android post-process incorrectly logging an informational message via `Debug.LogError`, which caused the build to be reported as failed.

### [1.5.0] - 2026-01-22

- **OpenRTB (ORTB) Bidding Adapter**
  - Added a dedicated ORTB Adapter that supports ad auctioning based on the IAB OpenRTB 2.6 specification:
    https://github.com/InteractiveAdvertisingBureau/openrtb2.x

  - Supported ad formats:
    - Banner

    - Interstitial

    - Rewarded

  - ORTB bidding is handled entirely inside the SDK via the adapter layer, without requiring any additional client-side configuration.

- Summary
  - All bidding logic is encapsulated inside the SDK and driven by server configuration.

    This release significantly reduces integration complexity while enabling advanced auction-based advertising workflows.

## [1.3.1] - 2026-25-12

### 🎉 Initial Public Release

First stable public release of **PubStar SDK for Unity**, supporting both **Android** and **iOS** platforms.

---

### ✨ Added

#### Core SDK
- PubStar SDK initialization API with success and error callbacks.
- Unified C# API layer for Android and iOS native integrations.
- Structured ad lifecycle callbacks with type-safe event handling.

#### Ad Formats
- **Banner Ads and Native Ads**
  - Multiple sizes support.
  - Flexible screen positioning.
  - Native layout rendering via platform SDKs.
- **Interstitial Ads**
- **Rewarded Ads**
- **App Open Ads**

#### API Methods
- `Initialize()`
- `Load()`
- `Show()`
- `LoadAndShow()`
- `BannerView` class
- `NativeView` class

#### Unity Integration
- Compatible with **Unity 2021.3 LTS** and later.
- Distributed via GitHub.
- Included **Sample Demo Scene** to test all ad formats.

---

### 📱 Platform Support

- **Android**
  - Minimum API level: **26**
  - Native integration via Gradle / Maven dependencies.
- **iOS**
  - Minimum iOS version: **13.0**
  - Native integration via CocoaPods.

---

### ⚙️ Configuration

#### Android
- Support for `io.pubstar.key` via `AndroidManifest.xml`.

#### iOS
- Support for:
  - `io.pubstar.key`
  - `GADApplicationIdentifier`
  - `NSUserTrackingUsageDescription`
  - `SKAdNetworkItems`

---

### 🧪 Samples

- Added **PubStar Demo** sample:
  - Demonstrates Banner, Native, Interstitial, Rewarded, and App Open ads.
  - Located under `Samples~/Driver`.

---

### 📄 Documentation

- Initial version of:
  - `README.md`
  - Installation & configuration guides.
  - API usage examples for all ad formats.

---

### ⚠️ Known Limitations

- No editor simulation for ads (ads only render on real devices).
- Ad behavior depends on network availability and platform SDK responses.

---

### 🔒 License

- Released under the **Apache License 2.0**.

---

[1.3.1]: https://pubstar.io/
