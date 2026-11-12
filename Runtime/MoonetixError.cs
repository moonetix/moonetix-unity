using System;

namespace MoonetixOfferwall
{
    /// <summary>Error categories reported through <see cref="Moonetix.OnError"/>.</summary>
    public enum MoonetixErrorCode
    {
        /// <summary><see cref="Moonetix.Open"/> was called before <see cref="Moonetix.Init"/>.</summary>
        NotInitialized = 0,

        /// <summary>The app ID passed to <see cref="Moonetix.Init"/> is not a 24-character lowercase hex string.</summary>
        InvalidAppId = 1,

        /// <summary>The token is malformed, or the server does not know it (HTTP 404).</summary>
        InvalidToken = 2,

        /// <summary>The token expired (10 minutes) or was already used (HTTP 410). Request a new one from your server.</summary>
        TokenExpiredOrUsed = 3,

        /// <summary>The offerwall could not be reached. The offerwall screen shows a retry button.</summary>
        Network = 4,

        /// <summary>The offerwall is not available here: the Unity Editor or a platform other than Android and iOS.</summary>
        Unsupported = 5,

        /// <summary>Anything else, for example no WebView on the device.</summary>
        Unknown = 6,
    }

    /// <summary>
    /// An error reported by the plugin. The plugin never throws for offerwall problems; every problem
    /// arrives as an <see cref="Moonetix.OnError"/> event with one of these.
    /// </summary>
    public sealed class MoonetixError : IEquatable<MoonetixError>
    {
        /// <summary>Creates an error. The plugin creates these; you only need it for your own tests.</summary>
        public MoonetixError(MoonetixErrorCode code, string message)
        {
            Code = code;
            Message = message ?? string.Empty;
        }

        /// <summary>Machine-readable category of the error.</summary>
        public MoonetixErrorCode Code { get; }

        /// <summary>Human-readable description for logs. It never contains the session token.</summary>
        public string Message { get; }

        /// <summary>The code as the Android and iOS SDKs name it, for example <c>TOKEN_EXPIRED_OR_USED</c>.</summary>
        public string CodeName => NameOf(Code);

        /// <summary>The name the native SDKs use for <paramref name="code"/>, for example <c>INVALID_TOKEN</c>.</summary>
        public static string NameOf(MoonetixErrorCode code)
        {
            switch (code)
            {
                case MoonetixErrorCode.NotInitialized: return "NOT_INITIALIZED";
                case MoonetixErrorCode.InvalidAppId: return "INVALID_APP_ID";
                case MoonetixErrorCode.InvalidToken: return "INVALID_TOKEN";
                case MoonetixErrorCode.TokenExpiredOrUsed: return "TOKEN_EXPIRED_OR_USED";
                case MoonetixErrorCode.Network: return "NETWORK";
                case MoonetixErrorCode.Unsupported: return "UNSUPPORTED";
                default: return "UNKNOWN";
            }
        }

        /// <summary>Maps a native code name such as <c>NETWORK</c> to a <see cref="MoonetixErrorCode"/>. Unknown names map to <see cref="MoonetixErrorCode.Unknown"/>.</summary>
        public static MoonetixErrorCode CodeFromName(string name)
        {
            switch (name)
            {
                case "NOT_INITIALIZED": return MoonetixErrorCode.NotInitialized;
                case "INVALID_APP_ID": return MoonetixErrorCode.InvalidAppId;
                case "INVALID_TOKEN": return MoonetixErrorCode.InvalidToken;
                case "TOKEN_EXPIRED_OR_USED": return MoonetixErrorCode.TokenExpiredOrUsed;
                case "NETWORK": return MoonetixErrorCode.Network;
                case "UNSUPPORTED": return MoonetixErrorCode.Unsupported;
                default: return MoonetixErrorCode.Unknown;
            }
        }

        /// <inheritdoc/>
        public bool Equals(MoonetixError other)
        {
            return other != null && other.Code == Code && other.Message == Message;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as MoonetixError);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Code * 397) ^ Message.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "MoonetixError(" + CodeName + "): " + Message;
        }
    }
}
