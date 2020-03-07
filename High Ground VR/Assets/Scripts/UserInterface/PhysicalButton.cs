using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalButton : MonoBehaviour
{
    [SerializeField, Tooltip("Distance for the button to travel")] private float m_buttonPressDistance = 0.03f;
    [SerializeField, Tooltip("The time the button takes to return. The 'Heavyness' of the button")] private float m_buttonReturnSpeed = 0.01f;

    [SerializeField, Tooltip("The method to run on this button's press"), Space(10)] private UnityEvent m_buttonPressMethod;

    private Vector3 m_startPosition;
    private Rigidbody m_rigidbody;
    private bool m_pressed;
    private bool m_released;


    // Start is called before the first frame update
    void Start()
    {
        m_startPosition = transform.localPosition;
        m_rigidbody = this.GetComponent<Rigidbody>();
        m_pressed = false;
    }
    void Update()
    {
        m_pressed = false;

        // Return button to startPosition
        if (transform.localPosition != m_startPosition)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, m_startPosition, Time.deltaTime * m_buttonReturnSpeed);
        }

        if (transform.localPosition.z > m_startPosition.z + m_buttonPressDistance - (m_buttonPressDistance / 4))
        {
            //Button is lower than it should be
            transform.localPosition = new Vector3(m_startPosition.x, m_startPosition.y, m_startPosition.z + m_buttonPressDistance);
            if (!m_pressed && m_released)
            {
                m_released = false;
                m_pressed = true;
                m_buttonPressMethod.Invoke();
            }
        }
        else if (transform.localPosition.z < m_startPosition.z)
        {
            //Button is higher than it should be
            transform.localPosition = new Vector3(m_startPosition.x, m_startPosition.y, m_startPosition.z);
            m_released = true;
        }

        else if (Mathf.Abs(transform.localPosition.z - m_startPosition.z) < m_buttonPressDistance / 5.0f)
        {
            m_released = true;


        }
        //Dectivate unpressed button
        if (transform.localPosition.x != m_startPosition.x || transform.localPosition.y != m_startPosition.y)
        {
            transform.localPosition = new Vector3(m_startPosition.x, m_startPosition.y, transform.localPosition.z);
        }

    }


}