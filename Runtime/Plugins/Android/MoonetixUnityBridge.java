package com.moonetix.unity;

import android.app.Activity;

import com.moonetix.offerwall.Moonetix;
import com.moonetix.offerwall.MoonetixError;
import com.moonetix.offerwall.MoonetixListener;
import com.moonetix.offerwall.MoonetixOptions;

/**
 * Unity side of the Moonetix offerwall plugin. Calls the Moonetix Android SDK on the main thread and
 * forwards its events to C# as plain strings (never null, so Unity can match the proxy method).
 */
public final class MoonetixUnityBridge {

    /** Implemented in C# by an AndroidJavaProxy. */
    public interface Callback {
        void onEvent(String type, String code, String message);
    }

    private static volatile Callback callback;

    private static final MoonetixListener FORWARDER = new MoonetixListener() {
        @Override
        public void onOpened() {
            send("opened", "", "");
        }

        @Override
        public void onClosed() {
            send("closed", "", "");
        }

        @Override
        public void onError(MoonetixError error) {
            send("error", error.getCode().name(), error.getMessage());
        }
    };

    private MoonetixUnityBridge() {
    }

    public static void setCallback(Callback cb) {
        callback = cb;
    }

    public static void init(final Activity activity, final String appId) {
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                Moonetix.init(activity.getApplicationContext(), appId);
                Moonetix.setListener(FORWARDER);
            }
        });
    }

    public static void open(final Activity activity, final String token, final String title, final boolean hasColor, final int color, final boolean showLogo) {
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                Moonetix.setListener(FORWARDER);
                try {
                    String cleanTitle = title == null || title.trim().isEmpty() ? null : title;
                    Integer toolbarColor = hasColor ? Integer.valueOf(color) : null;
                    Moonetix.open(activity, token, new MoonetixOptions(cleanTitle, toolbarColor, showLogo));
                } catch (RuntimeException e) {
                    send("error", "UNKNOWN", "Could not open the offerwall: " + e.getClass().getSimpleName() + ".");
                }
            }
        });
    }

    private static void send(String type, String code, String message) {
        Callback cb = callback;
        if (cb != null) {
            cb.onEvent(type, code == null ? "" : code, message == null ? "" : message);
        }
    }
}
