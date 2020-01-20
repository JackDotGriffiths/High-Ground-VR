using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager s_instance;

    [SerializeField] private GameObject m_vrRig, m_camera, m_leftController, m_rightController;
    [SerializeField, Space(20)] private float m_movementSensitivity;

    private bool m_leftGripped, m_rightGripped;
    private Vector3 m_prevControllerMidpoint,m_controllerMidpoint;


    #region Accessors
    public static InputManager Instance { get => s_instance; set => s_instance = value; }
    public bool LeftGripped { get => m_leftGripped; set => m_leftGripped = value; }
    public bool RightGripped { get => m_rightGripped; set => m_rightGripped = value; }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        //Singleton Implementation
        if (s_instance == null)
        {
            s_instance = this;
        }
        else
        {
            Destroy(s_instance.gameObject);
            s_instance = this;
        }
    }



    // Update is called once per frame
    void Update()
    {
        //Draw a line between the two controllers.
        Debug.DrawLine(m_leftController.transform.position, m_rightController.transform.position,Color.red);

        //Draw a line from the midpoint of the controllers to the camera.
        m_controllerMidpoint = (m_leftController.transform.position + m_rightController.transform.position) / 2;
        Debug.DrawLine(m_controllerMidpoint, m_camera.transform.position, Color.green);


        //If both controllers grip buttons are pressed, transform the camera by the vector difference between the controller midpoints previous and current position
        if(m_leftGripped == true && m_rightGripped == true)
        {
            Vector3 _offsetVector = -(m_controllerMidpoint - m_prevControllerMidpoint);
            _offsetVector = _offsetVector + (_offsetVector* m_movementSensitivity);
            Vector3 _targetRigPosition = new Vector3(m_vrRig.transform.position.x + _offsetVector.x, m_vrRig.transform.position.y, m_vrRig.transform.position.z + _offsetVector.z);
            m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, _targetRigPosition, 0.99f);
        }
        m_prevControllerMidpoint = m_controllerMidpoint;
    }
}
