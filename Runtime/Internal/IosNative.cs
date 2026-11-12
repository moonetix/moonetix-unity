#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace MoonetixOfferwall.Internal
{
    /// <summary>Calls the C functions in MoonetixUnityBridge.m, which wrap the Moonetix iOS SDK.</summary>
    internal sealed class IosNative : IMoonetixNative
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void EventCallback(int type, IntPtr code, IntPtr message);

        [DllImport("__Internal")]
        private static extern void MoonetixUnity_SetCallback(EventCallback callback);

        [DllImport("__Internal")]
        private static extern void MoonetixUnity_Init(string appId);

        [DllImport("__Internal")]
        private static extern void MoonetixUnity_Open(string token, string title, bool hasColor, int argb, bool showLogo);

        private static readonly EventCallback s_callback = OnNativeEvent;

        internal IosNative()
        {
            MoonetixUnity_SetCallback(s_callback);
        }

        public void Init(string appId)
        {
            MoonetixUnity_Init(appId);
        }

        public void Open(string token, NativeOptions options)
        {
            MoonetixUnity_Open(token, options.Title ?? string.Empty, options.HasToolbarColor, options.ToolbarArgb, options.ShowLogo);
        }

        [MonoPInvokeCallback(typeof(EventCallback))]
        private static void OnNativeEvent(int type, IntPtr code, IntPtr message)
        {
            string name = type == 1 ? "opened" : type == 2 ? "closed" : "error";
            Moonetix.HandleNativeEvent(name, Utf8(code), Utf8(message));
        }

        private static string Utf8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }
            int length = 0;
            while (Marshal.ReadByte(ptr, length) != 0)
            {
                length++;
            }
            var bytes = new byte[length];
            Marshal.Copy(ptr, bytes, 0, length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
#endif
