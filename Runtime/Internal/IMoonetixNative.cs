namespace MoonetixOfferwall.Internal
{
    /// <summary>The platform side of the plugin. Events come back through <see cref="Moonetix.HandleNativeEvent"/>.</summary>
    internal interface IMoonetixNative
    {
        void Init(string appId);

        void Open(string token, NativeOptions options);
    }
}
