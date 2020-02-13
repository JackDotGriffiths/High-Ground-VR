using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupBehaviour : MonoBehaviour
{

    [SerializeField, Tooltip("Unit Prefab")] private GameObject m_unitPrefab;
    [SerializeField, Tooltip("Enemy Group Size in number of units")] private int m_groupSize;
    [SerializeField,Tooltip("Time between each tick of the enemy group.")] private float m_tickTimer = 3.0f;
    [SerializeField, Tooltip("Movement Speed Between Nodes")] private float m_movementSpeed = 0.4f;
    [Range(0.2f,1.8f),Tooltip("multiplier on the timer. This can be used to speed up/slow down the enemy.")]public float timePerception = 1.0f; //Timeperception is a value that is changed to impact either slowness or speedyness on a player or enemy unit.


    public int currentX;
    public int currentY;
    public int goalX;
    public int goalY;
    public bool inCombat;//Tracks whether the unit is in combat or not.

    private bool m_unitInstantiated; // Tracks whether the unit has been created and all values appropriately declared.
    private bool m_reachedGoal;
    private bool m_validMove; //Tracks whether the unit should be moving to the next node or staying still.
    private float m_currentTimer; //Value of the timer on the enemy.
    private List<Node> m_groupPath; //List of Nodes that lead to the groups goal.
    private int m_currentStepIndex; //Index of the node within the current path.
    private Vector3 m_targetPosition; //Position the node should be moving to.


    public List<GameObject> m_units;
    private int m_currentUnits;

    void Start()
    {
        m_unitInstantiated = false;
        m_validMove = false;
        //Delay allows for player to see the enemy before it starts moving.
        Invoke("InstantiateUnit", 0.1f);
    }

    void Update()
    {
        if (m_unitInstantiated == true && inCombat == false)
        {
            drawDebugLines();
            //Run on a tick, timePerception allows the speeding up/slowing down of certain units.
            m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed * timePerception;
            if (m_currentTimer < 0.0f)
            {
                if(m_groupPath.Count == 0)
                {
                    RunPathfinding(0.0f);
                    return;
                }

                //If the next node is the gem, enter into combat with the gem.
                if (m_groupPath[m_currentStepIndex + 1] == GameManager.Instance.GameGemNode)
                {
                    //Unit is in an adjecent node to the gem, initiate combat
                    m_groupPath[m_currentStepIndex].navigability = navigabilityStates.navigable;
                    Debug.Log(this + "reached Gem");
                    Destroy(this.gameObject);
                }

                //Reset the timer
                m_currentTimer = m_tickTimer;
                //Reevaluate Pathfinding, as a more efficient route may now be available.
                //RunPathfinding();

                //If the next node is navigable, move the node forward
                if (m_groupPath[m_currentStepIndex + 1].navigability == navigabilityStates.navigable)
                {
                    m_validMove = true;
                    //Update previous and new node with the correct navigabilityStates
                    m_groupPath[m_currentStepIndex].navigability = navigabilityStates.navigable;
                    m_groupPath[m_currentStepIndex + 1].navigability = navigabilityStates.enemyUnit;
                    m_currentStepIndex++;
                    //Sets the parent so scaling works correclty.
                    this.transform.SetParent(m_groupPath[m_currentStepIndex].hex.transform);
                    m_groupPath[m_currentStepIndex].navigability = navigabilityStates.enemyUnit;
                    //Set the new target Position
                    m_targetPosition = new Vector3(m_groupPath[m_currentStepIndex].hex.transform.position.x, m_groupPath[m_currentStepIndex].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[m_currentStepIndex].hex.transform.position.z);
                    currentX = m_groupPath[m_currentStepIndex].x;
                    currentY = m_groupPath[m_currentStepIndex].y;
                }
                else
                {
                    //This renavigates if the unit is stuck, will help them go around combat and slower units.
                    Debug.Log(this + " enemy stuck.");
                    RunPathfinding(0.0f);
                    m_validMove = false;
                    return;
                }
            }

            ////TEMPORARY - If the unit encounters a destructable object, destroy it. This is only for demo purposes.
            //if(m_groupPath[m_currentStepIndex].navigability == navigabilityStates.destructable)
            //{
            //    //m_groupPath[m_currentStepIndex-1].navigability = navigabilityStates.navigable;
            //    m_groupPath[m_currentStepIndex].navigability = navigabilityStates.navigable;
            //    Destroy(m_groupPath[m_currentStepIndex].hex.transform.GetChild(0).gameObject);
            //    Destroy(this.gameObject);
            //}

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
        m_currentStepIndex = 0;
        inCombat = false;
        GameBoardGeneration.Instance.Graph[currentX,currentY].navigability = navigabilityStates.enemyUnit;
        goalX = GameManager.Instance.GameGemNode.x;
        goalY = GameManager.Instance.GameGemNode.y;
        RunPathfinding(1.0f);
        m_targetPosition = new Vector3(m_groupPath[0].hex.transform.position.x, m_groupPath[0].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_groupPath[0].hex.transform.position.z);
        m_unitInstantiated = true;

        //Spawn the unit prefab
        m_currentUnits = m_groupSize;
        m_units = new List<GameObject>();
        Node _spawnNode = GameBoardGeneration.Instance.Graph[currentX, currentY];
        for (int i = 0; i < m_groupSize; i++)
        {
            Vector3 _unitSpawnPos = new Vector3(_spawnNode.hex.transform.position.x, _spawnNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, _spawnNode.hex.transform.position.z);
            GameObject _newUnit = Instantiate(m_unitPrefab, _unitSpawnPos, transform.rotation, this.transform);
            _newUnit.GetComponent<UnitComponent>().enemyUnitConstructor();
            m_units.Add(_newUnit);
        }

        //Divide the hex into angles, based on the amount of units associated with this barracks.
        float _angleDifference = 360 / (m_currentUnits + 1);
        int _index = 0;
        float _multiplier = 1;
        foreach (GameObject _gameObj in m_units)
        {
            //Place the unit at a certain position along that angle, dividing the units into equal sectors of the hexagon.
            Quaternion _angle = Quaternion.Euler(0, _angleDifference * (_index + 1), 0);

            if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.small)
            {
                _multiplier = InputManager.Instance.LargestScale.y + 20;
            }
            _gameObj.transform.position += (_angle * (Vector3.forward * 0.4f) * _multiplier);
            _index++;
        }


    }

    /// <summary>
    /// Run the A* pathfinding
    /// </summary>
    void RunPathfinding(float _aggression)
    {
        if (currentX == goalX && currentY == goalY)
        {
            return;
        }
        m_groupPath = new List<Node>();
        m_currentStepIndex = 0;
        var graph = GameBoardGeneration.Instance.Graph;
        var search = new Search(GameBoardGeneration.Instance.Graph);
        search.Start(graph[currentX, currentY], graph[goalX, goalY],_aggression);
        while (!search.finished)
        {
            search.Step();
        }

        Transform[] _pathPositions = new Transform[search.path.Count];
        for (int i = 0; i < search.path.Count; i++)
        {
            m_groupPath.Add(search.path[i]);
        }

        if (search.path.Count == 0)
        {
            Debug.Log("Search Failed");
            return;
        }

        //Debug.Log("Search done. Path length : " + search.path.Count + ". Iterations : " + search.iterations);
    }

    /// <summary>
    /// Moves the GameObject towards m_targetPosition. This needs to be called in update to function.
    /// </summary>
    void MoveEnemy()
    {
        float _yOffset = 0;
        if(m_currentStepIndex != 0)
        {
            //Move the enemy along it's path. Calculate distance from the goal as a percentage.
            float _maxDistance = Vector3.Distance(m_groupPath[m_currentStepIndex - 1].hex.transform.position, m_targetPosition);
            float _currentDistance = Vector3.Distance(transform.position, m_targetPosition);
            float _percentage = _currentDistance / _maxDistance;
            //Sin of the percentage creates a 'hop' like movement.
            _yOffset = Mathf.Pow(Mathf.Sin(_percentage),2);
        }
        Vector3 _hopPosition = new Vector3(m_targetPosition.x, m_targetPosition.y + _yOffset, m_targetPosition.z);
        transform.position = Vector3.Lerp(transform.position, _hopPosition, m_movementSpeed * timePerception);
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
    /// Draws the lines of this nodes current route towards the gem.
    /// </summary>
    private void drawDebugLines()
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




}
