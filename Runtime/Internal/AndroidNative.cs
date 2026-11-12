#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
using UnityEngine.Scripting;

namespace MoonetixOfferwall.Internal
{
    /// <summary>Calls com.moonetix.unity.MoonetixUnityBridge, which wraps the Moonetix Android SDK.</summary>
    internal sealed class AndroidNative : IMoonetixNative
    {
        private const string BridgeClass = "com.moonetix.unity.MoonetixUnityBridge";

        private readonly AndroidJavaClass _bridge;
        private readonly CallbackProxy _callback;

        internal AndroidNative()
        {
            _bridge = new AndroidJavaClass(BridgeClass);
            _callback = new CallbackProxy();
            _bridge.CallStatic("setCallback", _callback);
        }

        public void Init(string appId)
        {
            using (AndroidJavaObject activity = CurrentActivity())
            {
                if (activity == null)
                {
                    Debug.LogWarning("[Moonetix] No Android activity yet. The app ID is checked again when the offerwall opens.");
                    return;
                }
                _bridge.CallStatic("init", activity, appId);
            }
        }

        public void Open(string token, NativeOptions options)
        {
            using (AndroidJavaObject activity = CurrentActivity())
            {
                if (activity == null)
                {
                    Moonetix.HandleNativeEvent("error", "UNKNOWN", "No Android activity is available to open the offerwall.");
                    return;
                }
                _bridge.CallStatic("open", activity, token, options.Title ?? string.Empty, options.HasToolbarColor, options.ToolbarArgb, options.ShowLogo);
            }
        }

        private static AndroidJavaObject CurrentActivity()
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                return player.GetStatic<AndroidJavaObject>("currentActivity");
            }
        }

        /// <summary>Implements MoonetixUnityBridge.Callback. Called on the Android main thread.</summary>
        [Preserve]
        private sealed class CallbackProxy : AndroidJavaProxy
        {
            [Preserve]
            internal CallbackProxy() : base(BridgeClass + "$Callback")
            {
            }

            [Preserve]
            public void onEvent(string type, string code, string message)
            {
                Moonetix.HandleNativeEvent(type, code, message);
            }
        }
    }
}
#endif
