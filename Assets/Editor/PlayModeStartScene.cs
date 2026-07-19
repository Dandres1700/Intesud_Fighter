#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayModeStartScene
{
    private const string MenuScenePath = "Assets/Scenes/" + FlujoEscenasManager.MenuSceneName + ".unity";

    static PlayModeStartScene()
    {
        SetPlayModeStartScene();
        EditorApplication.projectChanged += SetPlayModeStartScene;
    }

    private static void SetPlayModeStartScene()
    {
        SceneAsset menuScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(MenuScenePath);
        if (menuScene != null)
        {
            EditorSceneManager.playModeStartScene = menuScene;
        }
    }
}
#endif
