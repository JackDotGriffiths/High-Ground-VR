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
    private float m_maxWorldHeight; //This depends on the player's height.
    private SizeMode m_currentSize;


    [Header("World Rotation")]
    [SerializeField, Space(10)] private float m_rotationSensitivity;
    [SerializeField] private float m_rotationMagnitude;

    [Header("Point & Click")]
    [SerializeField, Space(10)] private Material m_pointerMaterial;
    [SerializeField] private Material m_grassMaterial, m_outlineMaterial;
    private GameObject m_currentlySelectedNode;

    [Header("Teleportation")]
    [SerializeField, Space(10)] private float m_rigTeleportationSpeed;
    [SerializeField] private Vector3 m_smallestScale, m_largestScale;
    private bool m_teleporterPrimed;
    private bool m_enlargePlayer;
    private Vector3 m_newPosition;

    private bool m_leftGripped, m_rightGripped, m_leftTrigger, m_rightTrigger, m_leftTeleport, m_rightTeleport, m_mainTrigger, m_mainTeleport;
    private Vector3 m_prevControllerMidpoint, m_controllerMidpoint;
    private Vector3 m_mainControllerPos, m_mainControllerPreviousPos;
    private float m_prevControllerDistance, m_controllerDistance;


    private GameObject m_mainController, m_offHandController;
    private LineRenderer m_mainPointer;
    List<MeshRenderer> _objectMeshes = new List<MeshRenderer>();


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
        m_mainPointer.sortingOrder = 1;

        //Setting the height based on the players height
        updateWorldHeight();

        m_currentSize = SizeMode.large;
        m_gameEnvironment.transform.localScale = m_smallestScale;
        m_teleporterPrimed = false;

        //Add all of the environment object mesh renderers.
        updateObjectList();

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
        //Taking Handedness into consideration.
        if ((m_leftTrigger == true && m_mainController == m_leftController) || (m_rightTrigger == true && m_mainController == m_rightController)) { m_mainTrigger = true; }
        else { m_mainTrigger = false; }
        if ((m_leftTeleport == true && m_mainController == m_leftController) || (m_rightTeleport == true && m_mainController == m_rightController)) { m_mainTeleport = true; }
        else { m_mainTeleport = false; }



        #region Trigger Button Handling

        m_mainControllerPos = m_mainController.transform.position;
        //Rotating the Game Board
        if (m_mainTrigger == true && !(m_rightTrigger == true && m_leftTrigger == true) && m_currentSize == SizeMode.large)
        {
            Rigidbody _gameEnvRigid = m_gameEnvironment.GetComponent<Rigidbody>();
            Vector3 _forceVector = m_mainControllerPos - m_mainControllerPreviousPos;
            if (_forceVector.magnitude > m_rotationSensitivity)
            {
                _gameEnvRigid.AddForceAtPosition(_forceVector * m_rotationMagnitude, m_mainControllerPreviousPos, ForceMode.Impulse);
            }
            else
            {
                _gameEnvRigid.angularVelocity = Vector3.Lerp(_gameEnvRigid.angularVelocity, Vector3.zero, 0.5f);
            }

        }
        m_mainControllerPreviousPos = m_mainControllerPos;
        #endregion

        #region Pointer Handling


        //Raycasting from the controllers
        RaycastHit _hit;
        if (Physics.Raycast(m_mainController.transform.position, m_mainController.transform.forward, out _hit, 1000))
        {
            if (_hit.collider.gameObject.tag == "Environment")
            {
                m_mainPointer.SetPosition(0, m_mainController.transform.position);
                m_mainPointer.SetPosition(1, _hit.point);
                m_enlargePlayer = false;
                MeshRenderer _hitMesh = _hit.collider.gameObject.GetComponent<MeshRenderer>();
                m_currentlySelectedNode = _hit.collider.gameObject;
                //Removes highlight from all objects not in the _objectMeshes list
                updateObjectList(_hitMesh);

                //Turn laser colour to blue when you correctly collided with environment.
                m_mainPointer.startColor = Color.blue;
                m_mainPointer.endColor = Color.blue;

                //Apply outline material to the selected object
                Material[] _matArray = _hitMesh.materials;
                List<Material> _matList = new List<Material>();
                _matList = new List<Material>();
                _matList.Add(_matArray[0]);
                _matList.Add(m_outlineMaterial);
                _hitMesh.materials = _matList.ToArray();
            }
        }
        else
        {
            m_mainPointer.SetPosition(0, m_mainController.transform.position);
            m_mainPointer.SetPosition(1, m_mainController.transform.position + m_mainController.transform.forward * 100);
            m_currentlySelectedNode = null;
            m_enlargePlayer = true;
            //Turn the laser colour red.
            m_mainPointer.startColor = Color.red;
            m_mainPointer.endColor = Color.red;

            ////Removes highlight from all objects
            updateObjectList();

        }

        removeHighlight();


        /////Clicking on an node, will be used alongside the UI to place buildings
        if ((m_leftTrigger == true || m_rightTrigger == true) && m_currentlySelectedNode != null)
        {
            Debug.Log(m_currentlySelectedNode);
            m_leftTrigger = false;
            m_rightTrigger = false;
        }

        #endregion

        #region Teleporting
        //Teleporting down to tiny size
        if (m_mainTeleport == true && m_teleporterPrimed == false)
        {
            //Make the line RED or GREEN depending on whether teleport is allowed?
            Debug.Log("Teleporter Primed");
            m_teleporterPrimed = true;
        }



        if (m_teleporterPrimed == true && m_mainTeleport == false && m_currentlySelectedNode != null && m_enlargePlayer == false)
        {
            //Teleport onto the game board
            Debug.Log("Teleported");
            m_teleporterPrimed = false;
            //Scale the game environment
            m_gameEnvironment.transform.localScale = m_largestScale;
            if (m_gameEnvironment.transform.position.y != 1)
            {
                m_gameEnvironment.transform.position = new Vector3(m_gameEnvironment.transform.position.x, 1, m_gameEnvironment.transform.position.z);
            }
            m_newPosition = new Vector3(m_currentlySelectedNode.transform.position.x, 1 + m_largestScale.x, m_currentlySelectedNode.transform.position.z);


            Rigidbody _gameEnvRigid = m_gameEnvironment.GetComponent<Rigidbody>();
            _gameEnvRigid.angularVelocity = Vector3.zero;
            _gameEnvRigid.velocity = Vector3.zero;
            m_currentSize = SizeMode.small;
            m_mainPointer.startWidth = 0.05f / m_largestScale.x;
            m_mainPointer.endWidth = 0.00f;
        }
        if (m_teleporterPrimed == true && m_mainTeleport == false && m_currentlySelectedNode == null && m_enlargePlayer == true)
        {
            //Teleport off the game board.
            Debug.Log("Teleported");
            m_teleporterPrimed = false;

            m_newPosition = new Vector3(0, 0, 0);
            m_gameEnvironment.transform.localScale = m_smallestScale;
            m_gameEnvironment.transform.position = new Vector3(0, m_maxWorldHeight, 0);


            m_currentSize = SizeMode.large;
            m_mainPointer.startWidth = 0.05f;
            m_mainPointer.endWidth = 0.00f;
        }
        #endregion

        m_vrRig.transform.position = Vector3.Lerp(m_vrRig.transform.position, m_newPosition, m_rigTeleportationSpeed);

        m_leftTeleport = false;
        m_rightTeleport = false;
        m_leftTrigger = false;
        m_rightTrigger = false;
    }

    [ContextMenu("Update Player Height")]
    public void updateWorldHeight()
    {
        m_maxWorldHeight = m_camera.transform.position.y * 0.6f;
        m_gameEnvironment.transform.position = new Vector3(0, m_maxWorldHeight, 0);
    }
    private void updateObjectList(MeshRenderer _selectedMesh)
    {
        _objectMeshes = new List<MeshRenderer>();
        //Add all of the environment object mesh renderers.
        int _childCount = m_gameEnvironment.transform.childCount;
        for (int i = 0; i < _childCount; i++)
        {
            if (m_gameEnvironment.transform.GetChild(i).tag == "Environment")
            {
                MeshRenderer _mesh = m_gameEnvironment.transform.GetChild(i).GetComponent<MeshRenderer>();
                if (_mesh != _selectedMesh)
                {
                    _objectMeshes.Add(_mesh);
                }
            }
        }
    }
    private void updateObjectList()
    {
        _objectMeshes = new List<MeshRenderer>();
        //Add all of the environment object mesh renderers.
        int _childCount = m_gameEnvironment.transform.childCount;
        for (int i = 0; i < _childCount; i++)
        {
            if(m_gameEnvironment.transform.GetChild(i).tag == "Environment")
            {
                MeshRenderer _mesh = m_gameEnvironment.transform.GetChild(i).GetComponent<MeshRenderer>();
                _objectMeshes.Add(_mesh);
            }
        }
    }
    private void removeHighlight()
    {
        foreach (MeshRenderer _mesh in _objectMeshes)
        {
            List<Material> _matList = new List<Material>();
            _matList.Add(m_grassMaterial);
            _mesh.materials = _matList.ToArray();
        }
    }
}
