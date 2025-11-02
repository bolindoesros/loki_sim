using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[Serializable]
public enum SceneIndex
{
    Persistent = 0,
    Menu = 1,
    Sauvc = 2
}

/// <summary>
/// This script should only be attached to an object in the PersistentScene. Not in any other scenes.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] CanvasGroup loadingScreen;

    public static SceneLoader Instance { get; private set; }
    readonly float minLoadingTime = 1f;
    readonly float fadeDuration = 0.5f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);

        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
#if UNITY_EDITOR
        // If only the persistent scene is loaded, open the menu scene
        // Else (e.g. when playtesting from another scene), set the non-persistent scene as active
        if (SceneManager.sceneCount == 1)
        {
            SceneManager.LoadSceneAsync((int)SceneIndex.Menu, LoadSceneMode.Additive);
        }
        else
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.buildIndex != (int)SceneIndex.Persistent)
                {
                    SceneManager.SetActiveScene(scene);
                    break;
                }
            }
        }
#else
        // In build: always start from menu
        SceneManager.LoadSceneAsync((int)SceneIndex.Menu, LoadSceneMode.Additive);
#endif
    }

    // Automatically set the newly loaded scene as active
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) { SceneManager.SetActiveScene(scene); }

    // StartCoroutine is intentionally called in this class so that the returned Coroutine can stay in the persistent scene
    // and will not be interrupted when other scenes are unloaded.
    public void LoadScene(SceneIndex sceneToLoad) { StartCoroutine(LoadSceneCoroutine(sceneToLoad)); }

    IEnumerator LoadSceneCoroutine(SceneIndex sceneToLoad)
    {
        // Show loading screen (wait till it's fully visible)
        yield return FadeCanvas(loadingScreen, 1f, fadeDuration);

        // Start tracking how long the loading screen has been visible for
        float startTime = Time.time;

        // Unload and Load new scene
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
        AsyncOperation loadAsyncOp = SceneManager.LoadSceneAsync((int)sceneToLoad, LoadSceneMode.Additive);
        loadAsyncOp.allowSceneActivation = false;

        // Track loading progress until 90% (when allowSceneActivation is false, progress stops at 0.9)
        while (loadAsyncOp.progress < 0.9f)
        {
            Debug.Log($"Loading progress: {loadAsyncOp.progress}");
            yield return null;
        }

        // Ensure the loading screen stays visible at least minLoadingTime seconds
        float elapsed = Time.time - startTime;
        if (elapsed < minLoadingTime)
            yield return new WaitForSeconds(minLoadingTime - elapsed);

        // Scene is loaded to 90%, ready to activate
        // Update progress bar to show completion
        loadAsyncOp.allowSceneActivation = true;

        // Wait until scene transition is fully completed
        while (!loadAsyncOp.isDone)
            yield return null;

        // Hide loading screen (wait till it's fully hidden)
        yield return FadeCanvas(loadingScreen, 0f, fadeDuration);
    }

    IEnumerator FadeCanvas(CanvasGroup cg, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        float startAlpha = loadingScreen.alpha;

        while (elapsed <= duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
    }
}
