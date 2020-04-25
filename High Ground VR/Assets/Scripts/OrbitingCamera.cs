using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitingCamera : MonoBehaviour
{

    [SerializeField,Range(0.0f,3.0f)] private float m_rotationSpeed;
    [SerializeField] private GameObject m_camera;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(this.transform.position,Vector3.up, m_rotationSpeed);
        Vector3 _lookpoint = new Vector3(0, 20, 0);
        m_camera.transform.LookAt(_lookpoint);
    }
}
