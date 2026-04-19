using UnityEngine;
using UnityEngine.Perception.GroundTruth;

public class PCamManualCapture : MonoBehaviour
{
    PerceptionCamera pCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pCam = GetComponent<PerceptionCamera>();
        if (pCam == null)
        {
            Debug.LogError("PCamManualCapture: No PerceptionCamera component found on this GameObject!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (pCam != null)
            {
                pCam.RequestCapture();
            }
        }
    }
}
