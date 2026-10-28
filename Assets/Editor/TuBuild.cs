using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class TuBuild
{
    const string Scene="Assets/Scenes/Hangar.unity";
    [MenuItem("Transformers Universe/Open Startup Scene")]
    public static void Open(){EditorSceneManager.OpenScene(Scene);}
    [MenuItem("Transformers Universe/Build Windows Game")]
    public static void Windows()
    {
        string output=System.IO.Path.GetFullPath("Build/TransformersUniverse-v20.exe");System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(output));
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        UnityEditor.Build.Reporting.BuildReport report=BuildPipeline.BuildPlayer(new string[]{Scene},output,BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("Windows build failed: "+report.summary.result);
        Debug.Log("TU_BUILD_OUTPUT "+output);
    }
    [InitializeOnLoadMethod]
    static void FirstOpen(){EditorApplication.delayCall+=delegate{if(!EditorApplication.isPlayingOrWillChangePlaymode&&string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path)&&!EditorSceneManager.GetActiveScene().isDirty)Open();};}
    [MenuItem("Transformers Universe/Verify Startup, Movement and UI")]
    public static void Verify(){Open();SessionState.SetBool("TU.RunVerification",true);EditorApplication.isPlaying=true;}
    [MenuItem("Transformers Universe/Verify Autobot Hangar")]
    public static void VerifyAutobot(){Open();SessionState.SetBool("TU.RunAutobotVerification",true);EditorApplication.isPlaying=true;}
}
