using System.IO;
using NUnit.Framework;
using MoonetixOfferwall.Editor;

namespace MoonetixOfferwall.Tests
{
    public class EditorStepTests
    {
        private const string Unity6Settings =
            "pluginManagement {\n" +
            "    repositories {\n" +
            "        gradlePluginPortal()\n" +
            "        google()\n" +
            "        mavenCentral()\n" +
            "    }\n" +
            "}\n\n" +
            "include ':launcher', ':unityLibrary'\n\n\n" +
            "dependencyResolutionManagement {\n" +
            "    repositoriesMode.set(RepositoriesMode.PREFER_SETTINGS)\n" +
            "    repositories {\n" +
            "        google()\n" +
            "        mavenCentral()\n" +
            "        flatDir {\n" +
            "            dirs \"${project(':unityLibrary').projectDir}/libs\"\n" +
            "        }\n" +
            "    }\n" +
            "}\n";

        private const string LibraryBuild =
            "apply plugin: 'com.android.library'\n" +
            "apply from: '../shared/keepUnitySymbols.gradle'\n\n" +
            "dependencies {\n" +
            "    implementation fileTree(dir: 'libs', include: ['*.jar'])\n" +
            "}\n\n" +
            "android {\n" +
            "    defaultConfig {\n" +
            "        consumerProguardFiles 'proguard-unity.txt'\n" +
            "    }\n" +
            "}\n";

        [Test]
        public void RepositoryGoesIntoDependencyResolutionManagement()
        {
            string patched = MoonetixGradlePatcher.AddRepositoryToSettings(Unity6Settings, false);
            string line = "        maven { url \"https://moonetix.com/maven\"; content { includeGroup \"com.moonetix\" } }\n";
            StringAssert.Contains("    repositories {\n" + line + "        google()", patched);
            int drm = patched.IndexOf("dependencyResolutionManagement");
            Assert.Greater(patched.IndexOf("https://moonetix.com/maven"), drm);
            Assert.AreEqual(1, Count(patched, "moonetix.com/maven"));
            StringAssert.StartsWith("pluginManagement {\n    repositories {\n        gradlePluginPortal()", patched);
        }

        [Test]
        public void RepositoryPatchIsIdempotent()
        {
            string once = MoonetixGradlePatcher.AddRepositoryToSettings(Unity6Settings, false);
            Assert.AreEqual(once, MoonetixGradlePatcher.AddRepositoryToSettings(once, false));
        }

        [Test]
        public void KotlinSettingsGetKotlinSyntax()
        {
            string kts = "dependencyResolutionManagement {\n    repositories {\n        google()\n    }\n}\n";
            StringAssert.Contains("maven { url = uri(\"https://moonetix.com/maven\"); content { includeGroup(\"com.moonetix\") } }", MoonetixGradlePatcher.AddRepositoryToSettings(kts, true));
        }

        [Test]
        public void SettingsWithoutResolutionBlockAreLeftToTheFallback()
        {
            Assert.IsNull(MoonetixGradlePatcher.AddRepositoryToSettings("include ':launcher', ':unityLibrary'\n", false));
            Assert.IsNull(MoonetixGradlePatcher.AddRepositoryToSettings(null, false));
        }

        [Test]
        public void SingleLineRepositoriesBlock()
        {
            string patched = MoonetixGradlePatcher.AddRepositoryToSettings("dependencyResolutionManagement { repositories { google() } }\n", false);
            Assert.AreEqual("dependencyResolutionManagement { repositories { maven { url \"https://moonetix.com/maven\"; content { includeGroup \"com.moonetix\" } }; google() } }\n", patched);
        }

        [Test]
        public void AllprojectsFallback()
        {
            string root = "allprojects {\n    repositories {\n        google()\n    }\n}\n";
            StringAssert.Contains("    repositories {\n        maven { url \"https://moonetix.com/maven\"", MoonetixGradlePatcher.AddRepositoryToAllprojects(root, false));
            Assert.IsNull(MoonetixGradlePatcher.AddRepositoryToAllprojects("buildscript {}\n", false));
        }

        [Test]
        public void DependencyGoesIntoTheTopLevelBlock()
        {
            string patched = MoonetixGradlePatcher.AddDependency(LibraryBuild, false);
            StringAssert.Contains("dependencies {\n    implementation 'com.moonetix:offerwall:2.0.1'\n    implementation fileTree", patched);
            Assert.AreEqual(patched, MoonetixGradlePatcher.AddDependency(patched, false));
        }

        [Test]
        public void DependencyBlockIsCreatedWhenMissing()
        {
            string patched = MoonetixGradlePatcher.AddDependency("apply plugin: 'com.android.library'", false);
            Assert.AreEqual("apply plugin: 'com.android.library'\n\ndependencies {\n    implementation 'com.moonetix:offerwall:2.0.1'\n}\n", patched);
            StringAssert.Contains("implementation(\"com.moonetix:offerwall:2.0.1\")", MoonetixGradlePatcher.AddDependency("plugins {}\n", true));
        }

        [Test]
        public void ExistingMoonetixDependencyIsKept()
        {
            string custom = "dependencies {\n    implementation 'com.moonetix:offerwall:1.2.0'\n}\n";
            Assert.AreEqual(custom, MoonetixGradlePatcher.AddDependency(custom, false));
        }

        [Test]
        public void KeepRuleIsAddedOnce()
        {
            string once = MoonetixGradlePatcher.AddKeepRule("-keep class com.unity3d.** { *; }");
            Assert.AreEqual("-keep class com.unity3d.** { *; }\n-keep class com.moonetix.unity.** { *; }\n", once);
            Assert.AreEqual(once, MoonetixGradlePatcher.AddKeepRule(once));
        }

        [TestCase("12.0", false)]
        [TestCase("12.4", false)]
        [TestCase("13.0", true)]
        [TestCase("13", true)]
        [TestCase("17.2", true)]
        [TestCase("", true)]
        [TestCase(null, true)]
        public void IosDeploymentTarget(string version, bool supported)
        {
            Assert.AreEqual(supported, MoonetixIosVersion.IsSupported(version));
        }

        [Test]
        public void PackageShipsTheIosFramework()
        {
            string path = MoonetixIosBuild.FindFramework();
            Assert.IsNotNull(path, "Native~/iOS/MoonetixOfferwall.xcframework not found");
            Assert.IsTrue(File.Exists(Path.Combine(path, "Info.plist")));
            Assert.IsTrue(File.Exists(Path.Combine(path, "ios-arm64/MoonetixOfferwall.framework/Headers/MoonetixOfferwall-Swift.h")));
            Assert.IsTrue(File.Exists(Path.Combine(path, "ios-arm64_x86_64-simulator/MoonetixOfferwall.framework/MoonetixOfferwall")));
        }

        private static int Count(string text, string part)
        {
            int n = 0;
            for (int i = text.IndexOf(part); i >= 0; i = text.IndexOf(part, i + part.Length))
            {
                n++;
            }
            return n;
        }
    }
}
