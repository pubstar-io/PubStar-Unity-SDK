# PubStar SDK for Unity - Architecture Context

## 1. Overview
PubStar SDK là một plugin cho Unity, được viết bằng C# để giao tiếp với tầng Native (Android/iOS) nhằm hiển thị và quản lý Ads.

## 2. Directory Structure & File Responsibilities

### 2.1. Editor/Postprocess/
Chứa các script tự động hóa quá trình build của Unity khi xuất ra platform tương ứng.
- `PubStarAndroidPostprocess.cs`: Tự động inject cấu hình PubStar SDK vào `settings.gradle` và `AndroidManifest.xml` khi build Android.
- `PubStarPostprocess.cs`: Tự động inject dependencies vào `Podfile` và cấu hình `Info.plist` khi build iOS.

### 2.2. Runtime/Scripts/
Chứa logic chính của SDK, bao gồm các model, interface và platform-specific implementations.

**Data Models & Callbacks:**
- `AdCallbacks.cs`: Định nghĩa các delegate/callback (ví dụ: OnAdLoaded, OnAdFailed, v.v.) để Unity App lắng nghe sự kiện từ SDK.
- `AdPosition.cs`: Định nghĩa properties setup vị trí hiển thị Ads trên màn hình.
- `AdSize.cs`: Định nghĩa properties setup kích thước của Ads.

**Core Logic & Native Communication:**
- `PubStar.cs`: Điểm entry-point (Facade/Singleton) của SDK. Tầng Application (Unity App) sẽ gọi trực tiếp vào class này để khởi tạo và request Ads.
- `INativeInvoker.cs`: Interface định nghĩa các contract giao tiếp với Native. Giúp decouple logic Unity và logic Native.
- `AndroidNativeInvoker.cs`: Implement `INativeInvoker` sử dụng `AndroidJavaClass` / `AndroidJavaObject` để gọi xuống code Kotlin/Java.
- `IosNativeInvoker.cs`: Implement `INativeInvoker` sử dụng `[DllImport("__Internal")]` để gọi xuống code Swift/Objective-C.