using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
enum Phases { Building, Attack };
public class GameManager : MonoBehaviour
{
    private static GameManager s_instance;
    [Header("Game Config")]
    [SerializeField, Range(0, 1), Tooltip("The speed of objects in the game on a scale of 0-1")] private float m_gameSpeed = 1.0f; //Game speed multiplier used across the game for allowing for slowmo/pausing etc.
    [SerializeField, Tooltip("How long in seconds the building phase should be")] private float m_buildingPhaseTime; //Length of time the player should be allowed to build.
    private bool m_gameOver = false;

    [Header("Enemy Spawn Management"),Space(10)]
    [Tooltip("Amount of spawns the game rounds should start with")]public int m_enemyStartingSpawns = 1;
    [Tooltip("Amount of enemies to spawn at the start")] public int m_enemyAmount;
    [Tooltip("Enemy Spawn Delay")] public int m_enemySpawnDelay;
    public List<EnemySpawnBehaviour> enemySpawns;
    private List<Node> m_outerEdgeNodes; //Set to the nodes on the outer edge of the map, used when spawning enemies.
    private int m_currentEnemies;


    [Header("Gold Management"), Space(10)]
    [SerializeField,Tooltip("Gold the player should start with.")] private int m_startingGold = 10; //Starting gold for the player
    [SerializeField, Tooltip("Amount of gold for a player to earn per interval.")] private int m_minedGoldPerRound = 20; //Amount of gold per tick
    [SerializeField, Tooltip("Amount of gold for a player to earn from killing an enemy")] private int m_goldPerKill = 20; //Amount of enemyKilled
    private int m_mineCount;


    [HideInInspector] public TextMeshProUGUI moneyBookText, timerBookText;

    [Header ("Debug Wall Displays"), Space(10)]
    [SerializeField] private TextMeshProUGUI m_goldValue; //Debug Wall Text object that temporarily displays money
    [SerializeField] private TextMeshProUGUI m_round; //Debug Wall Text object that temporarily displays round no.
    [SerializeField] private TextMeshProUGUI m_timerLeft; //Debug Wall Text object that temporarily displays time


    public int currentGold; //The current gold of the player.
    private int m_roundCounter;
    private Phases m_currentPhase;
    private float m_buildingPhaseTimer; //Track the current value of the countdown timer.
    private Node m_gameGemNode; // The Gem Gameobject, set by GameBoardGeneration.

    #region Accessors
    public static GameManager Instance { get => s_instance; set => s_instance = value; }
    public float GameSpeed { get => m_gameSpeed; set => m_gameSpeed = value; }
    public Node GameGemNode { get => m_gameGemNode; set => m_gameGemNode = value; }
    internal Phases CurrentPhase { get => m_currentPhase; set => m_currentPhase = value; }
    public bool GameOver { get => m_gameOver; set => m_gameOver = value; }
    public int CurrentEnemies { get => m_currentEnemies; set => m_currentEnemies = value; }
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
        m_buildingPhaseTimer = m_buildingPhaseTime;
        CurrentPhase = Phases.Building;

        //Invoke on a delay so GameBoard Graph has been created.
        Invoke("instantiateGem", 0.05f);
        Invoke("instantiateSpawns", 0.05f);

    }

    void Update()
    {
        if (m_gameOver == true)
        {
            m_round.text = "GAME OVER";
            m_gameSpeed = 0;
            return;
        }
        if(m_currentEnemies <= 0 && CurrentPhase == Phases.Building)
        {
            m_buildingPhaseTimer -= Time.deltaTime * m_gameSpeed;
            if (m_buildingPhaseTimer <= 0.0f)
            {
                    StartAttackPhase();
            }
        }
        if(m_currentEnemies <=0 && CurrentPhase == Phases.Attack)
        {
            StartBuildingPhase();
        }
        //Updating Debug Displays
        m_goldValue.text = currentGold.ToString();
        m_timerLeft.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString() + "s";

        //Update Book Display
        try
        {
            moneyBookText.text = currentGold.ToString();
            timerBookText.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString();
        }
        catch { }
    }

    #region Phase Control
    void StartBuildingPhase()
    {
        CurrentPhase = Phases.Building;
        //If playing in the Scene view, the book will not exist.
        try
        {
            BookManager.Instance.HideActions();
        }
        catch { }
        m_round.text = "Building";
        currentGold += (m_mineCount * m_minedGoldPerRound) + 30;
        //Set all adjecent nodes to the spawns to nonPlaceable, so the player cannot build around them.
        foreach (EnemySpawnBehaviour _spawn in GameManager.Instance.enemySpawns)
        {
            List<Node> _adjNodes = _spawn.gameObject.GetComponentInParent<NodeComponent>().node.adjecant;
            foreach (Node _node in _adjNodes)
            {
                _node.navigability = navigabilityStates.nonPlaceable;
            }
        }
        m_buildingPhaseTimer = m_buildingPhaseTime/2;
    }
    void StartAttackPhase()
    {
        //If playing in the Scene view, the book will not exist.
        try
        {
            BookManager.Instance.ShowActions();
        }
        catch { }
        CurrentPhase = Phases.Attack;
        //Set all adjecent nodes to the spawns to nonPlaceable, so the player cannot build around them.
        foreach (EnemySpawnBehaviour _spawn in GameManager.Instance.enemySpawns)
        {
            List<Node> _adjNodes = _spawn.gameObject.GetComponentInParent<NodeComponent>().node.adjecant;
            foreach (Node _node in _adjNodes)
            {
                _node.navigability = navigabilityStates.navigable;
            }
        }
        StartCoroutine("spawnEnemies");


    }
    #endregion

    #region Gem Control
    private void instantiateGem()
    {
        GameBoardGeneration.Instance.placeGem();
    }
    #endregion

    #region Money Control
    /// <summary>
    /// Increment the current gold by the amount per tick, all assigned in Game Manager.
    /// </summary>
    public void IncrementMines()
    {
        m_mineCount += 1;
    }

    /// <summary>
    /// Takes money away from the players balance
    /// </summary>
    /// <param name="_cost">Cost of the proposed purchase.</param>
    /// <returns>True/False depending on whether the transaction went through</returns>
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


    /// <summary>
    /// Adds money for the player killing an enemy.
    /// </summary>
    public void enemyGold()
    {
        currentGold += m_goldPerKill;
    }
    #endregion

    #region Enemy Spawning Control

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
            if (GameBoardGeneration.Instance.BuildingValidation.verifyEnemySpawn(_chosenNode, 0.0f) == true)
            {
                    GameBoardGeneration.Instance.BuildingValidation.placeEnemySpawn(_chosenNode, 0.0f);
            }
            else { 
                i--; 
            }
        }
    }

    /// <summary>
    /// Ran at the end of the timer to spawn enemies.
    /// </summary>
    IEnumerator spawnEnemies()
    {
        if(m_gameOver == false)
        {
            //Increase the count of enemies based on Enemy Counter;
            m_enemyAmount += Mathf.RoundToInt(Mathf.Pow((float)m_roundCounter,2.0f)/2);



            CurrentEnemies = m_enemyAmount;
            m_round.text = "Round " + m_roundCounter;

            //For each enemy to spawn, randomly choose a spawn and run spawnEnemy
            for (int i = 0; i < m_enemyAmount; i++)
            {
                if (!enemySpawns[Random.Range(0, enemySpawns.Count)].spawnEnemy()) { i--; }; //If it fails to spawn an enemy, try again.
                yield return new WaitForSeconds(Random.Range(m_enemySpawnDelay/2,m_enemySpawnDelay));
            }
            m_roundCounter++;
        }
    }


  

    #endregion


}
