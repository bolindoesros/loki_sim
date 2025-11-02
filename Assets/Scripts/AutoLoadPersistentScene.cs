using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility class to automatically load the persistent scene (build index 0) when entering play mode in the editor.
/// </summary>

#if UNITY_EDITOR
[InitializeOnLoad]
public static class AutoLoadPersistentScene
{
    static AutoLoadPersistentScene()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Ensure the persistent scene (build index 0) is loaded when entering play mode
                if (!SceneManager.GetSceneByBuildIndex(0).isLoaded)
                    SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive);
            }
        };
    }
}
#endif