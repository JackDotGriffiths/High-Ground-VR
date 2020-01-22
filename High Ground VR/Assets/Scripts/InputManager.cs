using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager s_instance;
    private enum HandTypes{left,right};
    [SerializeField] private HandTypes m_handedness;
    [SerializeField, Space(20)] private GameObject m_vrRig;
    [SerializeField]private GameObject m_camera, m_leftController, m_rightController,m_gameEnvironment;

    [Header("Player Scaling")]
    [SerializeField,Space(20)] private Vector3 m_smallestScale;
    [SerializeField] private Vector3 m_largestScale;
    [SerializeField] private float m_scalingSpeed;
    private float m_maxWorldHeight; //This depends on the player's height.


    [Header("Point & Click")]
    [SerializeField, Space(20)] private Material m_pointerMaterial;

    private bool m_leftGripped, m_rightGripped, m_leftTrigger,m_rightTrigger;
    private Vector3 m_prevControllerMidpoint, m_controllerMidpoint;
    private float m_prevControllerDistance, m_controllerDistance;

    private GameObject m_mainController, m_offHandController;
    private LineRenderer m_mainPointer;

    #region Accessors
    public static InputManager Instance { get => s_instance; set => s_instance = value; }
    public bool LeftGripped { get => m_leftGripped; set => m_leftGripped = value; }
    public bool RightGripped { get => m_rightGripped; set => m_rightGripped = value; }
    public bool LeftTrigger { get => m_leftTrigger; set => m_leftTrigger = value; }
    public bool RightTrigger { get => m_rightTrigger; set => m_rightTrigger = value; }
    public GameObject OffHandController { get => m_offHandController; set => m_offHandController = value; }
    public GameObject MainController { get => m_mainController; set => m_mainController = value; }
    #endregion

    void Start()
    {
        //Handles whether the player is left or right handed.
        if (m_handedness == HandTypes.left)
        {
            m_mainController = m_leftController;
            m_offHandController = m_rightController;
        }
        if (m_handedness == HandTypes.right)
        {
            m_mainController = m_rightController;
            m_offHandController = m_leftController;
        }

        //Adding a line renderer for the point & click functionality
        m_mainPointer = m_mainController.AddComponent<LineRenderer>();
        m_mainPointer.enabled = true;
        m_mainPointer.SetPosition(0, m_mainController.transform.position);
        m_mainPointer.SetPosition(1, m_mainController.transform.forward * 100);
        m_mainPointer.startWidth = 0.05f;
        m_mainPointer.endWidth = 0.00f;
        m_mainPointer.material = m_pointerMaterial;

        m_maxWorldHeight = m_camera.transform.position.y * 0.7f;
        m_gameEnvironment.transform.position = new Vector3(m_gameEnvironment.transform.position.x, m_maxWorldHeight, m_gameEnvironment.transform.position.z);
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

    void Update()
    {
        #region Player Movement Handling
        //Draw a line between the two controllers.
        Debug.DrawLine(m_leftController.transform.position, m_rightController.transform.position,Color.red);

        //Draw a line from the midpoint of the controllers to the camera.
        m_controllerMidpoint = (m_leftController.transform.position + m_rightController.transform.position) / 2;
        Debug.DrawLine(m_controllerMidpoint, m_camera.transform.position, Color.green);

       
        //If both controllers grip buttons are pressed, transform the camera by the vector difference between the controller midpoints previous and current position
        if(m_leftTrigger == true && m_rightTrigger == true)
        {
            Vector3 _offsetVector = -(m_controllerMidpoint - m_prevControllerMidpoint);
            Vector3 _targetRigPosition = new Vector3(m_vrRig.transform.position.x + _offsetVector.x, m_vrRig.transform.position.y, m_vrRig.transform.position.z + _offsetVector.z);
            if (_offsetVector.magnitude > 0.01f)
            {
                m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, _targetRigPosition, 0.99f);
            }
        }
        m_prevControllerMidpoint = m_controllerMidpoint;
        m_leftTrigger = false;
        m_rightTrigger = false;
        #endregion

        #region World Scaling Handling
        m_controllerDistance = Vector3.Distance(m_leftController.transform.position, m_rightController.transform.position);
        if(m_leftGripped == true && m_rightGripped == true)
        {
            float _distanceDifference = Mathf.Abs(m_controllerDistance - m_prevControllerDistance);
            if (_distanceDifference > 0.02f)
            {
                if (m_controllerDistance > m_prevControllerDistance)
                {
                    //ZOOM IN - SCALE UP
                    Vector3 _newScale = new Vector3(m_gameEnvironment.transform.localScale.x + m_scalingSpeed, m_gameEnvironment.transform.localScale.y + m_scalingSpeed, m_gameEnvironment.transform.localScale.z + m_scalingSpeed);
                    if (_newScale.x > m_largestScale.x)
                    {
                        _newScale = m_largestScale;
                    }
                    m_gameEnvironment.transform.localScale = Vector3.Lerp(m_gameEnvironment.transform.localScale, _newScale, 0.99f);
                    m_gameEnvironment.transform.position = Vector3.Lerp(m_gameEnvironment.transform.position, new Vector3(m_gameEnvironment.transform.position.x, m_gameEnvironment.transform.position.y - m_scalingSpeed, m_gameEnvironment.transform.position.z), 0.99f);
                    if (m_gameEnvironment.transform.position.y < -(m_largestScale.x/2))
                    {
                        m_gameEnvironment.transform.position = Vector3.Lerp(m_gameEnvironment.transform.position, new Vector3(m_gameEnvironment.transform.position.x, -(m_largestScale.x / 2), m_gameEnvironment.transform.position.z), 0.99f);
                    }
                }
                if (m_controllerDistance < m_prevControllerDistance)
                {
                    //ZOOM OUT - SCALE DOWN
                    Vector3 _newScale = new Vector3(m_gameEnvironment.transform.localScale.x - m_scalingSpeed, m_gameEnvironment.transform.localScale.y - m_scalingSpeed, m_gameEnvironment.transform.localScale.z - m_scalingSpeed);
                    if (_newScale.x < m_smallestScale.x)
                    {
                        _newScale = m_smallestScale;
                    }
                    m_gameEnvironment.transform.localScale = Vector3.Lerp(m_gameEnvironment.transform.localScale, _newScale, 0.99f);
                    m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, Vector3.zero, 0.99f);
                    m_gameEnvironment.transform.position = Vector3.Lerp(m_gameEnvironment.transform.position, new Vector3(m_gameEnvironment.transform.position.x, m_gameEnvironment.transform.position.y + m_scalingSpeed, m_gameEnvironment.transform.position.z), 0.99f);
                    if(m_gameEnvironment.transform.position.y > m_maxWorldHeight)
                    {
                        m_gameEnvironment.transform.position = Vector3.Lerp(m_gameEnvironment.transform.position, new Vector3(m_gameEnvironment.transform.position.x, m_maxWorldHeight, m_gameEnvironment.transform.position.z), 0.99f);
                    }

                }
            }
        }
        m_prevControllerDistance = m_controllerDistance;
        m_leftGripped = false;
        m_rightGripped = false;
        #endregion


        #region Point and Click Handling
        //Debug.DrawRay(m_mainController.transform.position, m_mainController.transform.forward, Color.blue);


        m_mainPointer.SetPosition(0, m_mainController.transform.position);
        m_mainPointer.SetPosition(1, m_mainController.transform.forward * 100);

        RaycastHit _hit;
        if (Physics.Raycast(m_mainController.transform.position, m_mainController.transform.forward, out _hit, 100))
        {
            m_mainPointer.startColor = Color.green;
            m_mainPointer.endColor = Color.green;
        }
        else // 3
        {
            m_mainPointer.startColor = Color.red;
            m_mainPointer.endColor = Color.red;
        }


        #endregion



    }
}
