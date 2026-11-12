using NUnit.Framework;
using MoonetixOfferwall.Internal;

namespace MoonetixOfferwall.Tests
{
    public class ValidationTests
    {
        [TestCase("eeee35416ef2e85e9b4d8203", true)]
        [TestCase("0123456789abcdef01234567", true)]
        [TestCase("EEEE35416EF2E85E9B4D8203", false)]
        [TestCase("eeee35416ef2e85e9b4d820", false)]
        [TestCase("eeee35416ef2e85e9b4d82033", false)]
        [TestCase("eeee35416ef2e85e9b4d820g", false)]
        [TestCase(" eeee35416ef2e85e9b4d8203", false)]
        [TestCase("eeee35416ef2e85e9b4d8203\n", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void AppIdFormat(string appId, bool valid)
        {
            Assert.AreEqual(valid, MoonetixValidation.IsValidAppId(appId));
        }

        [Test]
        public void TokenFormat()
        {
            string body43 = new string('a', 41) + "-_";
            Assert.IsTrue(MoonetixValidation.IsValidToken("sst_" + body43));
            Assert.IsTrue(MoonetixValidation.IsValidToken(Samples.Token));
            Assert.IsFalse(MoonetixValidation.IsValidToken("sst_" + new string('a', 42)));
            Assert.IsFalse(MoonetixValidation.IsValidToken("sst_" + new string('a', 44)));
            Assert.IsFalse(MoonetixValidation.IsValidToken("sst_" + new string('a', 42) + "="));
            Assert.IsFalse(MoonetixValidation.IsValidToken("sst_" + new string('a', 42) + "+"));
            Assert.IsFalse(MoonetixValidation.IsValidToken("sst_" + new string('a', 42) + "/"));
            Assert.IsFalse(MoonetixValidation.IsValidToken("SST_" + body43));
            Assert.IsFalse(MoonetixValidation.IsValidToken("tok_" + body43));
            Assert.IsFalse(MoonetixValidation.IsValidToken("sst_" + body43 + "\n"));
            Assert.IsFalse(MoonetixValidation.IsValidToken(""));
            Assert.IsFalse(MoonetixValidation.IsValidToken(null));
        }

        [Test]
        public void CleanTrimsWhitespaceAndNull()
        {
            Assert.AreEqual("abc", MoonetixValidation.Clean("  abc\n\t"));
            Assert.AreEqual(string.Empty, MoonetixValidation.Clean(null));
        }

        [Test]
        public void MessagesMatchTheNativeSdks()
        {
            StringAssert.StartsWith("The app ID passed to Moonetix.Init() is invalid.", MoonetixValidation.InvalidAppIdMessage);
            StringAssert.EndsWith("It must be the 24-character lowercase hex ID from your Moonetix dashboard.", MoonetixValidation.InvalidAppIdMessage);
            Assert.AreEqual("The session token is malformed. Pass the \"token\" value returned by POST /v1/session.", MoonetixValidation.InvalidTokenMessage);
            Assert.AreEqual("Call Moonetix.Init(appId) before Moonetix.Open().", MoonetixValidation.NotInitializedMessage);
            Assert.AreEqual("The Moonetix offerwall is not available on this platform.", MoonetixValidation.UnsupportedPlatformMessage);
        }
    }
}
