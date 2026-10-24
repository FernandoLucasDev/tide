using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using MareDeFerro;

public static class HarborBuild {
    [MenuItem("Maré de Ferro/Criar cena e gerar Windows")]
    public static void Build() {
        string project=Directory.GetParent(Application.dataPath).FullName;
        string output=Path.Combine(Directory.GetParent(project).FullName,"Builds","Windows");
        string shots=Path.Combine(Directory.GetParent(project).FullName,"verificacao");
        Directory.CreateDirectory(output);Directory.CreateDirectory(shots);Directory.CreateDirectory("Assets/Scenes");
        // Explicit resource dependency retains the surface shader in standalone builds.
        const string surfacePath="Assets/Resources/HarborSurface.mat";
        if(!AssetDatabase.LoadAssetAtPath<Material>(surfacePath)) {
            var surface=new Material(Shader.Find("Standard"));surface.EnableKeyword("_EMISSION");surface.SetColor("_EmissionColor",Color.black);
            AssetDatabase.CreateAsset(surface,surfacePath);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        PlayerSettings.companyName="Porto das Velas";PlayerSettings.productName="Maré de Ferro";
        PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=true;
        PlayerSettings.colorSpace=ColorSpace.Linear;
        // The world enables fog at runtime, so automatic scene-based stripping must retain it.
        var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
        graphics.FindProperty("m_FogStripping").intValue=1;
        graphics.FindProperty("m_FogKeepLinear").boolValue=true;
        graphics.FindProperty("m_FogKeepExp").boolValue=true;
        graphics.FindProperty("m_FogKeepExp2").boolValue=true;
        graphics.ApplyModifiedPropertiesWithoutUndo();
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.Standalone,ApiCompatibilityLevel.NET_Standard);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,new[]{UnityEngine.Rendering.GraphicsDeviceType.Direct3D11});
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var root=new GameObject("Maré de Ferro — iniciar");root.AddComponent<HarborGame>();
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/PortoDasVelas.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/PortoDasVelas.unity",true)};
        AssetDatabase.SaveAssets();
        // Render the generated world with the same camera and shaders used by the game.
        var world=HarborWorld.Create();
        HarborGame.RenderPhoto(HarborWorld.camera,Path.Combine(shots,"porto-editor.png"),1600,1000);
        UnityEngine.Object.DestroyImmediate(world.gameObject);UnityEngine.Object.DestroyImmediate(HarborWorld.camera.gameObject);
        scene=EditorSceneManager.OpenScene("Assets/Scenes/PortoDasVelas.unity");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/PortoDasVelas.unity"},locationPathName=Path.Combine(output,"MareDeFerro.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
        File.WriteAllText(Path.Combine(shots,"build-report.txt"),report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nBytes: "+report.summary.totalSize+"\nTime: "+report.summary.totalTime);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
        Debug.Log("MARE_BUILD_PASS");
    }
}
