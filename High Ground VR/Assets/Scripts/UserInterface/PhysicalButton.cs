using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalButton : MonoBehaviour
{
    [SerializeField, Tooltip("Distance for the button to travel")] private float m_buttonPressDistance = 0.2f;
    [SerializeField, Tooltip("The time the button takes to return. The 'Heavyness' of the button")] private float m_buttonReturnSpeed = 0.5f;

    public GameObject m_ButtonTestObject;

    private Vector3 m_startPosition;
    private Rigidbody m_rigidbody;
    private bool m_pressed;
    private bool m_released;


    // Start is called before the first frame update
    void Start()
    {
        m_startPosition = transform.localPosition;
        m_rigidbody = this.GetComponent<Rigidbody>();
    }
    void Update()
    {
        m_pressed = false;

        // Return button to startPosition
        transform.localPosition = Vector3.Lerp(transform.localPosition, m_startPosition, Time.deltaTime * m_buttonReturnSpeed);

        if(transform.localPosition.y < m_startPosition.y - m_buttonPressDistance)
        {
            //Button is lower than it should be
            transform.localPosition = new Vector3(m_startPosition.x, m_startPosition.y - m_buttonPressDistance, m_startPosition.z);
        }
        if (transform.localPosition.y > m_startPosition.y)
        {
            //Button is higher than it should be
            transform.localPosition = new Vector3(m_startPosition.x, m_startPosition.y, m_startPosition.z);
        }

        float _yDistance = Vector3.Distance(transform.localPosition,m_startPosition);


        if (_yDistance >= m_buttonPressDistance - (m_buttonPressDistance / 10) && !m_pressed && m_released == true)
        {
            m_pressed = true;
            m_released = false;
            Debug.Log("Pressed");

            Instantiate(m_ButtonTestObject);


        }
        //Dectivate unpressed button
        else if (_yDistance <= m_buttonPressDistance - (m_buttonPressDistance / 10) && m_pressed)
        {
            m_pressed = false;
            m_released = true;
        }

    }


}
