using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class ToggleGameObject : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private GameObject targetObject;

    private void Start()
    {
        if (toggle == null)
        {
            toggle = GetComponent<Toggle>();
        }
        toggle.isOn = targetObject != null && targetObject.activeSelf;
    }

    public void Toggle(bool isOn)
    {
        targetObject.SetActive(isOn);
    }
}
