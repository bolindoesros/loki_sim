using UnityEngine;

public class ToggleGameObject : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;

    public void Toggle()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(!targetObject.activeSelf);
        }
    }
}
