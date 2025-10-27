using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class FreeLookZoom : MonoBehaviour
{
    public CinemachineFreeLook freeLookCam;
    public float zoomSpeed = 2f;
    public float minRadius = 1f;
    public float maxRadius = 3f;

    void Update()
    {
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
}
