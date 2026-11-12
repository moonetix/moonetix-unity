using System;
using MoonetixOfferwall.Internal;
using UnityEngine;

namespace MoonetixOfferwall
{
    /// <summary>
    /// Entry point of the Moonetix offerwall plugin.
    /// </summary>
    /// <remarks>
    /// <para>Your server creates a single-use session token with <c>POST https://api.moonetix.com/v1/session</c>,
    /// and <see cref="Open"/> shows the offerwall with it. Never put your API key or link hash salt in the game.</para>
    /// <para>Problems (not initialized, malformed token, network, expired token, unsupported platform) arrive as
    /// <see cref="OnError"/> events. The methods never throw for them. Every event is raised on the Unity main thread.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// Moonetix.Init("YOUR_APP_ID");
    /// Moonetix.OnClosed += RefreshBalance;
    /// // token = fresh single-use session token fetched from YOUR server
    /// Moonetix.Open(token);
    /// </code>
    /// </example>
    public static class Moonetix
    {
        /// <summary>Version of this plugin.</summary>
        public const string Version = "2.0.1";

        private static string s_appId;
        private static bool s_initRejected;
        private static IMoonetixNative s_native;
        private static bool s_nativeResolved;

        /// <summary>The offerwall finished loading its first page.</summary>
        /// <remarks>On Android your game is paused while the offerwall is open, so this event can arrive when the user returns.</remarks>
        public static event Action OnOpened;

        /// <summary>The offerwall was closed. Raised once per <see cref="Open"/> that showed the offerwall. A good moment to load the user's balance from your server.</summary>
        public static event Action OnClosed;

        /// <summary>Something went wrong. See <see cref="MoonetixError.Code"/>.</summary>
        public static event Action<MoonetixError> OnError;

        /// <summary>Whether the offerwall can open here: true in Android and iOS players, false in the Editor and on other platforms.</summary>
        public static bool IsSupported
        {
            get { return Native != null; }
        }

        /// <summary>
        /// Initializes the plugin with your Moonetix app ID (24 lowercase hex characters, shown in the Integration
        /// tab of your app). Call it once, for example in the <c>Awake</c> of your first scene.
        /// </summary>
        /// <remarks>An invalid value is not stored. Every following <see cref="Open"/> call then reports
        /// <see cref="MoonetixErrorCode.InvalidAppId"/> instead of opening.</remarks>
        public static void Init(string appId)
        {
            string trimmed = MoonetixValidation.Clean(appId);
            if (!MoonetixValidation.IsValidAppId(trimmed))
            {
                s_appId = null;
                s_initRejected = true;
                Debug.LogWarning("[Moonetix] " + MoonetixValidation.InvalidAppIdMessage);
                return;
            }
            s_appId = trimmed;
            s_initRejected = false;
            IMoonetixNative native = Native;
            if (native == null)
            {
                return;
            }
            try
            {
                native.Init(trimmed);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Moonetix] Init failed: " + e.GetType().Name + ": " + e.Message);
            }
        }

        /// <summary>Opens the offerwall full screen.</summary>
        /// <param name="token">A fresh session token (<c>sst_...</c>) that your server got from
        /// <c>POST https://api.moonetix.com/v1/session</c>. Tokens are single-use and expire after 10 minutes,
        /// so request a new one for every call.</param>
        /// <param name="options">Optional top bar settings. By default the top bar shows the Moonetix mark with the
        /// title and colors from the Design tab of your dashboard.</param>
        public static void Open(string token, MoonetixOptions options = null)
        {
            if (s_appId == null)
            {
                if (s_initRejected)
                {
                    EmitError(MoonetixErrorCode.InvalidAppId, MoonetixValidation.InvalidAppIdMessage);
                }
                else
                {
                    EmitError(MoonetixErrorCode.NotInitialized, MoonetixValidation.NotInitializedMessage);
                }
                return;
            }
            string cleanToken = MoonetixValidation.Clean(token);
            if (!MoonetixValidation.IsValidToken(cleanToken))
            {
                EmitError(MoonetixErrorCode.InvalidToken, MoonetixValidation.InvalidTokenMessage);
                return;
            }
            IMoonetixNative native = Native;
            if (native == null)
            {
                EmitError(MoonetixErrorCode.Unsupported, Application.isEditor ? MoonetixValidation.EditorMessage : MoonetixValidation.UnsupportedPlatformMessage);
                return;
            }
            try
            {
                native.Open(cleanToken, NativeOptions.From(options));
            }
            catch (Exception e)
            {
                EmitError(MoonetixErrorCode.Unknown, "The offerwall could not be opened: " + e.GetType().Name + ".");
            }
        }

        /// <summary>Called by the platform bridges, on any thread. <paramref name="type"/> is opened, closed or error.</summary>
        internal static void HandleNativeEvent(string type, string code, string message)
        {
            switch (type)
            {
                case "opened":
                    MoonetixMainThread.Post(() => Raise(OnOpened));
                    break;
                case "closed":
                    MoonetixMainThread.Post(() => Raise(OnClosed));
                    break;
                case "error":
                    EmitError(MoonetixError.CodeFromName(code), string.IsNullOrEmpty(message) ? "The offerwall reported an error." : message);
                    break;
            }
        }

        private static void EmitError(MoonetixErrorCode code, string message)
        {
            var error = new MoonetixError(code, message);
            MoonetixMainThread.Post(() =>
            {
                Action<MoonetixError> handler = OnError;
                if (handler == null)
                {
                    Debug.LogWarning("[Moonetix] " + error + " (no OnError handler)");
                    return;
                }
                handler(error);
            });
        }

        private static void Raise(Action handler)
        {
            if (handler != null)
            {
                handler();
            }
        }

        internal static IMoonetixNative Native
        {
            get
            {
                if (!s_nativeResolved)
                {
                    s_native = CreateNative();
                    s_nativeResolved = true;
                }
                return s_native;
            }
            set
            {
                s_native = value;
                s_nativeResolved = true;
            }
        }

        private static IMoonetixNative CreateNative()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidNative();
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosNative();
#else
            return null;
#endif
        }

        /// <summary>Forgets the app ID, the events and the native bridge. Also runs when Play Mode starts without a domain reload.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetState()
        {
            s_appId = null;
            s_initRejected = false;
            s_native = null;
            s_nativeResolved = false;
            OnOpened = null;
            OnClosed = null;
            OnError = null;
        }
    }
}
