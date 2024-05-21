using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook cam;

    private float minZoom = 4f;
    private float maxZoom = 20f;

    // Start is called before the first frame update
    void Start()
    {
        cam.m_XAxis.m_MaxSpeed = 0;
        cam.m_YAxis.m_MaxSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            cam.m_XAxis.m_MaxSpeed = 500;
            cam.m_YAxis.m_MaxSpeed = 5;
        }

        if (Input.GetMouseButtonUp(1))
        {
            cam.m_XAxis.m_MaxSpeed = 0;
            cam.m_YAxis.m_MaxSpeed = 0;
        }

        var scrollwheel = Input.GetAxis("Mouse ScrollWheel") * -1;

        if (scrollwheel != 0)
        {
            if (cam.m_Orbits[1].m_Radius <= minZoom && scrollwheel < 0) return;
            else if (cam.m_Orbits[1].m_Radius >= maxZoom && scrollwheel > 0) return;
            else
            { 
                float newHeight = cam.m_Orbits[0].m_Height + scrollwheel * 1000;
                if (cam.m_Orbits[0].m_Height >= 2.5 || scrollwheel > 0) cam.m_Orbits[0].m_Height = Mathf.Lerp(cam.m_Orbits[0].m_Height, newHeight, Time.deltaTime);

                float newZoom = cam.m_Orbits[1].m_Radius + scrollwheel * 1000;

                cam.m_Orbits[1].m_Radius = Mathf.Lerp(cam.m_Orbits[1].m_Radius, newZoom, Time.deltaTime);
            }
        } 
    }
}
