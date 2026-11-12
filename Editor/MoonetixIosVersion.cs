using System;

namespace MoonetixOfferwall.Editor
{
    /// <summary>Checks the iOS deployment target against the minimum of the Moonetix iOS SDK.</summary>
    internal static class MoonetixIosVersion
    {
        internal const int MinimumMajor = 13;

        internal const string TooLowMessage =
            "Moonetix Offerwall needs iOS 13.0 or newer. Set Player Settings > iOS > Other Settings > Target minimum iOS Version to 13.0 or higher.";

        /// <summary>True when <paramref name="version"/> (for example "12.0" or "13") is iOS 13 or newer. Empty or unreadable values count as supported.</summary>
        internal static bool IsSupported(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                return true;
            }
            string major = version.Trim().Split('.')[0];
            int value;
            if (!int.TryParse(major, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                return true;
            }
            return value >= MinimumMajor;
        }
    }
}
