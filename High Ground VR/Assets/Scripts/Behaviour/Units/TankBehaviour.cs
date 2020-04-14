using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TankBehaviour : MonoBehaviour
{
    [SerializeField, Tooltip("Tank Prefab")] private GameObject m_tankPrefab;
    [SerializeField, Tooltip("Time between each tick of the enemy group.")] private float m_tickTimer = 3.0f;
    [SerializeField, Tooltip("Movement Speed Between Nodes")] private float m_movementSpeed = 0.4f;
    [Range(0.2f, 1.8f), Tooltip("multiplier on the timer. This can be used to speed up/slow down the enemy.")] public float timePerception = 1.0f; //Timeperception is a value that is changed to impact either slowness or speedyness on a player or enemy unit.


    public int currentX;
    public int currentY;
    public bool inCombat;//Tracks whether the unit is in combat or not.
    public bool inSiege;//Tracks whether the unit is currently sieging a building.

    private bool m_tankInstantiated; // Tracks whether the unit has been created and all values appropriately declared.
    private bool m_reachedGoal;
    private bool m_validMove; //Tracks whether the unit should be moving to the next node or staying still.
    private float m_currentTimer; //Value of the timer on the enemy.
    private List<Node> m_groupPath; //List of Nodes that lead to the groups goal.
    private Vector3 m_targetPosition; //Position the node should be moving to.
    public float tankAggression;
    public int currentStepIndex; //Index of the node within the current path.

    public GameObject  m_unit;
    private int m_currentUnits;

    void Start()
    {
        List<GameObject> m_units = new List<GameObject>();
        m_tankInstantiated = false;
        m_validMove = false;
        //Delay allows for player to see the enemy before it starts moving.
        Invoke("InstantiateTank", 0.1f);
    }

    void Update()
    {
        if (m_currentUnits == 0 && m_tankInstantiated == true)
        {
            m_groupPath[currentStepIndex].navigability = nodeTypes.navigable;
            GameManager.Instance.CurrentEnemies -= 1;
            GameManager.Instance.enemyGold();
            Destroy(this.gameObject);
        }

        if (m_tankInstantiated == true && inCombat == false && inSiege == false)
        {
            drawDebug();
            //Run on a tick, timePerception allows the speeding up/slowing down of certain units.
            m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed * timePerception;
            if (m_currentTimer < 0.0f)
            {
                if (m_groupPath.Count == 0 || m_groupPath.Count == 1)
                {
                    m_validMove = false;
                    return;
                }

                //If the next node is the gem, enter into combat with the gem.
                if (m_groupPath[currentStepIndex + 1] == GameManager.Instance.GameGemNode && inSiege == false)
                {
                    //Unit is in an adjecent node to the gem, initiate combat
                    Debug.Log(this + "reached Gem");

                    inSiege = true;
                    //List<Unit> _enemyUnits = new List<Unit>();
                    //_enemyUnits.Add(_gameObj.GetComponent<UnitComponent>().unit);
                    SiegeBehaviour _siege = gameObject.AddComponent<SiegeBehaviour>();
                    //_siege.StartSiege(m_groupPath[currentStepIndex + 1].hex.GetComponentInChildren<BuildingHealth>(), _enemyUnits);
                    //_siege.enemyGroups.Add(this);


                }

                //Reset the timer
                m_currentTimer = m_tickTimer;
                //Reevaluate Pathfinding, as a more efficient route may now be available.
                //RunPathfinding();

                //If the next node is navigable, move the node forward
                if (m_groupPath[currentStepIndex + 1].navigability == nodeTypes.navigable && m_groupPath[currentStepIndex + 1].hex.transform.childCount == 0 && inSiege == false)
                {
                    m_validMove = true;
                    m_groupPath[currentStepIndex + 1].navigability = nodeTypes.enemyUnit;
                    //Update previous and new node with the correct nodeTypes
                    m_groupPath[currentStepIndex].navigability = nodeTypes.navigable;
                    currentStepIndex++;
                    //Sets the parent so scaling works correclty.
                    this.transform.SetParent(m_groupPath[currentStepIndex].hex.transform);
                    //Set the new target Position
                    m_targetPosition = new Vector3(m_groupPath[currentStepIndex].hex.transform.position.x, m_groupPath[currentStepIndex].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[currentStepIndex].hex.transform.position.z);
                    currentX = m_groupPath[currentStepIndex].x;
                    currentY = m_groupPath[currentStepIndex].y;
                }
                else if ((m_groupPath[currentStepIndex + 1].navigability == nodeTypes.wall || m_groupPath[currentStepIndex + 1].navigability == nodeTypes.mine) && inSiege == false)
                {
                    //Start combat with the building in the way.
                    inSiege = true;
                    //List<Unit> _enemyUnits = new List<Unit>();
                    //_enemyUnits.Add(_gameObj.GetComponent<UnitComponent>().unit);
                    SiegeBehaviour _siege = gameObject.AddComponent<SiegeBehaviour>();
                    //_siege.StartSiege(m_groupPath[currentStepIndex + 1].hex.GetComponentInChildren<BuildingHealth>(), _enemyUnits);
                    //_siege.enemyGroups.Add(this);
                }
                else
                {
                    m_validMove = false;
                    return;
                }
            }
        }

        // Check for children ? that will track the units

        if (m_validMove == true)
        {
            MoveEnemy();
        }
    }

    /// <summary>
    /// Creates the unitGroup and starts moving. Creates a bit of a delay so that players have a bit of time to notice the enemy spawning.
    /// </summary>
    void InstantiateTank()
    {
        m_currentTimer = m_tickTimer;
        currentStepIndex = 0;
        inCombat = false;
    
        m_targetPosition = new Vector3(m_groupPath[0].hex.transform.position.x, m_groupPath[0].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[0].hex.transform.position.z);
        m_tankInstantiated = true;
        m_unit = Instantiate(m_tankPrefab, this.transform.position, Quaternion.identity, this.transform);
    }

    /// <summary>
    /// Run the A* pathfinding
    /// </summary>
    public void RunPathfinding()
    {
        currentStepIndex = 0;
        m_groupPath = GameManager.Instance.RunTankPathfinding(GameBoardGeneration.Instance.Graph[currentX, currentY]);
    }

    /// <summary>
    /// Moves the GameObject towards m_targetPosition. This needs to be called in update to function.
    /// </summary>
    void MoveEnemy()
    {
        float _yOffset = 0;
        if (currentStepIndex != 0)
        {
            //Move the enemy along it's path. Calculate distance from the goal as a percentage.
            float _maxDistance = Vector3.Distance(m_groupPath[currentStepIndex - 1].hex.transform.position, m_targetPosition);
            float _currentDistance = Vector3.Distance(transform.position, m_targetPosition);
            float _percentage = _currentDistance / _maxDistance;
            //Sin of the percentage creates a 'hop' like movement.
            _yOffset = 2 * Mathf.Pow(Mathf.Sin(_percentage), 2);
        }
        Vector3 _hopPosition = new Vector3(m_targetPosition.x, m_targetPosition.y + _yOffset, m_targetPosition.z);
        transform.position = Vector3.Lerp(transform.position, _hopPosition, m_movementSpeed * GameManager.Instance.GameSpeed * timePerception);

        RotateTank();
    }

    /// <summary>
    /// Rotate each unit towards the direction they're heading.
    /// </summary>
    void RotateTank()
    {
        if (inCombat == false)
        {
            Vector3 _targetRotation = new Vector3(m_groupPath[currentStepIndex + 1].hex.transform.position.x, m_groupPath[currentStepIndex + 1].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[currentStepIndex + 1].hex.transform.position.z);
            m_unit.transform.LookAt(_targetRotation);
        }
    }

    /// <summary>
    /// Returns true if the node is found within the passed in list. Used to see if the unit is adjecent to the gem.
    /// </summary>
    /// <param name="node">Search Node</param>
    /// <param name="list">List to Search through</param>
    /// <returns></returns>
    public bool FindNode(Node node, List<Node> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (node == list[i])
            {
                return true;
            }
        }
        return false;
    }

    #region Debug + Gizmos
    /// <summary>
    /// Draws the lines of this nodes current route towards the gem.
    /// </summary>
    private void drawDebug()
    {
        Vector3 _startPos;
        Vector3 _endPos;
        for (int i = 1; i < m_groupPath.Count; i++)
        {
            _startPos = new Vector3(m_groupPath[i - 1].hex.transform.position.x, m_groupPath[i - 1].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[i - 1].hex.transform.position.z);
            _endPos = new Vector3(m_groupPath[i].hex.transform.position.x, m_groupPath[i].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[i].hex.transform.position.z);


            Debug.DrawLine(_startPos, _endPos, Color.blue);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 _labelPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        Handles.Label(_labelPos, "Anger: " + tankAggression.ToString("F2"));
        _labelPos = new Vector3(m_unit.transform.position.x, m_unit.transform.position.y + 0.5f, m_unit.transform.position.z);
        Handles.Label(_labelPos, "HP: " + m_unit.GetComponent<UnitComponent>().unit.health.ToString("F2"));


    }
#endif

#endregion
}
