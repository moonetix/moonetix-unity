using System;
using System.Collections.Generic;
using MoonetixOfferwall.Internal;

namespace MoonetixOfferwall.Tests
{
    internal sealed class FakeNative : IMoonetixNative
    {
        public readonly List<string> Calls = new List<string>();
        public string InitAppId;
        public string OpenToken;
        public NativeOptions OpenOptions;
        public Exception OpenFailure;

        public void Init(string appId)
        {
            Calls.Add("init");
            InitAppId = appId;
        }

        public void Open(string token, NativeOptions options)
        {
            Calls.Add("open");
            if (OpenFailure != null)
            {
                throw OpenFailure;
            }
            OpenToken = token;
            OpenOptions = options;
        }
    }

    /// <summary>Collects the events and runs queued main-thread callbacks on demand.</summary>
    internal sealed class EventRecorder : IDisposable
    {
        public readonly List<Action> Queue = new List<Action>();
        public readonly List<string> Events = new List<string>();
        public readonly List<MoonetixError> Errors = new List<MoonetixError>();

        public EventRecorder()
        {
            Moonetix.ResetState();
            MoonetixMainThread.PostOverride = Queue.Add;
            Moonetix.OnOpened += Opened;
            Moonetix.OnClosed += Closed;
            Moonetix.OnError += Failed;
        }

        private void Opened()
        {
            Events.Add("opened");
        }

        private void Closed()
        {
            Events.Add("closed");
        }

        private void Failed(MoonetixError error)
        {
            Events.Add("error");
            Errors.Add(error);
        }

        public void Flush()
        {
            while (Queue.Count > 0)
            {
                Action next = Queue[0];
                Queue.RemoveAt(0);
                next();
            }
        }

        public void Dispose()
        {
            MoonetixMainThread.PostOverride = null;
            Moonetix.ResetState();
        }
    }

    internal static class Samples
    {
        public const string AppId = "eeee35416ef2e85e9b4d8203";
        public static readonly string Token = "sst_" + new string('A', 20) + "-_" + new string('z', 21);
    }
}
