using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnemyBehaviour : MonoBehaviour
{

    [SerializeField, Tooltip("Unit Prefab")] private GameObject m_unitPrefab;
    [SerializeField, Tooltip("Enemy Group Size in number of units")] private int m_groupSize;
    [SerializeField,Tooltip("Time between each tick of the enemy group.")] private float m_tickTimer = 3.0f;
    [SerializeField, Tooltip("Movement Speed Between Nodes")] private float m_movementSpeed = 0.4f;
    [Range(0.2f,1.8f),Tooltip("multiplier on the timer. This can be used to speed up/slow down the enemy.")]public float timePerception = 1.0f; //Timeperception is a value that is changed to impact either slowness or speedyness on a player or enemy unit.
    [SerializeField, Tooltip("Whether or not to spawn with an aggression of 1.")] private bool m_fullAggression;
    [HideInInspector] public float groupAggression; //The aggression of the group/unit.
    [HideInInspector] public int currentStepIndex; //Index of the node within the current path.
    [HideInInspector] public List<GameObject> m_units;


    public int currentX;
    public int currentY;
    public bool inCombat;//Tracks whether the unit is in combat or not.
    public bool inSiege;//Tracks whether the unit is currently sieging a building.

    private bool m_unitInstantiated; // Tracks whether the unit has been created and all values appropriately declared.
    private bool m_reachedGoal;
    private bool m_validMove; //Tracks whether the unit should be moving to the next node or staying still.
    private float m_currentTimer; //Value of the timer on the enemy.
    private List<Node> m_groupPath = new List<Node>(); //List of Nodes that lead to the groups goal.
    private Vector3 m_targetPosition; //Position the node should be moving to.
    private int m_currentUnits;
    private float m_timeAlive;

    void Start()
    {
        if (m_fullAggression)
        {
            groupAggression = 1.0f;
        }
        List<GameObject> m_units = new List<GameObject>();
        m_unitInstantiated = false;
        m_validMove = false;
        //Delay allows for player to see the enemy before it starts moving.
        Invoke("InstantiateUnit", 0.1f);
    }

    void Update()
    {
        //Timer that tracks how long an enemy has lived for. This is used for giving points to the player.
        m_timeAlive += Time.deltaTime * GameManager.Instance.GameSpeed * timePerception;

        //Clear destroyed units from the list of units associated with this group.
        for (int i = 0; i < m_units.Count; i++)
        {
            if (m_units[i] == null)
            {
                m_units.Remove(m_units[i]);
            }
        }
        m_currentUnits = m_units.Count;



        if(m_currentUnits == 0 && m_unitInstantiated == true)
        {
            m_groupPath[currentStepIndex].navigability = nodeTypes.navigable;
            GameManager.Instance.CurrentEnemies -= 1;
            GameManager.Instance.enemyGold();
            //Reward the player for killing the enemies quicker.
            Mathf.Clamp(100 - m_timeAlive, 0, 100);
            GameManager.Instance.addScore(Mathf.RoundToInt(m_timeAlive));
            Destroy(this.gameObject);
        }

        if (m_unitInstantiated == true && inCombat == false && inSiege == false)
        {
            drawDebug();
            //Run on a tick, timePerception allows the speeding up/slowing down of certain units.
            m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed * timePerception;
            if (m_currentTimer < 0.0f)
            {
                if(m_groupPath.Count == 0 || m_groupPath.Count == 1)
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
                    List<Unit> _enemyUnits = new List<Unit>();
                    foreach (GameObject _gameObj in m_units)
                    {
                        _enemyUnits.Add(_gameObj.GetComponent<UnitComponent>().unit);
                    }
                    SiegeBehaviour _siege = gameObject.AddComponent<SiegeBehaviour>();
                    _siege.StartSiege(m_groupPath[currentStepIndex + 1].hex.GetComponentInChildren<BuildingHealth>(), _enemyUnits);
                    _siege.enemyGroups.Add(this);


                }
                //else
                //{
                //    RunPathfinding(enemyTargets.Gem,groupAggression);
                //    m_validMove = false;
                //}

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
                    //Start combat with the building in the way, if they're aggressive enough. Otherwise run pathfinding.
                    float _currentAggressionBoundary = Mathf.Clamp(1.0f - (GameManager.Instance.RoundCounter / 8), 0.6f, 1.0f); //It becomes more likely a unit will get aggressive over time. By round 10, 40% of enemies will be aggressive instantly.
                    if (groupAggression >= _currentAggressionBoundary)
                    {
                        //Start siege
                        inSiege = true;
                        List<Unit> _enemyUnits = new List<Unit>();
                        foreach (GameObject _gameObj in m_units)
                        {
                            _enemyUnits.Add(_gameObj.GetComponent<UnitComponent>().unit);
                        }
                        SiegeBehaviour _siege = gameObject.AddComponent<SiegeBehaviour>();
                        _siege.StartSiege(m_groupPath[currentStepIndex + 1].hex.GetComponentInChildren<BuildingHealth>(), _enemyUnits);
                        _siege.enemyGroups.Add(this);
                    }
                    else
                    {
                        //Pathfind
                        RunPathfinding(groupAggression);
                    }

                }
                else
                {
                    //This renavigates if the unit is stuck, will help them go around combat and slower units.
                    //Debug.Log(this + " enemy stuck.");
                    //Start a timer, increasing the aggression and repathfinding after a set time.
                    StartCoroutine("aggressionTimer");
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
    void InstantiateUnit()
    {
        m_currentTimer = m_tickTimer;
        currentStepIndex = 0;
        inCombat = false;
        GameBoardGeneration.Instance.Graph[currentX,currentY].navigability = nodeTypes.enemyUnit;

        RunPathfinding(groupAggression);
        m_targetPosition = new Vector3(m_groupPath[0].hex.transform.position.x, m_groupPath[0].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[0].hex.transform.position.z);
        m_unitInstantiated = true;

        //Spawn the unit prefab
        m_currentUnits = m_groupSize;
        Node _spawnNode = GameBoardGeneration.Instance.Graph[currentX, currentY];
        for (int i = 0; i < m_groupSize; i++)
        {
            Vector3 _unitSpawnPos = new Vector3(_spawnNode.hex.transform.position.x, _spawnNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, _spawnNode.hex.transform.position.z);
            GameObject _newUnit = Instantiate(m_unitPrefab, _unitSpawnPos, transform.rotation, this.transform);
            _newUnit.GetComponent<UnitComponent>().enemyUnitConstructor();
            m_units.Add(_newUnit);
        }


        if(m_groupSize != 1)
        {
            //Divide the hex into angles, based on the amount of units associated with this barracks.
            float _angleDifference = 360 / (m_currentUnits + 1);
            int _index = 0;
            foreach (GameObject _gameObj in m_units)
            {
                //Place the unit at a certain position along that angle, dividing the units into equal sectors of the hexagon.
                Quaternion _angle = Quaternion.Euler(0, _angleDifference * (_index + 1), 0);
                _gameObj.transform.position += (_angle * (Vector3.forward * 0.4f));
                _index++;
            }

        }
    }

    /// <summary>
    /// Run the A* pathfinding
    /// </summary>
    public void RunPathfinding(float _aggression)
    {
        currentStepIndex = 0;
        m_groupPath = GameManager.Instance.RunPathfinding(GameBoardGeneration.Instance.Graph[currentX,currentY],GameManager.Instance.GameGemNode,_aggression);
    }

    /// <summary>
    /// Moves the GameObject towards m_targetPosition. This needs to be called in update to function.
    /// </summary>
    void MoveEnemy()
    {
        float _yOffset = 0;
        if(currentStepIndex != 0)
        {
            //Move the enemy along it's path. Calculate distance from the goal as a percentage.
            float _maxDistance = Vector3.Distance(m_groupPath[currentStepIndex - 1].hex.transform.position, m_targetPosition);
            float _currentDistance = Vector3.Distance(transform.position, m_targetPosition);
            float _percentage = _currentDistance / _maxDistance;
            //Sin of the percentage creates a 'hop' like movement.
            _yOffset = 2 * Mathf.Pow(Mathf.Sin(_percentage),2);
        }
        Vector3 _hopPosition = new Vector3(m_targetPosition.x, m_targetPosition.y + _yOffset, m_targetPosition.z);
        transform.position = Vector3.Lerp(transform.position, _hopPosition, m_movementSpeed * GameManager.Instance.GameSpeed * timePerception);

        try
        {
            RotateEachUnit();
        }
        catch { }
    }

    /// <summary>
    /// Rotate each unit towards the direction they're heading.
    /// </summary>
    void RotateEachUnit()
    {
        if(inCombat == false && inSiege == false)
        {
            for (int i = 0; i < m_units.Count; i++)
            {
                Vector3 _targetRotation = new Vector3(m_groupPath[currentStepIndex + 1].hex.transform.position.x, m_groupPath[currentStepIndex + 1].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[currentStepIndex + 1].hex.transform.position.z);
                m_units[i].transform.LookAt(_targetRotation);
            }

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


    /// <summary>
    /// Increases the aggression of the enemy. This is used when the enemy is stuck.
    /// </summary>
    /// <returns></returns>
    IEnumerator aggressionTimer()
    {
        groupAggression = Mathf.Clamp(groupAggression + 0.02f, 0, 0.9f);
        yield return new WaitForSeconds(2.0f);
        RunPathfinding(groupAggression);
        yield return null;
    }



    #region Debug + Gizmos
    /// <summary>
    /// Draws the lines of this nodes current route towards the gem.
    /// </summary>
    private void drawDebug()
    {
        Vector3 _startPos;
        Vector3 _endPos;
        if(m_groupPath.Count > 1)
        {
            for (int i = 1; i < m_groupPath.Count; i++)
            {
                _startPos = new Vector3(m_groupPath[i - 1].hex.transform.position.x, m_groupPath[i - 1].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[i - 1].hex.transform.position.z);
                _endPos = new Vector3(m_groupPath[i].hex.transform.position.x, m_groupPath[i].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[i].hex.transform.position.z);


                Debug.DrawLine(_startPos, _endPos, Color.blue);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 _labelPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        Handles.Label(_labelPos, "Anger: " + groupAggression.ToString("F2"));

        int _count = 1;
        for (int i = 0; i < m_units.Count; i++)
        {
            try
            {
                _labelPos = new Vector3(m_units[i].transform.position.x, m_units[i].transform.position.y + 0.3f, m_units[i].transform.position.z);
                Handles.Label(_labelPos, _count.ToString());

                float _health = m_units[i].GetComponent<UnitComponent>().unit.health;
                _labelPos = new Vector3(m_units[i].transform.position.x, m_units[i].transform.position.y + 0.5f, m_units[i].transform.position.z);
                Handles.Label(_labelPos, "HP: " + _health.ToString("F2"));
                _count++;
            }
            catch { }

        }

    }
#endif

#endregion

}
