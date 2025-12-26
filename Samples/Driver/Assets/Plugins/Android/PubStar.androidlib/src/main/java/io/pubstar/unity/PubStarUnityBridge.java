package io.pubstar.unity;

import android.app.Activity;
import android.content.Context;
import android.graphics.Rect;
import android.graphics.RectF;
import android.os.Looper;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import io.pubstar.mobile.core.api.PubStarAdManager;
import io.pubstar.mobile.core.base.BannerAdRequest;
import io.pubstar.mobile.core.base.NativeAdRequest;
import io.pubstar.mobile.core.interfaces.AdLoaderListener;
import io.pubstar.mobile.core.interfaces.AdShowedListener;
import io.pubstar.mobile.core.interfaces.InitAdListener;
import io.pubstar.mobile.core.interfaces.PubStarAdController;
import io.pubstar.mobile.core.models.ErrorCode;
import io.pubstar.mobile.core.models.RewardModel;

import android.os.Handler;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.view.ViewParent;
import android.view.ViewTreeObserver;
import android.widget.FrameLayout;


import com.unity3d.player.UnityPlayer;

import java.util.concurrent.ConcurrentHashMap;

class AdPreset {
    public static final String AdPresetNone = "None";
    public static final String AdPresetTop = "Top";
    public static final String AdPresetBottom = "Bottom";
    public static final String AdPresetCenter = "Center";
    public static final String AdPresetTopLeft = "TopLeft";
    public static final String AdPresetTopRight = "TopRight";
    public static final String AdPresetBottomLeft = "BottomLeft";
    public static final String AdPresetBottomRight = "BottomRight";

    static int dpToPx(Context ctx, float dp) {
        return Math.round(TypedValue.applyDimension(
                TypedValue.COMPLEX_UNIT_DIP,
                dp,
                ctx.getResources().getDisplayMetrics()
        ));
    }

    public static RectF calculateAdFramePx(
            Context ctx,
            Rect boundsPx,
            float xDp,
            float yDp,
            float widthDp,
            float heightDp,
            String positionString
    ) {
        float screenWidthPx = boundsPx.width();
        float screenHeightPx = boundsPx.height();

        float widthPx = dpToPx(ctx, widthDp);
        if (widthDp == -1.0f) {
            widthPx = screenWidthPx;
        } else if (widthDp > 0.0f && widthDp < 1.0f) {
            widthPx = screenWidthPx * widthDp;
        }

        float heightPx = dpToPx(ctx, heightDp);
        if (heightDp == -1.0f) {
            heightPx = screenHeightPx;
        } else if (heightDp > 0.0f && heightDp < 1.0f) {
            heightPx = screenHeightPx * heightDp;
        }

        float xPx = dpToPx(ctx, xDp);
        float yPx = dpToPx(ctx, yDp);

        String position = (positionString == null ? AdPresetNone : positionString)
                .trim()
                .toLowerCase();

        if (position.equals(AdPresetBottom.toLowerCase())) {
            yPx = screenHeightPx - heightPx - dpToPx(ctx, yDp);
        } else if (position.equals(AdPresetTop.toLowerCase())) {
            yPx = dpToPx(ctx, yDp);
        } else if (position.equals(AdPresetCenter.toLowerCase())) {
            xPx = (screenWidthPx - widthPx) / 2.0f + dpToPx(ctx, xDp);
            yPx = (screenHeightPx - heightPx) / 2.0f + dpToPx(ctx, yDp);
        } else if (position.equals(AdPresetTopLeft.toLowerCase())) {
            xPx = dpToPx(ctx, xDp);
            yPx = dpToPx(ctx, yDp);
        } else if (position.equals(AdPresetTopRight.toLowerCase())) {
            xPx = screenWidthPx - widthPx - dpToPx(ctx, xDp);
            yPx = dpToPx(ctx, yDp);
        } else if (position.equals(AdPresetBottomLeft.toLowerCase())) {
            xPx = dpToPx(ctx, xDp);
            yPx = screenHeightPx - heightPx - dpToPx(ctx, yDp);
        } else if (position.equals(AdPresetBottomRight.toLowerCase())) {
            xPx = screenWidthPx - widthPx - dpToPx(ctx, xDp);
            yPx = screenHeightPx - heightPx - dpToPx(ctx, yDp);
        }

        float left = boundsPx.left + xPx;
        float top = boundsPx.top + yPx;

        return new RectF(left, top, left + widthPx, top + heightPx);
    }
}

