#if UNITY_ANDROID
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace MoonetixOfferwall.Editor
{
    /// <summary>
    /// After Unity generates the Android Gradle project: adds the Moonetix Maven repository (for the
    /// com.moonetix group only), the Moonetix Android SDK dependency and the R8 keep rule for the bridge.
    /// Nothing to set up by hand, also for exported projects and custom Gradle templates.
    /// </summary>
    internal sealed class MoonetixAndroidGradleStep : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder
        {
            get { return 100; }
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string library = path;
            string root = Path.GetDirectoryName(Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            bool repositoryAdded = PatchFile(Path.Combine(root, "settings.gradle"), t => MoonetixGradlePatcher.AddRepositoryToSettings(t, false))
                || PatchFile(Path.Combine(root, "settings.gradle.kts"), t => MoonetixGradlePatcher.AddRepositoryToSettings(t, true))
                || PatchFile(Path.Combine(root, "build.gradle"), t => MoonetixGradlePatcher.AddRepositoryToAllprojects(t, false))
                || PatchFile(Path.Combine(root, "build.gradle.kts"), t => MoonetixGradlePatcher.AddRepositoryToAllprojects(t, true));
            if (!repositoryAdded)
            {
                Debug.LogWarning("[Moonetix] Could not add the Moonetix Maven repository to the Gradle project. Add maven { url \"" + MoonetixGradlePatcher.MavenUrl + "\" } to the dependencyResolutionManagement repositories of your settings.gradle.");
            }

            bool dependencyAdded = PatchFile(Path.Combine(library, "build.gradle"), t => MoonetixGradlePatcher.AddDependency(t, false))
                || PatchFile(Path.Combine(library, "build.gradle.kts"), t => MoonetixGradlePatcher.AddDependency(t, true));
            if (!dependencyAdded)
            {
                Debug.LogWarning("[Moonetix] Could not add the Moonetix Android SDK to unityLibrary. Add implementation '" + MoonetixGradlePatcher.Coordinates + "' to its dependencies.");
            }

            string proguard = Path.Combine(library, "proguard-unity.txt");
            string current = File.Exists(proguard) ? File.ReadAllText(proguard) : null;
            if (current == null)
            {
                Debug.LogWarning("[Moonetix] unityLibrary/proguard-unity.txt not found. If you minify, keep com.moonetix.unity.** in your ProGuard rules.");
            }
            else
            {
                string updated = MoonetixGradlePatcher.AddKeepRule(current);
                if (updated != current)
                {
                    File.WriteAllText(proguard, updated);
                }
            }
        }

        private static bool PatchFile(string file, System.Func<string, string> patch)
        {
            if (!File.Exists(file))
            {
                return false;
            }
            string text = File.ReadAllText(file);
            string updated = patch(text);
            if (updated == null)
            {
                return false;
            }
            if (updated != text)
            {
                File.WriteAllText(file, updated);
            }
            return true;
        }
    }
}
#endif
