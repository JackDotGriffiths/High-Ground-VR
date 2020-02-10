using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager s_instance;
    [Header("Game Config")]
    [SerializeField, Range(0, 1), Tooltip("The speed of objects in the game on a scale of 0-1")] private float m_gameSpeed = 1.0f; //Game speed multiplier used across the game for allowing for slowmo/pausing etc.
    [SerializeField] private GameBoardGeneration m_gameEnvironment;
    [SerializeField] private ValidateBuildingLocation m_buildingValidation;
    public GameObject gameGem;

    [Header("Round Management"),Space(10)]
    [Tooltip("Amount of spawns the game rounds should start with")]public int m_enemyStartingSpawns;
    private List<Node> m_outerEdgeNodes; //Set to the nodes on the outer edge of the map, used when spawning enemies.


    [Header("Gold Management"), Space(10)]
    [SerializeField,Tooltip("Gold the player should start with.")] private int m_startingGold = 10; //Starting gold for the player
    [SerializeField, Tooltip("Amount of time in seconds between each interval of the timer.")] private float m_tickInterval = 1.0f; //Time between each tick of the timer for gold
    [SerializeField, Tooltip("Amount of gold for a player to earn per interval.")] private int m_goldPerTick = 1; //Amount of gold per tick



    [Header ("Debug Wall Displays"), Space(10)]
    [SerializeField] private TextMeshProUGUI m_goldValue; //Debug Wall Text object that temporarily displays money


    private int m_currentGold; //The current gold of the player.





    #region Accessors
    public static GameManager Instance { get => s_instance; set => s_instance = value; }
    public float GameSpeed { get => m_gameSpeed; set => m_gameSpeed = value; }
    public int Money { get => m_currentGold;}
    public float TickInterval { get => m_tickInterval; set => m_tickInterval = value; }
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
        m_currentGold = m_startingGold;


        //Invoke on a delay so GameBoard Graph has been created.
        Invoke("instantiateSpawns",0.1f);
    }



    void Update()
    {
        try { m_goldValue.text = m_currentGold.ToString(); } catch { Debug.LogWarning("Debug Wall Gold text not set in GameManager."); }
    }


    #region Money
    /// <summary>
    /// Increment the current gold by the amount per tick, all assigned in Game Manager.
    /// </summary>
    public void IncrementGold()
    {
        m_currentGold += m_goldPerTick;
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
            if (m_buildingValidation.verifyEnemySpawn(_chosenNode) == true)
            {
                m_buildingValidation.placeEnemySpawn(_chosenNode);
            }
            else { 
                i--; 
            }
        }
    }

    #endregion


}
