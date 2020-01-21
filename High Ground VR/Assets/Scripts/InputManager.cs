using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager s_instance;

    [SerializeField] private GameObject m_vrRig, m_camera, m_leftController, m_rightController;

    [Header("Player Scaling")]
    [SerializeField,Space(20)] private Vector3 m_smallestScale;
    [SerializeField] private Vector3 m_largestScale;
    [SerializeField] private float m_scalingSpeed;

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
   


        m_controllerDistance = Vector3.Distance(m_leftController.transform.position, m_rightController.transform.position);
        if(m_leftGripped == true && m_rightGripped == true)
        {
            float _distanceDifference = Mathf.Abs(m_controllerDistance - m_prevControllerDistance);
            if (_distanceDifference > 0.02f)
            {
                if (m_controllerDistance > m_prevControllerDistance)
                {
                    //ZOOM IN - SCALE UP
                    Vector3 _newScale = new Vector3(m_vrRig.transform.localScale.x + m_scalingSpeed, m_vrRig.transform.localScale.y + m_scalingSpeed, m_vrRig.transform.localScale.z + m_scalingSpeed);
                    if (_newScale.x > m_largestScale.x)
                    {
                        _newScale = m_largestScale;
                    }
                    m_vrRig.transform.localScale = Vector3.Lerp(m_vrRig.transform.localScale, _newScale, 0.99f);
                    m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, new Vector3(m_vrRig.transform.position.x, m_vrRig.transform.position.y - 1f, m_vrRig.transform.position.z), 0.99f);
                    if (m_vrRig.transform.position.y < -25)
                    {
                        m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, new Vector3(m_vrRig.transform.position.x, -25, m_vrRig.transform.position.z), 0.99f);
                    }
                }
                if (m_controllerDistance < m_prevControllerDistance)
                {
                    //ZOOM OUT - SCALE DOWN
                    Vector3 _newScale = new Vector3(m_vrRig.transform.localScale.x - m_scalingSpeed, m_vrRig.transform.localScale.y - m_scalingSpeed, m_vrRig.transform.localScale.z - m_scalingSpeed);
                    if (_newScale.x < m_smallestScale.x)
                    {
                        _newScale = m_smallestScale;
                    }
                    m_vrRig.transform.localScale = Vector3.Lerp(m_vrRig.transform.localScale, _newScale, 0.99f);
                    m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, new Vector3(m_vrRig.transform.position.x, m_vrRig.transform.position.y + 1f, m_vrRig.transform.position.z), 0.99f);
                    if(m_vrRig.transform.position.y > 0)
                    {
                        m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, new Vector3(m_vrRig.transform.position.x, 0, m_vrRig.transform.position.z), 0.99f);
                    }

                }
            }
        }
        m_prevControllerDistance = m_controllerDistance;
        m_leftGripped = false;
        m_rightGripped = false;



    }
}
