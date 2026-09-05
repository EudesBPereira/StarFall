using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Starfall.EditorTools
{
    /// <summary>
    /// Command-line builds.
    ///   Android: Unity.exe -batchmode -projectPath . -executeMethod Starfall.EditorTools.BuildScript.BuildAndroid -quit
    ///   iOS (Xcode project, macOS only): -executeMethod Starfall.EditorTools.BuildScript.BuildIos
    /// </summary>
    public static class BuildScript
    {
        private static string[] Scenes => EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();

        [MenuItem("Starfall/Build/Android APK")]
        public static void BuildAndroid()
        {
            string output = Path.GetFullPath("Builds/Android/StarfallDefense.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            EditorUserBuildSettings.buildAppBundle = false;
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None,
            };
            Report(BuildPipeline.BuildPlayer(options));
        }

        [MenuItem("Starfall/Build/iOS Xcode Project")]
        public static void BuildIos()
        {
            string output = Path.GetFullPath("Builds/iOS");
            Directory.CreateDirectory(output);
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = output,
                target = BuildTarget.iOS,
                options = BuildOptions.None,
            };
            Report(BuildPipeline.BuildPlayer(options));
        }

        [MenuItem("Starfall/Build/Windows (dev build)")]
        public static void BuildWindows()
        {
            string output = Path.GetFullPath("Builds/Windows/StarfallDefense.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None,
            };
            Report(BuildPipeline.BuildPlayer(options));
        }

        private static void Report(BuildReport report)
        {
            var summary = report.summary;
            Debug.Log($"[Starfall] Build {summary.result}: {summary.outputPath} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalErrors} errors, {summary.totalWarnings} warnings)");
            if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
                EditorApplication.Exit(1);
        }
    }
}
