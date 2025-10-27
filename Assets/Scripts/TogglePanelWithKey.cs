using UnityEngine;

public class TogglePanelWithKey : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }
    }
}
