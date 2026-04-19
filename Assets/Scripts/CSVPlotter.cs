using System.IO;
using UnityEngine;

public class CSVPlotter : MonoBehaviour
{
    [Header("Recording Settings")]
    [Tooltip("The name of the saved file. Will be saved in project root directory.")]
    public string fileName = "y_positions.csv";

    [Tooltip("How often to record the position (in seconds).")]
    public float recordInterval = 0.1f;

    [Tooltip("If true, starts recording immediately when the scene starts.")]
    public bool recordOnStart = true;

    private StreamWriter writer;
    private float timer;
    private float startTime;
    private bool isRecording = false;

    void Start()
    {
        if (recordOnStart)
        {
            StartRecording();
        }
    }

    void Update()
    {
        if (!isRecording) return;

        timer += Time.deltaTime;

        // When the timer exceeds the interval, record a new data point
        if (timer >= recordInterval)
        {
            timer -= recordInterval;
            RecordData();
        }
    }

    /// <summary>
    /// Initializes the StreamWriter and starts the recording process.
    /// </summary>
    public void StartRecording()
    {
        if (isRecording) return;

        // Saves to the folder just above the Assets folder (Project Root)
        string filePath = Path.Combine(Application.dataPath, "..", fileName);

        try
        {
            // Set 'false' to overwrite any existing file with the same name. Use 'true' to append.
            writer = new StreamWriter(filePath, false);
            writer.WriteLine("t,y"); // Write CSV header

            startTime = Time.time;
            timer = 0f;
            isRecording = true;

            Debug.Log($"CSVPlotter: Started recording to {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CSVPlotter: Failed to initialize CSV writer: {e.Message}");
        }
    }

    /// <summary>
    /// Closes the file smoothly so data is not corrupted.
    /// </summary>
    public void StopRecording()
    {
        if (!isRecording) return;

        isRecording = false;
        if (writer != null)
        {
            writer.Close();
            writer = null;
            Debug.Log("CSVPlotter: Stopped recording and saved CSV.");
        }
    }

    private void RecordData()
    {
        if (writer != null)
        {
            float currentTime = Time.time - startTime;
            float currentY = transform.position.y;

            // Write a new line of comma-separated values
            writer.WriteLine($"{currentTime:F3},{currentY}");
        }
    }

    // Ensure the file is safely closed if the GameObject is destroyed or the game stops
    void OnDestroy()
    {
        StopRecording();
    }

    void OnApplicationQuit()
    {
        StopRecording();
    }
}