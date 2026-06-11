using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TypingGameSetup
{
    [MenuItem("OnlyToType/Setup Platformer Scene")]
    static void SetupScene()
    {
        if (Object.FindAnyObjectByType<TypingGameBootstrap>() != null)
        {
            Debug.Log("TypingGameBootstrap already exists in the scene.");
            return;
        }

        var bootstrapObj = new GameObject("TypingGameBootstrap");
        bootstrapObj.AddComponent<TypingGameBootstrap>();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = bootstrapObj;

        Debug.Log("Auto-scroll platformer setup complete! Press Play to start.");
    }
}