public class PubStarUnityBridge {
    private static final String TAG = "PubStarUnityBridge";
    private static final Context appContext = UnityPlayer.currentActivity;
    private static String gameObjectName;
    private static final PubStarAdController adController = PubStarAdManager.getAdController();

    private static void runOnMainThread(Runnable block) {
        if (Looper.myLooper() == Looper.getMainLooper()) {
            block.run();
        } else {
            new Handler(Looper.getMainLooper()).post(block);
        }
    }

    private static void runWhenLaidOut(final View view, final Runnable block) {
        if (view.getWidth() > 0 && view.getHeight() > 0) {
            block.run();
            return;
        }

        view.getViewTreeObserver().addOnGlobalLayoutListener(new ViewTreeObserver.OnGlobalLayoutListener() {
            @Override
            public void onGlobalLayout() {
                if (view.getWidth() > 0 && view.getHeight() > 0) {
                    view.getViewTreeObserver().removeOnGlobalLayoutListener(this);
                    block.run();
                }
            }
        });
    }

    private static void pubstarUnitySendMessage(String method, String message) {
        if (gameObjectName == null || gameObjectName.trim().isEmpty()) {
            Log.d(TAG, "[PubStarUnityBridge][pubstarUnitySendMessage] gameObjectName is null/empty, skip send. method=" + method);
            return;
        }

        runOnMainThread(() -> UnityPlayer.UnitySendMessage(gameObjectName, method, message == null ? "" : message));
    }

    public static void initialize(String objectName) {
        Log.d(TAG, "[PubStarUnityBridge][initialize] is called, appId=" + objectName);

        if (objectName == null || objectName.trim().isEmpty()) {
            Log.d(TAG, "[PubStarUnityBridge][initialize] objectName is null/empty, skip send");
            return;
        }

        gameObjectName = objectName;

        if (appContext == null) {
            Log.d(TAG, "[PubStarUnityBridge][initialize] appContext is null");
            pubstarUnitySendMessage("OnPubstarInitError", "context is null");
            return;
        }

        InitAdListener listener = new InitAdListener() {
            @Override
            public void onError(@NonNull ErrorCode errorCode) {
                Log.d(TAG, "[PubStarUnityBridge][initialize][onError] code: "
                        + errorCode.getCode() + "- name: " + errorCode.name());

                String payload = errorCode.getCode() + "|" + errorCode.name();
                Log.d(TAG, "[PubStarUnityBridge][initialize][onError] " + payload);
                pubstarUnitySendMessage("OnPubstarInitError", payload);
            }

            @Override
            public void onDone() {
                Log.d(TAG, "[PubStarUnityBridge][initialize][onDone] is called");
                pubstarUnitySendMessage("OnPubstarInitDone", "");
            }
        };

        runOnMainThread(() -> {
            PubStarAdManager.getInstance()
                    .setInitAdListener(listener)
                    .init(appContext);
        });
    }

