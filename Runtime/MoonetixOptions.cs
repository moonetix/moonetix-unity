using UnityEngine;

namespace MoonetixOfferwall
{
    /// <summary>
    /// Appearance options for the offerwall screen.
    /// </summary>
    /// <remarks>
    /// By default the top bar shows the Moonetix mark with the title and colors from the Design tab of
    /// your Moonetix dashboard, loaded automatically. Set an option here only to override it.
    /// </remarks>
    /// <example>
    /// <code>
    /// Moonetix.Open(token, new MoonetixOptions(title: "Earn coins", toolbarColor: new Color32(0x00, 0x4A, 0xBB, 0xFF)));
    /// </code>
    /// </example>
    public sealed class MoonetixOptions
    {
        /// <summary>Creates appearance options. Every value is optional.</summary>
        /// <param name="title">Text shown in the top bar. <c>null</c> uses the title from your Design tab, or "Earn rewards".</param>
        /// <param name="toolbarColor">Color of the top bar. <c>null</c> uses your Design tab. Transparency is ignored.</param>
        /// <param name="showLogo">Whether to show the Moonetix mark next to the title.</param>
        public MoonetixOptions(string title = null, Color? toolbarColor = null, bool showLogo = true)
        {
            Title = title;
            ToolbarColor = toolbarColor;
            ShowLogo = showLogo;
        }

        /// <summary>
        /// Text shown in the top bar. <c>null</c> or empty (default) uses the title from your Design tab,
        /// or "Earn rewards" when none is set.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Color of the top bar. <c>null</c> (default) uses the background color from your Design tab, or a
        /// neutral light or dark bar that follows the system theme. The title and close icon always keep a
        /// readable contrast. Transparency is ignored.
        /// </summary>
        public Color? ToolbarColor { get; set; }

        /// <summary>Whether to show the Moonetix mark next to the title. Default <c>true</c>.</summary>
        public bool ShowLogo { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "MoonetixOptions(Title: " + (Title ?? "null") + ", ToolbarColor: " + (ToolbarColor.HasValue ? ToolbarColor.Value.ToString() : "null") + ", ShowLogo: " + ShowLogo + ")";
        }
    }
}
