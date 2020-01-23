using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager s_instance;
    private enum HandTypes { left, right };
    private enum SizeMode { small, large };


    [SerializeField] private HandTypes m_handedness;
    [SerializeField, Space(20)] private GameObject m_vrRig;
    [SerializeField] private GameObject m_camera, m_leftController, m_rightController, m_gameEnvironment;

    [Header("Player Scaling")]
    [SerializeField, Space(20)] private Vector3 m_smallestScale;
    [SerializeField] private Vector3 m_largestScale;
    [SerializeField] private float m_scalingSpeed;
    private float m_maxWorldHeight; //This depends on the player's height.
    private SizeMode m_currentSize;


    [Header("World Rotation")]
    [SerializeField, Space(20)] private float m_rotationSensitivity;

    [Header("Point & Click")]
    [SerializeField, Space(20)] private Material m_pointerMaterial;
    [SerializeField] private Material m_grassMaterial, m_outlineMaterial;
    private GameObject m_currentlySelected;

    [Header("Teleportation")]
    private bool m_teleporterPrimed;

    private bool m_leftGripped, m_rightGripped, m_leftTrigger, m_rightTrigger, m_leftTeleport, m_rightTeleport, m_mainTrigger, m_mainTeleport;
    private Vector3 m_prevControllerMidpoint, m_controllerMidpoint;
    private Vector3 m_mainControllerPos, m_mainControllerPreviousPos;
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
    private SizeMode CurrentSize { get => m_currentSize; set => m_currentSize = value; }
    public bool LeftTeleport { get => m_leftTeleport; set => m_leftTeleport = value; }
    public bool RightTeleport { get => m_rightTeleport; set => m_rightTeleport = value; }
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
        m_mainPointer.startWidth = 0.05f;
        m_mainPointer.endWidth = 0.00f;
        m_mainPointer.material = m_pointerMaterial;

        m_maxWorldHeight = m_camera.transform.position.y * 0.6f;
        m_gameEnvironment.transform.position = new Vector3(m_gameEnvironment.transform.position.x, m_maxWorldHeight, m_gameEnvironment.transform.position.z);

        m_currentSize = SizeMode.large;
        m_teleporterPrimed = false;
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
        if ((m_leftTrigger == true && m_mainController == m_leftController) || (m_rightTrigger == true && m_mainController == m_rightController))
        {
            m_mainTrigger = true;
        }
        else
        {
            m_mainTrigger = false;
        }
        if ((m_leftTeleport == true && m_mainController == m_leftController) || (m_rightTeleport == true && m_mainController == m_rightController))
        {
            m_mainTeleport = true;
        }
        else
        {
            m_mainTeleport = false;
        }
        #region Trigger Button Handling
        m_controllerDistance = Vector3.Distance(m_leftController.transform.position, m_rightController.transform.position);
        m_mainControllerPos = m_mainController.transform.position;
        if (m_rightTrigger == true && m_leftTrigger == true)
        {
            float _distanceDifference = Mathf.Abs(m_controllerDistance - m_prevControllerDistance);
            if (_distanceDifference > 0.02f)
            {
                if (m_controllerDistance > m_prevControllerDistance)
                {
                    //SCALE UP
                    Vector3 _newScale = new Vector3(m_gameEnvironment.transform.localScale.x + m_scalingSpeed, m_gameEnvironment.transform.localScale.y + m_scalingSpeed, m_gameEnvironment.transform.localScale.z + m_scalingSpeed);
                    if (_newScale.x > m_largestScale.x) { _newScale = m_largestScale; }
                    m_gameEnvironment.transform.localScale = Vector3.Lerp(m_gameEnvironment.transform.localScale, _newScale, 0.99f);

                    //MOVE DOWNWARDS
                    Vector3 _newPosition = new Vector3(m_gameEnvironment.transform.position.x, m_gameEnvironment.transform.position.y - m_scalingSpeed, m_gameEnvironment.transform.position.z);
                    if (_newPosition.y < -(m_largestScale.x / 2)) { _newPosition = new Vector3(m_gameEnvironment.transform.position.x, -(m_largestScale.x / 2), m_gameEnvironment.transform.position.z); }
                    m_gameEnvironment.transform.position = Vector3.Lerp(m_gameEnvironment.transform.position, _newPosition, 0.99f);
                }
                if (m_controllerDistance < m_prevControllerDistance)
                {
                    //SCALE DOWN
                    Vector3 _newScale = new Vector3(m_gameEnvironment.transform.localScale.x - (m_scalingSpeed * 1.5f), m_gameEnvironment.transform.localScale.y - (m_scalingSpeed * 1.5f), m_gameEnvironment.transform.localScale.z - (m_scalingSpeed * 1.5f));
                    if (_newScale.x < m_smallestScale.x) { _newScale = m_smallestScale; }
                    m_gameEnvironment.transform.localScale = Vector3.Lerp(m_gameEnvironment.transform.localScale, _newScale, 0.99f);

                    //MOVE UPWARDS
                    Vector3 _newPosition = new Vector3(m_gameEnvironment.transform.position.x, m_gameEnvironment.transform.position.y + m_scalingSpeed, m_gameEnvironment.transform.position.z);
                    if (_newPosition.y > m_maxWorldHeight) { _newPosition = new Vector3(m_gameEnvironment.transform.position.x, m_maxWorldHeight, m_gameEnvironment.transform.position.z); }
                    m_gameEnvironment.transform.position = Vector3.Lerp(m_gameEnvironment.transform.position, _newPosition, 0.99f);
                }
            }
        }
        if (m_mainTrigger == true && !(m_rightTrigger == true && m_leftTrigger == true) && m_currentlySelected == null)
        {
            Rigidbody _gameEnvRigid = m_gameEnvironment.GetComponent<Rigidbody>();
            Vector3 _forceVector = m_mainControllerPos - m_mainControllerPreviousPos;
            if (_forceVector.magnitude > m_rotationSensitivity)
            {
                _gameEnvRigid.AddForceAtPosition(_forceVector * _forceVector.magnitude * 500, m_mainControllerPreviousPos, ForceMode.Acceleration);
            }
            else
            {
                _gameEnvRigid.angularVelocity = Vector3.Lerp(_gameEnvRigid.angularVelocity, Vector3.zero, 0.5f);
            }

        }

        m_mainControllerPreviousPos = m_mainControllerPos;
        m_prevControllerDistance = m_controllerDistance;
        #endregion

        //Debug.DrawRay(m_mainController.transform.position, m_mainController.transform.forward * 100, Color.blue);
        m_mainPointer.SetPosition(0, m_mainController.transform.position);
        m_mainPointer.SetPosition(1, m_mainController.transform.position + m_mainController.transform.forward * 100);

        RaycastHit _hit;
        if (Physics.Raycast(m_mainController.transform.position, m_mainController.transform.forward, out _hit, 1000))
        {
            if (_hit.collider.gameObject.tag == "Environment" || _hit.collider.gameObject.tag == "ResizePlayer")
            {
                MeshRenderer _mesh;
                List<Material> _matList = new List<Material>();

                if (m_currentlySelected != _hit.collider.gameObject && m_currentlySelected != null && _hit.collider.gameObject.tag == "Environment")
                {
                    _mesh = m_currentlySelected.GetComponent<MeshRenderer>();
                    _matList = new List<Material>();
                    _matList.Add(m_grassMaterial);
                    _mesh.materials = _matList.ToArray();
                }

                m_currentlySelected = _hit.collider.gameObject;
                m_mainPointer.startColor = Color.blue;
                m_mainPointer.endColor = Color.blue;
                _mesh = _hit.collider.gameObject.GetComponent<MeshRenderer>();
                Material[] _matArray = _mesh.materials;
                _matList = new List<Material>();
                _matList.Add(_matArray[0]);
                _matList.Add(m_outlineMaterial);
                _mesh.materials = _matList.ToArray();

            }

        }
        else // 3
        {
            m_mainPointer.startColor = Color.red;
            m_mainPointer.endColor = Color.red;
            if (m_currentlySelected != null && m_currentlySelected.tag == "Environment")
            {
                MeshRenderer _mesh = m_currentlySelected.GetComponent<MeshRenderer>();
                List<Material> _matList = new List<Material>();
                _matList.Add(m_grassMaterial);
                _mesh.materials = _matList.ToArray();
            }
            m_teleporterPrimed = false;
            m_leftTeleport = false;
            m_rightTeleport = false;
            m_currentlySelected = null;
        }


        //Clicking on an object
        if ((m_leftTrigger == true || m_rightTrigger == true) && m_currentlySelected != null)
        {
            //Debug.Log(m_currentlySelected);
            m_leftTrigger = false;
            m_rightTrigger = false;
        }


        //Teleporting down to tiny size
        if (m_mainTeleport == true && m_teleporterPrimed == false && m_currentlySelected != null)
        {
            Debug.Log("Teleporter Primed");
            m_teleporterPrimed = true;
        }
        if (m_teleporterPrimed == true && m_mainTeleport == false && m_currentlySelected != null)
        {
            Debug.Log("Teleported");
            m_teleporterPrimed = false;
            m_leftTeleport = false;
            m_rightTeleport = false;

            if (m_currentlySelected.tag == "Environment")
            {
                m_vrRig.transform.localScale = Vector3.one;
                Vector3 _newPosition = new Vector3(m_currentlySelected.transform.position.x, m_currentlySelected.transform.position.y + 0.5f, m_currentlySelected.transform.position.z);
                m_vrRig.transform.position = _newPosition;
                Rigidbody _gameEnvRigid = m_gameEnvironment.GetComponent<Rigidbody>();
                _gameEnvRigid.angularVelocity = Vector3.zero;
                m_gameEnvironment.transform.localScale = Vector3.one;
                m_currentSize = SizeMode.small;
            }
            else if (m_currentlySelected.tag == "ResizePlayer" && m_currentSize == SizeMode.small)
            {
                m_vrRig.transform.localScale = new Vector3(20, 20, 20);
                Vector3 _newPosition = new Vector3(0, 0, 0);
                m_vrRig.transform.position = _newPosition;
                m_currentSize = SizeMode.large;
            }
        }
        m_leftTeleport = false;
        m_rightTeleport = false;
        m_leftTrigger = false;
        m_rightTrigger = false;
    }
}
