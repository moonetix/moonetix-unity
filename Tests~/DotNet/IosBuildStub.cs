using System.IO;
using System.Runtime.CompilerServices;

namespace MoonetixOfferwall.Editor
{
    // Stands in for the UnityEditor-based MoonetixIosBuild: same lookup, rooted at this checkout.
    internal static class MoonetixIosBuild
    {
        internal static string FindFramework()
        {
            string root = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourcePath()), "..", ".."));
            string path = Path.Combine(root, "Native~", "iOS", "MoonetixOfferwall.xcframework");
            return Directory.Exists(path) ? path : null;
        }

        private static string SourcePath([CallerFilePath] string path = "") => path;
    }
}
