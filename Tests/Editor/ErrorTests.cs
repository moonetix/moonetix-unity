using System;
using NUnit.Framework;

namespace MoonetixOfferwall.Tests
{
    public class ErrorTests
    {
        [TestCase(MoonetixErrorCode.NotInitialized, "NOT_INITIALIZED")]
        [TestCase(MoonetixErrorCode.InvalidAppId, "INVALID_APP_ID")]
        [TestCase(MoonetixErrorCode.InvalidToken, "INVALID_TOKEN")]
        [TestCase(MoonetixErrorCode.TokenExpiredOrUsed, "TOKEN_EXPIRED_OR_USED")]
        [TestCase(MoonetixErrorCode.Network, "NETWORK")]
        [TestCase(MoonetixErrorCode.Unsupported, "UNSUPPORTED")]
        [TestCase(MoonetixErrorCode.Unknown, "UNKNOWN")]
        public void CodeNamesMatchTheNativeSdks(MoonetixErrorCode code, string name)
        {
            Assert.AreEqual(name, MoonetixError.NameOf(code));
            Assert.AreEqual(code, MoonetixError.CodeFromName(name));
            Assert.AreEqual(name, new MoonetixError(code, "m").CodeName);
        }

        [Test]
        public void EveryCodeHasAName()
        {
            foreach (MoonetixErrorCode code in Enum.GetValues(typeof(MoonetixErrorCode)))
            {
                Assert.AreEqual(code, MoonetixError.CodeFromName(MoonetixError.NameOf(code)));
            }
        }

        [Test]
        public void UnknownNamesMapToUnknown()
        {
            Assert.AreEqual(MoonetixErrorCode.Unknown, MoonetixError.CodeFromName("SOMETHING_NEW"));
            Assert.AreEqual(MoonetixErrorCode.Unknown, MoonetixError.CodeFromName(""));
            Assert.AreEqual(MoonetixErrorCode.Unknown, MoonetixError.CodeFromName(null));
        }

        [Test]
        public void EqualityAndText()
        {
            var a = new MoonetixError(MoonetixErrorCode.Network, "offline");
            Assert.AreEqual(a, new MoonetixError(MoonetixErrorCode.Network, "offline"));
            Assert.AreNotEqual(a, new MoonetixError(MoonetixErrorCode.Unknown, "offline"));
            Assert.AreEqual(a.GetHashCode(), new MoonetixError(MoonetixErrorCode.Network, "offline").GetHashCode());
            Assert.AreEqual("MoonetixError(NETWORK): offline", a.ToString());
            Assert.AreEqual(string.Empty, new MoonetixError(MoonetixErrorCode.Unknown, null).Message);
        }
    }
}
