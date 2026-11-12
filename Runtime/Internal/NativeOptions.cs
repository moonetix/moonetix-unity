using UnityEngine;

namespace MoonetixOfferwall.Internal
{
    /// <summary>
    /// <see cref="MoonetixOptions"/> as sent to the native SDKs: no nulls, the color as a 32-bit ARGB value.
    /// </summary>
    internal struct NativeOptions
    {
        /// <summary>The title, or an empty string for the title from the Design tab.</summary>
        public string Title;

        public bool HasToolbarColor;

        /// <summary>Opaque ARGB color, for example 0xFF004ABB. Only valid when <see cref="HasToolbarColor"/> is true.</summary>
        public int ToolbarArgb;

        public bool ShowLogo;

        internal static NativeOptions From(MoonetixOptions options)
        {
            var result = new NativeOptions { Title = string.Empty, ShowLogo = true };
            if (options == null)
            {
                return result;
            }
            if (!string.IsNullOrEmpty(options.Title) && options.Title.Trim().Length > 0)
            {
                result.Title = options.Title;
            }
            if (options.ToolbarColor.HasValue)
            {
                result.HasToolbarColor = true;
                result.ToolbarArgb = ToOpaqueArgb(options.ToolbarColor.Value);
            }
            result.ShowLogo = options.ShowLogo;
            return result;
        }

        /// <summary>Converts a color to an opaque ARGB integer, rounding each channel like <see cref="Color32"/>.</summary>
        internal static int ToOpaqueArgb(Color color)
        {
            uint r = Channel(color.r);
            uint g = Channel(color.g);
            uint b = Channel(color.b);
            return unchecked((int)(0xFF000000u | (r << 16) | (g << 8) | b));
        }

        private static uint Channel(float value)
        {
            if (float.IsNaN(value))
            {
                return 0;
            }
            return (uint)Mathf.Round(Mathf.Clamp01(value) * 255f);
        }
    }
}
