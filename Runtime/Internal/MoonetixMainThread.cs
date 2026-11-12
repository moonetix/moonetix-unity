using System;
using System.Threading;
using UnityEngine;

namespace MoonetixOfferwall.Internal
{
    /// <summary>Runs event callbacks on the Unity main thread, in a later frame.</summary>
    internal static class MoonetixMainThread
    {
        private static SynchronizationContext s_context;

        /// <summary>Replaces the dispatcher in tests. Receives every callback instead of the main thread queue.</summary>
        internal static Action<Action> PostOverride;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Capture()
        {
            s_context = SynchronizationContext.Current;
        }

        internal static void Post(Action action)
        {
            if (action == null)
            {
                return;
            }
            Action<Action> over = PostOverride;
            if (over != null)
            {
                over(action);
                return;
            }
            SynchronizationContext context = s_context ?? SynchronizationContext.Current;
            if (context != null)
            {
                context.Post(Run, action);
            }
            else
            {
                Run(action);
            }
        }

        private static void Run(object state)
        {
            try
            {
                ((Action)state)();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
