using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager s_instance;
    [Header("Game Config")]
    [SerializeField, Range(0, 1), Tooltip("The speed of objects in the game on a scale of 0-1")] private float m_gameSpeed = 1.0f; //Game speed multiplier used across the game for allowing for slowmo/pausing etc.
    [SerializeField, Tooltip("How long in seconds the building phase should be")] private float m_buildingPhaseTime; //Length of time the player should be allowed to build.

    [Header("Enemy Spawn Management"),Space(10)]
    [Tooltip("Amount of spawns the game rounds should start with")]public int m_enemyStartingSpawns = 1;
    [Tooltip("Amount of enemies to spawn at the start")] public int m_enemyAmount;
    public List<EnemySpawnBehaviour> enemySpawns;
    private List<Node> m_outerEdgeNodes; //Set to the nodes on the outer edge of the map, used when spawning enemies.


    [Header("Gold Management"), Space(10)]
    [SerializeField,Tooltip("Gold the player should start with.")] private int m_startingGold = 10; //Starting gold for the player
    [SerializeField, Tooltip("Amount of time in seconds between each interval of the timer.")] private float m_tickInterval = 1.0f; //Time between each tick of the timer for gold
    [SerializeField, Tooltip("Amount of gold for a player to earn per interval.")] private int m_goldPerTick = 1; //Amount of gold per tick



    [Header ("Debug Wall Displays"), Space(10)]
    [SerializeField] private TextMeshProUGUI m_goldValue; //Debug Wall Text object that temporarily displays money
    [SerializeField] private TextMeshProUGUI m_round; //Debug Wall Text object that temporarily displays round no.
    [SerializeField] private TextMeshProUGUI m_timerLeft; //Debug Wall Text object that temporarily displays time

    public int currentGold; //The current gold of the player.
    private int m_roundCounter;
    private float m_buildingPhaseTimer; //Track the current value of the countdown timer.
    private Node m_gameGemNode; // The Gem Gameobject, set by GameBoardGeneration.

    #region Accessors
    public static GameManager Instance { get => s_instance; set => s_instance = value; }
    public float GameSpeed { get => m_gameSpeed; set => m_gameSpeed = value; }
    public float TickInterval { get => m_tickInterval; set => m_tickInterval = value; }
    public Node GameGemNode { get => m_gameGemNode; set => m_gameGemNode = value; }
    #endregion


    void Awake()
    {
        //Singleton Implementation
        if (s_instance == null)
            s_instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
   
    void Start()
    {
        m_roundCounter = 1;
        currentGold = m_startingGold;
        m_buildingPhaseTimer = 20;

        //Invoke on a delay so GameBoard Graph has been created.
        Invoke("instantiateSpawns",0.1f);
    }

    void Update()
    {
        m_buildingPhaseTimer -= Time.deltaTime * m_gameSpeed;
        if (m_buildingPhaseTimer <= 0.0f)
        {
            Debug.Log("Spawning Enemies");
            spawnEnemies();
            m_buildingPhaseTimer = m_buildingPhaseTime;
        }
        //Updating Debug Displays
        m_goldValue.text = currentGold.ToString();
        m_timerLeft.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString() + "s";


    }


    #region Money
    /// <summary>
    /// Increment the current gold by the amount per tick, all assigned in Game Manager.
    /// </summary>
    public void IncrementGold()
    {
        currentGold += m_goldPerTick;
    }

    public bool spendGold(int _cost)
    {
        if(_cost <= currentGold)
        {
            currentGold -= _cost;
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    #region Enemy Spawning

    /// <summary>
    /// Instantiates the game starting spawns around the edge of the board.
    /// </summary>
    void instantiateSpawns()
    {
        //Check if the amount of spawns in m_enemyStartingSpawns is too large for the border of the game board.
        int _maximumSpawns = (GameBoardGeneration.Instance.Graph.GetLength(0) + GameBoardGeneration.Instance.Graph.GetLength(1)) / 2;
        if (m_enemyStartingSpawns > _maximumSpawns)
        {
            Debug.LogError("There are too many spawns set in the GameManager. Make the board larger or decrease the amount of spawns from " + m_enemyStartingSpawns + " to " + _maximumSpawns);
            return;
        }

        //Generate a list of the nodes on the outside of the Game Board.
        m_outerEdgeNodes = new List<Node>();

        //Bottom and Top Rows of the Game Board
        for (int i = 0; i < GameBoardGeneration.Instance.Graph.GetLength(0)-1; i++)
        {
            m_outerEdgeNodes.Add(GameBoardGeneration.Instance.Graph[i, 0]);
            m_outerEdgeNodes.Add(GameBoardGeneration.Instance.Graph[i, GameBoardGeneration.Instance.Graph.GetLength(1)-1]);
        }

        //Left and Right of the Game Board
        for (int i = 0; i < GameBoardGeneration.Instance.Graph.GetLength(1)-1; i++)
        {
            m_outerEdgeNodes.Add(GameBoardGeneration.Instance.Graph[0, i]);
            m_outerEdgeNodes.Add(GameBoardGeneration.Instance.Graph[GameBoardGeneration.Instance.Graph.GetLength(0)-1, i]);
        }


        //For the amount of m_enemyStartingSpawns
        for (int i = 0; i < m_enemyStartingSpawns; i++)
        {
            //Randomly choose a hex on the OUTSIDE
            Node _chosenNode = m_outerEdgeNodes[Random.Range(0, m_outerEdgeNodes.Count)];
            
            //Verify and place the enemy spawn at this hex
            if (GameBoardGeneration.Instance.BuildingValidation.verifyEnemySpawn(_chosenNode) == true)
            {
                    GameBoardGeneration.Instance.BuildingValidation.placeEnemySpawn(_chosenNode);
            }
            else { 
                i--; 
            }
        }
    }

    /// <summary>
    /// Ran at the end of the timer to spawn enemies.
    /// </summary>
    void spawnEnemies()
    {
        currentGold += 30;
        //Increase the count of enemies based on Enemy Counter;
        m_enemyAmount += Mathf.RoundToInt(m_roundCounter/2);
        m_round.text = "Round " + m_roundCounter;

        //For each enemy to spawn, randomly choose a spawn and run spawnEnemy
        for (int i = 0; i < m_enemyAmount; i++)
        {
            enemySpawns[Random.Range(0, enemySpawns.Count - 1)].spawnEnemy();
        }
        m_roundCounter++;
    }

    #endregion


}
