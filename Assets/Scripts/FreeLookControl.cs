using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FreeLookControl : MonoBehaviour
{
    public CinemachineFreeLook freeLookCam;

    // For zooming
    public float zoomSpeed = 2f;
    public float minRadius = 1f;
    public float maxRadius = 3f;

    // For right click free look rotation
    private float xAxisDefaultSpeed;
    private float yAxisDefaultSpeed;
    void Awake()
    {
        // Cache original speeds
        xAxisDefaultSpeed = freeLookCam.m_XAxis.m_MaxSpeed;
        yAxisDefaultSpeed = freeLookCam.m_YAxis.m_MaxSpeed;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            // Enable camera rotation while mouse button held
            freeLookCam.m_XAxis.m_MaxSpeed = xAxisDefaultSpeed;
            freeLookCam.m_YAxis.m_MaxSpeed = yAxisDefaultSpeed;

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                for (int i = 0; i < 3; i++)
                {
                    freeLookCam.m_Orbits[i].m_Radius = Mathf.Clamp(
                        freeLookCam.m_Orbits[i].m_Radius - scroll * zoomSpeed,
                        minRadius,
                        maxRadius
                    );
                }
            }
        }
        else
        {
            // Disable rotation input
            freeLookCam.m_XAxis.m_MaxSpeed = 0f;
            freeLookCam.m_YAxis.m_MaxSpeed = 0f;
        }


    }
}
