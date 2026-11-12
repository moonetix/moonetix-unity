using System.Text.RegularExpressions;

namespace MoonetixOfferwall.Internal
{
    /// <summary>Input checks and messages. Same formats as the Android and iOS SDKs (^[a-f0-9]{24}$ and ^sst_[A-Za-z0-9_-]{43}$, whole string).</summary>
    internal static class MoonetixValidation
    {
        private static readonly Regex AppIdPattern = new Regex(@"\A[a-f0-9]{24}\z", RegexOptions.CultureInvariant);
        private static readonly Regex TokenPattern = new Regex(@"\Asst_[A-Za-z0-9_-]{43}\z", RegexOptions.CultureInvariant);

        internal const string InvalidAppIdMessage =
            "The app ID passed to Moonetix.Init() is invalid. It must be the 24-character lowercase hex ID from your Moonetix dashboard.";

        internal const string NotInitializedMessage = "Call Moonetix.Init(appId) before Moonetix.Open().";

        internal const string InvalidTokenMessage =
            "The session token is malformed. Pass the \"token\" value returned by POST /v1/session.";

        internal const string EditorMessage =
            "The Moonetix offerwall opens only on Android and iOS devices, not in the Unity Editor. The app ID and token have the right format.";

        internal const string UnsupportedPlatformMessage = "The Moonetix offerwall is not available on this platform.";

        internal static bool IsValidAppId(string appId)
        {
            return appId != null && AppIdPattern.IsMatch(appId);
        }

        internal static bool IsValidToken(string token)
        {
            return token != null && TokenPattern.IsMatch(token);
        }

        /// <summary>Trims like the native SDKs (Kotlin trim / Swift whitespacesAndNewlines).</summary>
        internal static string Clean(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }
    }
}
