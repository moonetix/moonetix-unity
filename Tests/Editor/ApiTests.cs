using System;
using NUnit.Framework;
using MoonetixOfferwall.Internal;
using UnityEngine;
using UnityEngine.TestTools;

namespace MoonetixOfferwall.Tests
{
    public class ApiTests
    {
        private EventRecorder _rec;
        private FakeNative _native;

        [SetUp]
        public void SetUp()
        {
            _rec = new EventRecorder();
            _native = new FakeNative();
            Moonetix.Native = _native;
        }

        [TearDown]
        public void TearDown()
        {
            _rec.Dispose();
        }

        [Test]
        public void OpenBeforeInitReportsNotInitialized()
        {
            Moonetix.Open(Samples.Token);
            _rec.Flush();
            Assert.AreEqual(new[] { "error" }, _rec.Events.ToArray());
            Assert.AreEqual(new MoonetixError(MoonetixErrorCode.NotInitialized, "Call Moonetix.Init(appId) before Moonetix.Open()."), _rec.Errors[0]);
            Assert.IsEmpty(_native.Calls);
        }

        [Test]
        public void InvalidAppIdIsNotStoredAndOpenReportsIt()
        {
            LogAssert.Expect(LogType.Warning, "[Moonetix] " + MoonetixValidation.InvalidAppIdMessage);
            Moonetix.Init("EEEE35416EF2E85E9B4D8203");
            Moonetix.Open(Samples.Token);
            _rec.Flush();
            Assert.AreEqual(MoonetixErrorCode.InvalidAppId, _rec.Errors[0].Code);
            Assert.AreEqual(MoonetixValidation.InvalidAppIdMessage, _rec.Errors[0].Message);
            Assert.IsEmpty(_native.Calls);
        }

        [Test]
        public void ValidInitAfterAnInvalidOneClearsTheError()
        {
            LogAssert.Expect(LogType.Warning, "[Moonetix] " + MoonetixValidation.InvalidAppIdMessage);
            Moonetix.Init("nope");
            Moonetix.Init(Samples.AppId);
            Moonetix.Open(Samples.Token);
            _rec.Flush();
            Assert.IsEmpty(_rec.Errors);
            Assert.AreEqual(new[] { "init", "open" }, _native.Calls.ToArray());
        }

        [Test]
        public void InitTrimsAndPassesTheAppId()
        {
            Moonetix.Init("  " + Samples.AppId + "\n");
            Assert.AreEqual(Samples.AppId, _native.InitAppId);
        }

        [Test]
        public void MalformedTokenIsReportedWithoutCallingNative()
        {
            Moonetix.Init(Samples.AppId);
            Moonetix.Open("sst_short");
            _rec.Flush();
            Assert.AreEqual(new MoonetixError(MoonetixErrorCode.InvalidToken, MoonetixValidation.InvalidTokenMessage), _rec.Errors[0]);
            Assert.AreEqual(new[] { "init" }, _native.Calls.ToArray());
        }

        [Test]
        public void OpenPassesTheTrimmedTokenAndOptions()
        {
            Moonetix.Init(Samples.AppId);
            Moonetix.Open(" " + Samples.Token + " ", new MoonetixOptions(title: "Earn coins", toolbarColor: new Color32(0x00, 0x4A, 0xBB, 0xFF), showLogo: false));
            Assert.AreEqual(Samples.Token, _native.OpenToken);
            Assert.AreEqual("Earn coins", _native.OpenOptions.Title);
            Assert.IsTrue(_native.OpenOptions.HasToolbarColor);
            Assert.AreEqual(unchecked((int)0xFF004ABB), _native.OpenOptions.ToolbarArgb);
            Assert.IsFalse(_native.OpenOptions.ShowLogo);
            _rec.Flush();
            Assert.IsEmpty(_rec.Events);
        }

        [Test]
        public void OpenWithoutOptionsUsesDefaults()
        {
            Moonetix.Init(Samples.AppId);
            Moonetix.Open(Samples.Token);
            Assert.AreEqual(string.Empty, _native.OpenOptions.Title);
            Assert.IsFalse(_native.OpenOptions.HasToolbarColor);
            Assert.IsTrue(_native.OpenOptions.ShowLogo);
        }

