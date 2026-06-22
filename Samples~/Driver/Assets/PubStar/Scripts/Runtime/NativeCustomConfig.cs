using UnityEngine;
using System;

[Serializable] // Bắt buộc để JsonUtility hoạt động
public class NativeCustomConfig
{
    // Lưu ý: JsonUtility không serialize các thuộc tính { get; } 
    // nên ta dùng field để đảm bảo dữ liệu xuất ra JSON
    public string layoutName;
    public string advertiserTextViewId;
    public string iconImageViewId;
    public string titleTextViewId;
    public string mediaContentViewGroupId;
    public string bodyTextViewId;
    public string callToActionButtonId;
    public string loadingViewName;
    public string ctaColorHex;

    private NativeCustomConfig(Builder builder)
    {
        layoutName = builder.LayoutName;
        advertiserTextViewId = builder.AdvertiserTextViewId;
        iconImageViewId = builder.IconImageViewId;
        titleTextViewId = builder.TitleTextViewId;
        mediaContentViewGroupId = builder.MediaContentViewGroupId;
        bodyTextViewId = builder.BodyTextViewId;
        callToActionButtonId = builder.CallToActionButtonId;
        loadingViewName = builder.LoadingViewName;
        ctaColorHex = builder.CtaColorHex;
    }

    public class Builder
    {
        public string LayoutName { get; private set; }
        public string AdvertiserTextViewId { get; private set; }
        public string IconImageViewId { get; private set; }
        public string TitleTextViewId { get; private set; }
        public string MediaContentViewGroupId { get; private set; }
        public string BodyTextViewId { get; private set; }
        public string CallToActionButtonId { get; private set; }
        public string LoadingViewName { get; private set; }
        public string CtaColorHex { get; private set; }

        public Builder(string layoutName) { this.LayoutName = layoutName; }

        public Builder SetAdvertiserTextViewId(string id) { AdvertiserTextViewId = id; return this; }
        public Builder SetIconImageViewId(string id) { IconImageViewId = id; return this; }
        public Builder SetTitleTextViewId(string id) { TitleTextViewId = id; return this; }
        public Builder SetMediaContentViewGroupId(string id) { MediaContentViewGroupId = id; return this; }
        public Builder SetBodyTextViewId(string id) { BodyTextViewId = id; return this; }
        public Builder SetCallToActionButtonId(string id) { CallToActionButtonId = id; return this; }
        public Builder SetLoadingViewName(string name) { LoadingViewName = name; return this; }
        public Builder SetCtaColorHex(string hex) { CtaColorHex = hex; return this; }

        // Chỉnh sửa hàm Build trả về JSON string
        public string Build()
        {
            NativeCustomConfig config = new NativeCustomConfig(this);
            return JsonUtility.ToJson(config);
        }
    }
}