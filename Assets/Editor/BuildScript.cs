using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.Build.Reporting;

public class BuildScript
{
    [MenuItem("Build/Build Linux")]
    public static void BuildLinux()
    {
        string buildPath = "Build/Linux/";
        string buildName = "PAIRSIM_batch.x86_64"; 

        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = GetScenes(),
            locationPathName = buildPath + buildName,
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("Build failed!");
        }
    }

    private static string[] GetScenes()
    {
        return new string[]
        {
            "Assets/Autonoma/Scenes/MenuScene.unity", // scene path
            "Assets/Autonoma/Scenes/DrivingScene.unity"
        };
    }
}
