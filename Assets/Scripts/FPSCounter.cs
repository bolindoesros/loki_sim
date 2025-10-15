using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    
    [Header("FPS Settings")]
    [Tooltip("How often to update the FPS display (in seconds)")]
    public float updateInterval = 0.2f;
    
    private float accumulatedTime = 0f;
    private int frames = 0;
    private float currentFPS = 0f;
    private float timeLeft;

    void Start()
    {
        timeLeft = updateInterval;
    }

    void Update()
    {
        timeLeft -= Time.unscaledDeltaTime;
        accumulatedTime += Time.unscaledDeltaTime;
        frames++;

        // Update the FPS display at the specified interval
        if (timeLeft <= 0f)
        {
            currentFPS = frames / accumulatedTime;

            if (fpsText != null)
            {
                fpsText.text = Mathf.RoundToInt(currentFPS).ToString() + " FPS";
            }

            // Reset for next interval
            timeLeft = updateInterval;
            accumulatedTime = 0f;
            frames = 0;
        }
    }
}
