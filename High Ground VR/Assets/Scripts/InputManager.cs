using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager s_instance;

    [SerializeField] private GameObject m_vrRig, m_camera, m_leftController, m_rightController;
    [SerializeField,Space(20)] private Vector3 m_smallestScale;
    [SerializeField] private Vector3 m_largestScale;

    private bool m_leftGripped, m_rightGripped, m_leftTrigger,m_rightTrigger;
    private Vector3 m_prevControllerMidpoint, m_controllerMidpoint;
    private float m_prevControllerDistance, m_controllerDistance;

    #region Accessors
    public static InputManager Instance { get => s_instance; set => s_instance = value; }
    public bool LeftGripped { get => m_leftGripped; set => m_leftGripped = value; }
    public bool RightGripped { get => m_rightGripped; set => m_rightGripped = value; }
    public bool LeftTrigger { get => m_leftTrigger; set => m_leftTrigger = value; }
    public bool RightTrigger { get => m_rightTrigger; set => m_rightTrigger = value; }
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
        m_controllerDistance = Vector3.Distance(m_leftController.transform.position, m_rightController.transform.position);
        Debug.DrawLine(m_controllerMidpoint, m_camera.transform.position, Color.green);





        //If both controllers grip buttons are pressed, transform the camera by the vector difference between the controller midpoints previous and current position
        if(m_leftGripped == true && m_rightGripped == true)
        {
            Vector3 _offsetVector = -(m_controllerMidpoint - m_prevControllerMidpoint);
            Vector3 _targetRigPosition = new Vector3(m_vrRig.transform.position.x + _offsetVector.x, m_vrRig.transform.position.y, m_vrRig.transform.position.z + _offsetVector.z);
            if (_offsetVector.magnitude > 0.01f)
            {
                m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, _targetRigPosition, 0.99f);
            }
        }
        m_prevControllerMidpoint = m_controllerMidpoint;
        m_prevControllerDistance = m_controllerDistance;
    }
}
