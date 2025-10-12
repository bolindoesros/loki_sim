using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;

    void Update()
    {
        int frameRate = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        fpsText.text = frameRate.ToString() + "  FPS";
    }
}