        [Test]
        public void NativeFailureBecomesUnknownError()
        {
            Moonetix.Init(Samples.AppId);
            _native.OpenFailure = new InvalidOperationException("boom");
            Moonetix.Open(Samples.Token);
            _rec.Flush();
            Assert.AreEqual(MoonetixErrorCode.Unknown, _rec.Errors[0].Code);
            Assert.AreEqual("The offerwall could not be opened: InvalidOperationException.", _rec.Errors[0].Message);
        }

        [Test]
        public void EditorReportsUnsupportedAfterTheChecks()
        {
            Moonetix.Native = null;
            Assert.IsFalse(Moonetix.IsSupported);
            Moonetix.Init(Samples.AppId);
            Moonetix.Open(Samples.Token);
            _rec.Flush();
            Assert.AreEqual(MoonetixErrorCode.Unsupported, _rec.Errors[0].Code);
            Assert.AreEqual("UNSUPPORTED", _rec.Errors[0].CodeName);
            Assert.AreEqual(MoonetixValidation.EditorMessage, _rec.Errors[0].Message);
        }

        [Test]
        public void EditorStillChecksTheTokenFirst()
        {
            Moonetix.Native = null;
            Moonetix.Init(Samples.AppId);
            Moonetix.Open("bad");
            _rec.Flush();
            Assert.AreEqual(MoonetixErrorCode.InvalidToken, _rec.Errors[0].Code);
        }

        [Test]
        public void EditorHasNoNativeBridgeByDefault()
        {
            Moonetix.ResetState();
            Assert.IsNull(Moonetix.Native);
            Assert.IsFalse(Moonetix.IsSupported);
        }

        [Test]
        public void NativeEventsAreQueuedForTheMainThread()
        {
            Moonetix.HandleNativeEvent("opened", "", "");
            Moonetix.HandleNativeEvent("closed", "", "");
            Moonetix.HandleNativeEvent("error", "TOKEN_EXPIRED_OR_USED", "Request a new token.");
            Assert.IsEmpty(_rec.Events);
            Assert.AreEqual(3, _rec.Queue.Count);
            _rec.Flush();
            Assert.AreEqual(new[] { "opened", "closed", "error" }, _rec.Events.ToArray());
            Assert.AreEqual(new MoonetixError(MoonetixErrorCode.TokenExpiredOrUsed, "Request a new token."), _rec.Errors[0]);
        }

        [Test]
        public void UnknownNativeCodesAndEmptyMessagesAreHandled()
        {
            Moonetix.HandleNativeEvent("error", "NEW_CODE", "");
            Moonetix.HandleNativeEvent("something-else", "", "");
            _rec.Flush();
            Assert.AreEqual(1, _rec.Errors.Count);
            Assert.AreEqual(MoonetixErrorCode.Unknown, _rec.Errors[0].Code);
            Assert.AreEqual("The offerwall reported an error.", _rec.Errors[0].Message);
        }

        [Test]
        public void ErrorWithoutHandlerIsLogged()
        {
            Moonetix.ResetState();
            Moonetix.Native = _native;
            Moonetix.Open(Samples.Token);
            LogAssert.Expect(LogType.Warning, "[Moonetix] MoonetixError(NOT_INITIALIZED): " + MoonetixValidation.NotInitializedMessage + " (no OnError handler)");
            _rec.Flush();
        }

        [Test]
        public void ResetForgetsEverything()
        {
            Moonetix.Init(Samples.AppId);
            Moonetix.ResetState();
            Moonetix.Native = _native;
            Moonetix.OnError += e => _rec.Errors.Add(e);
            Moonetix.Open(Samples.Token);
            _rec.Flush();
            Assert.AreEqual(MoonetixErrorCode.NotInitialized, _rec.Errors[0].Code);
        }

        [Test]
        public void VersionIsSet()
        {
            Assert.AreEqual("2.0.1", Moonetix.Version);
        }
    }
}
