# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

PubStar SDK Unity

All notable changes to this project will be documented in this file.

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
