using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InstructionMenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject pressF1Text;

    bool _isOpen = false;

    void Awake()
    {
        // Start closed by default
        if (menuCanvas != null) SetMenuOpen(false);
    }


    void Update()
    {
        // Toggle with F1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleMenu();
        }

        // Can also close by pressing Esc
        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            SetMenuOpen(false);
        }
    }


    public void ToggleMenu()
    {
        SetMenuOpen(!_isOpen);
    }


    public void SetMenuOpen(bool open)
    {
        _isOpen = open;
        if (menuCanvas != null) menuCanvas.SetActive(open);
        if (pressF1Text != null) pressF1Text.SetActive(!open);
    }
}
