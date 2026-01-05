using UnityEngine;
using UnityEngine.SceneManagement;

public static class CanvasUtils
{
    /// <summary>
    /// Gets the Canvas in the active scene.
    /// </summary>
    /// <returns>The Canvas in the active scene, or null if not found.</returns>
    public static Canvas GetCanvasInActiveScene()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Scene activeScene = SceneManager.GetActiveScene();

        foreach (Canvas canvas in canvases)
        {
            if (canvas.gameObject.scene == activeScene)
            {
                return canvas;
            }
        }

        Debug.LogWarning("CanvasUtils: No Canvas found in the active scene!");
        return null;
    }

    /// <summary>
    /// Gets the Transform of the Canvas in the active scene.
    /// </summary>
    /// <returns>The Canvas Transform in the active scene, or null if not found.</returns>
    public static Transform GetCanvasTransformInActiveScene()
    {
        Canvas canvas = GetCanvasInActiveScene();
        return canvas != null ? canvas.transform : null;
    }
}
