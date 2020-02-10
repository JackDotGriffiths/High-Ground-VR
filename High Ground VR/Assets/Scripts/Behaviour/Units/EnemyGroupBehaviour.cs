using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGroupBehaviour : MonoBehaviour
{
    [SerializeField,Tooltip("Time between each tick of the enemy group.")] private float m_tickTimer = 3.0f;
    [SerializeField, Tooltip("Movement Speed Between Nodes")] private float m_movementSpeed = 0.4f;

    public int m_currentX;
    public int m_currentY;
    public int m_goalX;
    public int m_goalY;

    private float m_currentTimer;
    private List<Node> m_groupPath;
    private int m_currentStepIndex;
    private Vector3 m_targetPosition;

    void Start()
    {
        m_currentTimer = m_tickTimer;
        m_currentStepIndex = 0;
        m_goalX = GameManager.Instance.GameGemNode.x;
        m_goalY = GameManager.Instance.GameGemNode.y;
        Invoke("RunPathfinding", 0.1f);
    }


    void Update()
    {
        m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;
        if(m_currentTimer < 0)
        {
            m_currentStepIndex++;
            if(m_currentStepIndex == m_groupPath.Count-1)
            {
                m_currentStepIndex = 0;
            }
            m_targetPosition = new Vector3(m_groupPath[m_currentStepIndex].hex.transform.position.x, m_groupPath[m_currentStepIndex].hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.buildingHeightOffset, m_groupPath[m_currentStepIndex].hex.transform.position.z);
            Debug.Log("Tick");
            m_currentTimer = m_tickTimer;
        }
        MoveEnemy();
    }


    /// <summary>
    /// Run the A* pathfinding
    /// </summary>
    void RunPathfinding()
    {
        m_groupPath = new List<Node>();
        var graph = GameBoardGeneration.Instance.Graph;
        var search = new Search(GameBoardGeneration.Instance.Graph);
        search.Start(graph[m_currentX, m_currentY], graph[m_goalX, m_goalY]);
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

        Debug.Log("Search done. Path length : " + search.path.Count + ". Iterations : " + search.iterations);
    }

    /// <summary>
    /// Moves the GameObject towards m_targetPosition. This needs to be called in update to function.
    /// </summary>
    void MoveEnemy()
    {
        float _yOffset = 0;
        if(m_currentStepIndex != 0)
        {
            float _maxDistance = Vector3.Distance(m_groupPath[m_currentStepIndex - 1].hex.transform.position, m_targetPosition);
            float _currentDistance = Vector3.Distance(transform.position, m_targetPosition);
            float _percentage = _currentDistance / _maxDistance;
            _yOffset = Mathf.Sin(_percentage);
        }
        Vector3 _hopPosition = new Vector3(m_targetPosition.x, m_targetPosition.y + _yOffset, m_targetPosition.z);
        transform.position = Vector3.Lerp(transform.position, _hopPosition, m_movementSpeed);
    }


}
