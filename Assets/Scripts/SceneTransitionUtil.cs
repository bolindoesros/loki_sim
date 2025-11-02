using UnityEngine;

public class SceneTransitionUtil : MonoBehaviour
{
    [SerializeField] SceneIndex newSceneToLoad;

    public void LoadNewScene()
    {
        SceneLoader.Instance.LoadScene(newSceneToLoad);
    }
}
