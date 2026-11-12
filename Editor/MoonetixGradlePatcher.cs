using System.Text.RegularExpressions;

namespace MoonetixOfferwall.Editor
{
    /// <summary>
    /// Text changes to the Gradle project that Unity generates for Android: the Moonetix Maven repository
    /// (only for the com.moonetix group), the Android SDK dependency and the R8 keep rule for the bridge.
    /// Every change is idempotent.
    /// </summary>
    internal static class MoonetixGradlePatcher
    {
        internal const string MavenUrl = "https://moonetix.com/maven";
        internal const string Group = "com.moonetix";
        internal const string AndroidSdkVersion = "2.0.1";
        internal const string Coordinates = "com.moonetix:offerwall:" + AndroidSdkVersion;
        internal const string KeepRule = "-keep class com.moonetix.unity.** { *; }";

        private const string GroovyRepository = "maven { url \"" + MavenUrl + "\"; content { includeGroup \"" + Group + "\" } }";
        private const string KotlinRepository = "maven { url = uri(\"" + MavenUrl + "\"); content { includeGroup(\"" + Group + "\") } }";

        private static readonly Regex RepositoriesOpen = new Regex(@"\brepositories\s*\{", RegexOptions.CultureInvariant);
        private static readonly Regex TopLevelDependencies = new Regex(@"(?m)^dependencies\s*\{[^\n]*\n", RegexOptions.CultureInvariant);
        private static readonly Regex OurDependency = new Regex(@"com\.moonetix:offerwall:", RegexOptions.CultureInvariant);

        /// <summary>
        /// Adds the repository to <c>dependencyResolutionManagement { repositories { } }</c> of settings.gradle.
        /// Returns null when the file has no such block (Gradle then uses the project repositories).
        /// </summary>
        internal static string AddRepositoryToSettings(string settings, bool kotlinDsl)
        {
            if (settings == null)
            {
                return null;
            }
            if (settings.Contains(MavenUrl))
            {
                return settings;
            }
            int block = settings.IndexOf("dependencyResolutionManagement", System.StringComparison.Ordinal);
            if (block < 0)
            {
                return null;
            }
            Match open = RepositoriesOpen.Match(settings, block);
            if (!open.Success)
            {
                return null;
            }
            return InsertAfterBrace(settings, open.Index + open.Length, kotlinDsl ? KotlinRepository : GroovyRepository);
        }

        /// <summary>
        /// Adds the repository to <c>allprojects { repositories { } }</c> of the root build.gradle, for Gradle
        /// projects without repositories in settings.gradle. Returns null when there is no such block.
        /// </summary>
        internal static string AddRepositoryToAllprojects(string rootBuild, bool kotlinDsl)
        {
            if (rootBuild == null)
            {
                return null;
            }
            if (rootBuild.Contains(MavenUrl))
            {
                return rootBuild;
            }
            int block = rootBuild.IndexOf("allprojects", System.StringComparison.Ordinal);
            if (block < 0)
            {
                return null;
            }
            Match open = RepositoriesOpen.Match(rootBuild, block);
            if (!open.Success)
            {
                return null;
            }
            return InsertAfterBrace(rootBuild, open.Index + open.Length, kotlinDsl ? KotlinRepository : GroovyRepository);
        }

        /// <summary>Adds the Android SDK to the top-level dependencies block of unityLibrary/build.gradle.</summary>
        internal static string AddDependency(string build, bool kotlinDsl)
        {
            if (build == null)
            {
                return null;
            }
            if (OurDependency.IsMatch(build))
            {
                return build;
            }
            string line = kotlinDsl
                ? "    implementation(\"" + Coordinates + "\")\n"
                : "    implementation '" + Coordinates + "'\n";
            Match open = TopLevelDependencies.Match(build);
            if (open.Success)
            {
                int at = open.Index + open.Length;
                return build.Substring(0, at) + line + build.Substring(at);
            }
            string tail = build.EndsWith("\n", System.StringComparison.Ordinal) ? string.Empty : "\n";
            return build + tail + "\ndependencies {\n" + line + "}\n";
        }

        /// <summary>Adds the keep rule for the bridge class to unityLibrary/proguard-unity.txt.</summary>
        internal static string AddKeepRule(string proguard)
        {
            string text = proguard ?? string.Empty;
            if (text.Contains(KeepRule))
            {
                return text;
            }
            string tail = text.Length == 0 || text.EndsWith("\n", System.StringComparison.Ordinal) ? string.Empty : "\n";
            return text + tail + KeepRule + "\n";
        }

        private static string InsertAfterBrace(string text, int afterBrace, string statement)
        {
            int lineEnd = text.IndexOf('\n', afterBrace);
            string rest = lineEnd >= 0 ? text.Substring(afterBrace, lineEnd - afterBrace) : text.Substring(afterBrace);
            if (rest.Trim().Length > 0)
            {
                return text.Substring(0, afterBrace) + " " + statement + ";" + text.Substring(afterBrace);
            }
            string indent = "        ";
            if (lineEnd >= 0)
            {
                Match next = Regex.Match(text.Substring(lineEnd + 1), @"^[ \t]*(?=\S)");
                if (next.Success && next.Length > 0)
                {
                    indent = next.Value;
                }
                return text.Substring(0, lineEnd + 1) + indent + statement + "\n" + text.Substring(lineEnd + 1);
            }
            return text.Substring(0, afterBrace) + "\n" + indent + statement + "\n" + text.Substring(afterBrace);
        }
    }
}
