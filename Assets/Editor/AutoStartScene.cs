using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AutoStartScene
{
    // Ajusta la ruta a donde realmente está tu escena
    private const string ScenePath = "Assets/ColonyScene.unity";

    static AutoStartScene()
    {
        // Configura la escena de inicio de Play al cargar el editor
        var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (scene == null)
        {
            Debug.LogWarning($"AutoStartScene: No se encontró la escena en {ScenePath}");
            return;
        }

        EditorSceneManager.playModeStartScene = scene;

        // Opcional: si alguien borra esa asignación durante la sesión, la reponemos antes de entrar a Play
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode && EditorSceneManager.playModeStartScene == null)
        {
            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene != null)
            {
                EditorSceneManager.playModeStartScene = scene;
            }
        }
    }
}