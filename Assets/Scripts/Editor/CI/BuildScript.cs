using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Mojipet.Editor.CI
{
    // Invoked via `-executeMethod Mojipet.Editor.CI.BuildScript.BuildIos` from Jenkins batchmode.
    public static class BuildScript
    {
        private const string DefaultBundleIdentifier = "com.lenia.mojipet";

        public static void BuildIos()
        {
            PlayerSettings.applicationIdentifier = GetArgument("-bundleId") ?? DefaultBundleIdentifier;

            var teamId = GetArgument("-teamId");
            if (!string.IsNullOrEmpty(teamId))
            {
                PlayerSettings.iOS.appleDeveloperTeamID = teamId;
            }

            PlayerSettings.iOS.appleEnableAutomaticSigning = true;

            var outputPath = GetArgument("-buildOutput") ?? "Builds/iOS";
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"iOS build failed: {report.summary.result} ({report.summary.totalErrors} errors)");
            }
        }

        private static string GetArgument(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }

            return null;
        }
    }
}