    public static void load(String placementId) {
        Log.d(TAG, "[PubStarUnityBridge][load] is called with adId: " + placementId);

        adController.load(
                appContext,
                placementId,
                new AdLoaderListener() {
                    @Override
                    public void onLoaded() {
                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onLoaded] is called.");

                        pubstarUnitySendMessage("OnPubstarLoaded", placementId);
                    }

                    @Override
                    public void onError(@NonNull ErrorCode errorCode) {

                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onError] is called with errorName: "
                                + errorCode.name() + "|" + errorCode.getCode());

                        pubstarUnitySendMessage("OnPubstarLoadError", String.valueOf(errorCode.getCode()));
                    }
                }
        );
    }

    public static void show(String placementId) {
        Log.d(TAG, "[PubStarUnityBridge][show] is called with adId: " + placementId);


        adController.show(
                appContext,
                placementId,
                null,
                new AdShowedListener() {
                    @Override
                    public void onError(@NonNull ErrorCode errorCode) {
                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onAdHide] is called with errorName: "
                                + errorCode.name() + "|" + errorCode.getCode());

                        pubstarUnitySendMessage("OnPubstarShowError", String.valueOf(errorCode.getCode()));
                    }

                    @Override
                    public void onAdShowed() {
                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onAdShowed] is called.");

                        pubstarUnitySendMessage("OnPubstarAdShowed", placementId);
                    }

                    @Override
                    public void onAdHide(@Nullable RewardModel rewardModel) {
                        String reward = "{}";
                        if (rewardModel != null) {
                            Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onError] is called with rewardModel: "
                                    + rewardModel.getType() + "|" + rewardModel.getAmount());
                            reward = "{ \"type\": "
                                    + String.valueOf(rewardModel.getType()) +
                                    ", \"amount\": " + String.valueOf(rewardModel.getAmount())
                                    + " }";
                        } else {
                            Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onError] is called with rewardModel");
                        }

                        pubstarUnitySendMessage("OnPubstarAdHidden", reward);
                    }
                }
        );
    }

    public static void loadAndShow(String placementId) {
        Log.d(TAG, "[PubStarUnityBridge][loadAndShow] is called with adId: " + placementId);

        adController.loadAndShow(
                appContext,
                placementId,
                null,
                new AdLoaderListener() {
                    @Override
                    public void onLoaded() {
                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onLoaded] is called.");

                        pubstarUnitySendMessage("OnPubstarLoaded", placementId);
                    }

                    @Override
                    public void onError(@NonNull ErrorCode errorCode) {

                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onError] is called with errorName: "
                                + errorCode.name() + "|" + errorCode.getCode());

                        pubstarUnitySendMessage("OnPubstarLoadError", String.valueOf(errorCode.getCode()));
                    }
                },
                new AdShowedListener() {
                    @Override
                    public void onError(@NonNull ErrorCode errorCode) {
                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onAdHide] is called with errorName: "
                                + errorCode.name() + "|" + errorCode.getCode());

                        pubstarUnitySendMessage("OnPubstarShowError", String.valueOf(errorCode.getCode()));
                    }

                    @Override
                    public void onAdShowed() {
                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onAdShowed] is called.");

                        pubstarUnitySendMessage("OnPubstarAdShowed", placementId);
                    }

                    @Override
                    public void onAdHide(@Nullable RewardModel rewardModel) {
                        String reward = "{}";
                        if (rewardModel != null) {
                            Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onAdHide] is called with rewardModel: "
                                    + rewardModel.getType() + "|" + rewardModel.getAmount());
                            reward = "{ \"type\": "
                                    + String.valueOf(rewardModel.getType()) +
                                    ", \"amount\": " + String.valueOf(rewardModel.getAmount())
                                    + " }";
                        } else {
                            Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onAdHide] is called with rewardModel");
                        }

                        pubstarUnitySendMessage("OnPubstarAdHidden", reward);
                    }
                }
        );
    }

    private static FrameLayout sOverlayRoot;

    private static FrameLayout getOrCreateRootView(Activity activity) {
        if (sOverlayRoot != null) return sOverlayRoot;

        ViewGroup content = activity.findViewById(android.R.id.content);
        if (content == null) {
            Log.e(TAG, "[createAdView] rootView is null (android.R.id.content not found)");
            return null;
        }

        FrameLayout overlay = new FrameLayout(activity);
        overlay.setLayoutParams(new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
        ));
        overlay.setClickable(false);
        overlay.setFocusable(false);

        content.addView(overlay);
        sOverlayRoot = overlay;

        return sOverlayRoot;
    }

    private static int dpToPx(Context ctx, float dp) {
        return Math.round(TypedValue.applyDimension(
                TypedValue.COMPLEX_UNIT_DIP,
                dp,
                ctx.getResources().getDisplayMetrics()
        ));
    }

    private static final ConcurrentHashMap<String, FrameLayout> sContainers = new ConcurrentHashMap<>();

    public static void createAdView(
            String viewId,
            float xDp,
            float yDp,
            float widthDp,
            float heightDp,
            String position
    ) {
        Log.d(TAG, "[PubStarUnityBridge][createAdView] is called with viewId=" + viewId + ", position=" + position);

        if (viewId == null || viewId.trim().isEmpty()) return;

        final String vid = viewId;
        final String positionString = (position != null ? position : "None");

        runOnMainThread(() -> {
            if (appContext == null) {
                Log.e(TAG, "[PubStarUnityBridge][createAdView] currentActivity is null");
                return;
            }

            Activity activity = (Activity) appContext;

            FrameLayout rootView = getOrCreateRootView(activity);
            if (rootView == null) {
                Log.e(TAG, "[PubStarUnityBridge][createAdView] rootView is null");
                return;
            }

            Log.d(TAG, "[PubStarUnityBridge][createAdView] xDp=" + xDp + " yDp=" + yDp
                    + " widthDp=" + widthDp + " heightDp=" + heightDp
                    + " position=" + positionString);

            runWhenLaidOut(rootView, () -> {
                Rect bounds = new Rect(rootView.getLeft(), rootView.getTop(), rootView.getWidth(), rootView.getHeight());

                Log.d(TAG, "[PubStarUnityBridge][createAdView] value of bounds. bounds.left=" + bounds.left + " bounds.top=" + bounds.top
                        + " bounds.width()=" + bounds.width() + " bounds.height()=" + bounds.height()
                        + " position=" + positionString);

                RectF frame = AdPreset.calculateAdFramePx(
                        activity,
                        bounds,
                        xDp, yDp, widthDp, heightDp,
                        positionString
                );

                Log.d(TAG, "[PubStarUnityBridge][createAdView] xPx=" + frame.left + " yPx=" + frame.top
                        + " widthPx=" + frame.width() + " heightPx=" + frame.height()
                        + " position=" + positionString);

                FrameLayout.LayoutParams layoutParams = new FrameLayout.LayoutParams(
                        Math.round(frame.width()),
                        Math.round(frame.height())
                );
                layoutParams.leftMargin = Math.round(frame.left);
                layoutParams.topMargin = Math.round(frame.top);
                layoutParams.gravity = Gravity.TOP | Gravity.LEFT;

                FrameLayout container = sContainers.get(vid);
                if (container == null) {
                    container = new FrameLayout(activity);
                    container.setLayoutParams(layoutParams);
                     container.setBackgroundColor(0x00000000); // transparent
//                    container.setBackgroundColor(Color.RED);
                    container.setClickable(true);
                    container.setFocusable(true);

                    rootView.addView(container);
                    sContainers.put(vid, container);

                    Log.d(TAG, "[PubStarUnityBridge][createAdView] Created ad view '" + vid + "'");
                    return;
                }

                container.setLayoutParams(layoutParams);

                if (container.getParent() == null) {
                    rootView.addView(container);
                }

                Log.d(TAG, "[createAdView] Updated ad view '" + vid + "'");
            });
        });
    }

    private static NativeAdRequest.Type extractNativeSize(String size) {
        if (size == null) {
            return NativeAdRequest.Type.Small;
        }

        return switch (size) {
            case "small" -> NativeAdRequest.Type.Small;
            case "medium" -> NativeAdRequest.Type.Medium;
            case "big" -> NativeAdRequest.Type.Big;
            default -> NativeAdRequest.Type.Small;
        };
    }

    private static BannerAdRequest.AdTag extractBannerSize(String tag) {
        if (tag == null) {
            return BannerAdRequest.AdTag.Small;
        }

        return switch (tag) {
            case "small" -> BannerAdRequest.AdTag.Small;
            case "medium" -> BannerAdRequest.AdTag.Medium;
            case "big" -> BannerAdRequest.AdTag.Big;
            case "collapsible" -> BannerAdRequest.AdTag.Collapsible;
            default -> BannerAdRequest.AdTag.Small;
        };
    }

    public static void showBannerInView(
            String viewId,
            String placementId,
            String size
    ) {
        Log.d(TAG, "[PubStarUnityBridge][showBannerInView] is called with viewId=" + viewId + ", placementId=" + placementId + ", size=" + size);

        runOnMainThread(() -> {
            if (appContext == null) {
                Log.e(TAG, "[PubStarUnityBridge][showBannerInView] currentActivity is null");
                pubstarUnitySendMessage("OnPubstarLoadError", "context is null");
                return;
            }

            Log.d(TAG, "[PubStarUnityBridge][showBannerInView] sContainers is " + sContainers.size());
            Log.d(TAG, "[PubStarUnityBridge][showBannerInView] viewId is " + viewId);

            if (!sContainers.containsKey(viewId)) {
                Log.d(TAG, "[PubStarUnityBridge][showBannerInView] not container key: " + viewId);
                pubstarUnitySendMessage("OnPubstarLoadError", "view is null");
                return;
            }

            ViewGroup adView = sContainers.get(viewId);

            Log.d(TAG, "[PubStarUnityBridge][showBannerInView] asdView is " + adView.toString());

            BannerAdRequest request = new BannerAdRequest.Builder(appContext)
                    .withView(adView)
                    .tag(extractBannerSize(size))
                    .adLoaderListener(
                            new AdLoaderListener() {
                                @Override
                                public void onLoaded() {
                                    Log.d(TAG, "[PubStarUnityBridge][showBannerInView][adLoaderListener][onLoaded] is called");

                                    pubstarUnitySendMessage("OnPubstarLoaded", viewId);
                                }

                                @Override
                                public void onError(@NonNull ErrorCode errorCode) {
                                    Log.e(TAG, "[PubStarUnityBridge][showBannerInView][adLoaderListener][onError] error with name: " + errorCode.name() + ", code: " + errorCode.getCode());

                                    pubstarUnitySendMessage("OnPubstarLoadError", String.valueOf(errorCode.getCode()));
                                }
                            }
                    )
                    .adShowedListener(
                            new AdShowedListener() {
                                @Override
                                public void onAdShowed() {
                                    Log.d(TAG, "[PubStarUnityBridge][showBannerInView][AdShowedListener][onAdShowed] is called");

                                    pubstarUnitySendMessage("OnPubstarAdShowed", viewId);
                                }

                                @Override
                                public void onAdHide(@Nullable RewardModel rewardModel) {
                                    Log.d(TAG, "[PubStarUnityBridge][showBannerInView][AdShowedListener][onAdHide] reward has type: " + rewardModel.getType() + ", amount: " + rewardModel.getAmount());
                                    String reward = "{}";
                                    if (rewardModel != null) {
                                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][adAdHide] is called with rewardModel: "
                                                + rewardModel.getType() + "|" + rewardModel.getAmount());
                                        reward = "{ \"type\": "
                                                + String.valueOf(rewardModel.getType()) +
                                                ", \"amount\": " + String.valueOf(rewardModel.getAmount())
                                                + " }";
                                    } else {
                                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onError] is called with rewardModel");
                                    }

                                    pubstarUnitySendMessage("OnPubstarAdHidden", reward);
                                }

                                @Override
                                public void onError(@NonNull ErrorCode errorCode) {
                                    Log.e(TAG, "[PubStarUnityBridge][showBannerInView][AdShowedListener][onError] error with name: " + errorCode.name() + ", code: " + errorCode.getCode());

                                    pubstarUnitySendMessage("OnPubstarShowError", String.valueOf(errorCode.getCode()));
                                }
                            }
                    )
                    .build();

            adController.loadAndShow(
                    placementId,
                    request
            );
        });
    }

    public static void showNativeInView(
            String viewId,
            String placementId,
            String size
    ) {
        Log.d(TAG, "[PubStarUnityBridge][showNativeInView] is called with viewId=" + viewId + ", placementId=" + placementId + ", size=" + size);


        runOnMainThread(() -> {
            if (appContext == null) {
                Log.e(TAG, "[PubStarUnityBridge][showNativeInView] currentActivity is null");

                pubstarUnitySendMessage("OnPubstarLoadError", "context is null");
                return;
            }

            Log.d(TAG, "[PubStarUnityBridge][showNativeInView] sContainers is " + sContainers.size());
            Log.d(TAG, "[PubStarUnityBridge][showNativeInView] viewId is " + viewId);

            if (!sContainers.containsKey(viewId)) {
                Log.d(TAG, "[PubStarUnityBridge][showNativeInView] not container key: " + viewId);

                pubstarUnitySendMessage("OnPubstarLoadError", "view is null");
                return;
            }

            ViewGroup tempView = sContainers.get(viewId);

            Log.d(TAG, "[PubStarUnityBridge][showNativeInView] tempView is " + tempView.toString());

            NativeAdRequest request = new NativeAdRequest.Builder(appContext)
                    .withView(tempView)
                    .sizeType(extractNativeSize(size))
                    .adLoaderListener(
                            new AdLoaderListener() {
                                @Override
                                public void onLoaded() {
                                    Log.d(TAG, "[PubStarUnityBridge][showNativeInView][adLoaderListener][onLoaded] is called");

                                    pubstarUnitySendMessage("OnPubstarLoaded", viewId);
                                }

                                @Override
                                public void onError(@NonNull ErrorCode errorCode) {
                                    Log.e(TAG, "[PubStarUnityBridge][showNativeInView][adLoaderListener][onError] error with name: " + errorCode.name() + ", code: " + errorCode.getCode());

                                    pubstarUnitySendMessage("OnPubstarLoadError", String.valueOf(errorCode.getCode()));
                                }
                            }
                    )
                    .adShowedListener(
                            new AdShowedListener() {
                                @Override
                                public void onAdShowed() {
                                    Log.d(TAG, "[PubStarUnityBridge][showNativeInView][AdShowedListener][onAdShowed] is called");

                                    pubstarUnitySendMessage("OnPubstarAdShowed", viewId);
                                }

                                @Override
                                public void onAdHide(@Nullable RewardModel rewardModel) {
                                    Log.d(TAG, "[PubStarUnityBridge][showNativeInView][AdShowedListener][onAdHide] reward has type: " + rewardModel.getType() + ", amount: " + rewardModel.getAmount());

                                    String reward = "{}";
                                    if (rewardModel != null) {
                                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onAdHide] is called with rewardModel: "
                                                + rewardModel.getType() + "|" + rewardModel.getAmount());
                                        reward = "{ \"type\": "
                                                + String.valueOf(rewardModel.getType()) +
                                                ", \"amount\": " + String.valueOf(rewardModel.getAmount())
                                                + " }";
                                    } else {
                                        Log.d(TAG, "[PubStarUnityBridge][loadAndShow][onAdHide] is called with rewardModel");
                                    }

                                    pubstarUnitySendMessage("OnPubstarAdHidden", reward);
                                }

                                @Override
                                public void onError(@NonNull ErrorCode errorCode) {
                                    Log.e(TAG, "[PubStarUnityBridge][showNativeInView][AdShowedListener][onError] error with name: " + errorCode.name() + ", code: " + errorCode.getCode());

                                    pubstarUnitySendMessage("OnPubstarShowError", String.valueOf(errorCode.getCode()));
                                }
                            }
                    )
                    .build();

            adController.loadAndShow(
                    placementId,
                    request
            );
        });
    }

    public static void destroyAdView(String viewId) {
        if(viewId == null || viewId.trim().isEmpty()) {
            return;
        }

        runOnMainThread(() -> {
            ViewGroup adView = sContainers.get(viewId);

            if(adView == null) {
                return;
            }

            ViewParent parent = adView.getParent();
            if (parent instanceof ViewGroup) {
                ((ViewGroup) parent).removeView(adView);
            }

            sContainers.remove(viewId);
        });
    }
}
