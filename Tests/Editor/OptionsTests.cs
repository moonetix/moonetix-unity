using NUnit.Framework;
using MoonetixOfferwall.Internal;
using UnityEngine;

namespace MoonetixOfferwall.Tests
{
    public class OptionsTests
    {
        [Test]
        public void NullOptionsUseTheDesignTab()
        {
            NativeOptions o = NativeOptions.From(null);
            Assert.AreEqual(string.Empty, o.Title);
            Assert.IsFalse(o.HasToolbarColor);
            Assert.IsTrue(o.ShowLogo);
        }

        [Test]
        public void DefaultsMatchTheNativeSdks()
        {
            var options = new MoonetixOptions();
            Assert.IsNull(options.Title);
            Assert.IsNull(options.ToolbarColor);
            Assert.IsTrue(options.ShowLogo);
            NativeOptions o = NativeOptions.From(options);
            Assert.AreEqual(string.Empty, o.Title);
            Assert.IsFalse(o.HasToolbarColor);
            Assert.IsTrue(o.ShowLogo);
        }

        [Test]
        public void MapsTitleColorAndLogo()
        {
            NativeOptions o = NativeOptions.From(new MoonetixOptions(title: "Earn coins", toolbarColor: new Color32(0x0B, 0x5F, 0xFF, 0xFF), showLogo: false));
            Assert.AreEqual("Earn coins", o.Title);
            Assert.IsTrue(o.HasToolbarColor);
            Assert.AreEqual(unchecked((int)0xFF0B5FFF), o.ToolbarArgb);
            Assert.IsFalse(o.ShowLogo);
        }

        [Test]
        public void ObjectInitializerWorks()
        {
            NativeOptions o = NativeOptions.From(new MoonetixOptions { Title = "Coins", ShowLogo = false });
            Assert.AreEqual("Coins", o.Title);
            Assert.IsFalse(o.HasToolbarColor);
            Assert.IsFalse(o.ShowLogo);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public void BlankTitleUsesTheDesignTab(string title)
        {
            Assert.AreEqual(string.Empty, NativeOptions.From(new MoonetixOptions(title: title)).Title);
        }

        [Test]
        public void ColorIgnoresAlpha()
        {
            Assert.AreEqual(unchecked((int)0xFF004ABB), NativeOptions.ToOpaqueArgb(new Color32(0x00, 0x4A, 0xBB, 0x10)));
            Assert.AreEqual(unchecked((int)0xFF000000), NativeOptions.ToOpaqueArgb(new Color(0, 0, 0, 0)));
        }

        [Test]
        public void ColorClampsAndRounds()
        {
            Assert.AreEqual(unchecked((int)0xFFFF0080), NativeOptions.ToOpaqueArgb(new Color(2f, -1f, 0.5f, 1f)));
            Assert.AreEqual(unchecked((int)0xFF00FFFF), NativeOptions.ToOpaqueArgb(new Color(float.NaN, 1f, 1f, 1f)));
        }

        [Test]
        public void EveryByteSurvivesTheColorRoundTrip()
        {
            for (int v = 0; v < 256; v++)
            {
                byte b = (byte)v;
                Color color = new Color32(b, b, b, 255);
                int expected = unchecked((int)(0xFF000000u | ((uint)v << 16) | ((uint)v << 8) | (uint)v));
                Assert.AreEqual(expected, NativeOptions.ToOpaqueArgb(color), "channel value " + v);
            }
        }

        [Test]
        public void ToStringShowsTheValues()
        {
            StringAssert.Contains("Title: Earn", new MoonetixOptions(title: "Earn").ToString());
            StringAssert.Contains("ShowLogo: True", new MoonetixOptions().ToString());
        }
    }
}
