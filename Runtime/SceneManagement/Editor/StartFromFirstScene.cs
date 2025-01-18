using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StartFromFirstScene
{
    // MenuItem to add a shortcut Ctrl+R (Cmd+R on macOS) to play the game
    [MenuItem("Tools/Play From First Scene #r")]
    private static void PlayFromFirstScene()
    {
        // Check if the build settings have any scenes
        if (EditorBuildSettings.scenes.Length == 0)
        {
            Debug.LogError("No scenes in the build settings!");
            return;
        }

        // Get the path of the first scene in build settings
        string firstScenePath = EditorBuildSettings.scenes[0].path;

        // Save the current scene and start playing the first scene
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(firstScenePath);
            EditorApplication.isPlaying = true;
        }
    }
}
