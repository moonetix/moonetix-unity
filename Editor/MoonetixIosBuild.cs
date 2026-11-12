using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif

namespace MoonetixOfferwall.Editor
{
    /// <summary>
    /// iOS builds: checks the deployment target before the build, then copies MoonetixOfferwall.xcframework
    /// into the Xcode project, links it to UnityFramework and embeds it in the app.
    /// </summary>
    internal sealed class MoonetixIosBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        internal const string FrameworkName = "MoonetixOfferwall.xcframework";
        internal const string ProjectFolder = "Frameworks/com.moonetix.offerwall";

        public int callbackOrder
        {
            get { return 100; }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }
            if (!MoonetixIosVersion.IsSupported(PlayerSettings.iOS.targetOSVersionString))
            {
                throw new BuildFailedException(MoonetixIosVersion.TooLowMessage);
            }
            if (FindFramework() == null)
            {
                throw new BuildFailedException("[Moonetix] " + FrameworkName + " is missing from the com.moonetix.offerwall package. Reinstall the package.");
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS)
            {
                return;
            }
#if UNITY_IOS
            AddFramework(report.summary.outputPath);
#endif
        }

        internal static string FindFramework()
        {
            var info = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(MoonetixIosBuild).Assembly);
            string root = info != null ? info.resolvedPath : Path.GetFullPath("Packages/com.moonetix.offerwall");
            string path = Path.Combine(Path.Combine(Path.Combine(root, "Native~"), "iOS"), FrameworkName);
            return Directory.Exists(path) ? path : null;
        }

#if UNITY_IOS
        private static void AddFramework(string buildPath)
        {
            string source = FindFramework();
            if (source == null)
            {
                throw new BuildFailedException("[Moonetix] " + FrameworkName + " is missing from the com.moonetix.offerwall package. Reinstall the package.");
            }
            string relative = ProjectFolder + "/" + FrameworkName;
            string target = Path.Combine(buildPath, relative);
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }
            CopyDirectory(source, target);

            string projectPath = PBXProject.GetPBXProjectPath(buildPath);
            var project = new PBXProject();
            project.ReadFromFile(projectPath);
            string appTarget = project.GetUnityMainTargetGuid();
            string frameworkTarget = project.GetUnityFrameworkTargetGuid();
            string fileGuid = project.FindFileGuidByProjectPath(relative);
            if (fileGuid == null)
            {
                fileGuid = project.AddFile(relative, relative, PBXSourceTree.Source);
                project.AddFileToBuildSection(frameworkTarget, project.GetFrameworksBuildPhaseByTarget(frameworkTarget), fileGuid);
                project.AddFileToEmbedFrameworks(appTarget, fileGuid);
            }
            project.AddBuildProperty(appTarget, "LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            project.WriteToFile(projectPath);
            FixFileType(projectPath, relative);
        }

        // Older Xcode API versions do not know the xcframework file type; Xcode needs it to pick the slice.
        private static void FixFileType(string projectPath, string relative)
        {
            string text = File.ReadAllText(projectPath);
            string[] lines = text.Split('\n');
            bool changed = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.Contains("PBXFileReference") && line.Contains(FrameworkName) && !line.Contains("wrapper.xcframework"))
                {
                    string updated = System.Text.RegularExpressions.Regex.Replace(line, @"lastKnownFileType = [^;]+;", "lastKnownFileType = wrapper.xcframework;");
                    if (updated == line)
                    {
                        updated = line.Replace("isa = PBXFileReference;", "isa = PBXFileReference; lastKnownFileType = wrapper.xcframework;");
                    }
                    updated = System.Text.RegularExpressions.Regex.Replace(updated, @"explicitFileType = [^;]+; ?", string.Empty);
                    lines[i] = updated;
                    changed = true;
                }
            }
            if (changed)
            {
                File.WriteAllText(projectPath, string.Join("\n", lines));
            }
        }

        private static void CopyDirectory(string source, string target)
        {
            Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source))
            {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            }
            foreach (string dir in Directory.GetDirectories(source))
            {
                CopyDirectory(dir, Path.Combine(target, Path.GetFileName(dir)));
            }
        }
#endif
    }
}
