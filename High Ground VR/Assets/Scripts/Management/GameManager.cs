﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
enum Phases { Building, Attack };
public enum enemyTargets {Gem,randomMine,randomDestructableBuilding};
enum enemyTypes { Regular,Tank};
public class GameManager : MonoBehaviour
{
    #region Variables
    private static GameManager s_instance;
    [Header("Game Config")]
    [SerializeField, Range(0.0f, 2.0f), Tooltip("The speed of objects in the game on a scale of 0-1")] private float m_gameSpeed = 1.0f; //Game speed multiplier used across the game for allowing for slowmo/pausing etc.
    [SerializeField, Tooltip("How long in seconds the building phase should be")] private float m_buildingPhaseTime; //Length of time the player should be allowed to build.
    [SerializeField, Tooltip("The animator of the Main Menu. Used to show/hide it.")] private Animator m_mainMenuAnim;
    private bool m_gameOver = false;

    [Header("Enemy Spawn Management"),Space(10)]
    [Tooltip("Amount of spawns the game rounds should start with")]public int enemyStartingSpawns = 1;
    [Tooltip("Amount of enemies to spawn at the start of Round One.")] public int enemyAmount = 1;
    [Tooltip("Enemy Spawn Delay")] public int enemySpawnDelay = 10;
    [Tooltip("Which round to start spawning enemies after")] public int spawnAggresiveAfter = 2;
    [Tooltip("Proportion of aggressive enemies. This is multiplied by the round number."),Range(0.0f,1.0f)] public float aggressionPercentage = 0.1f;
    [Tooltip("Which round number to start spawning tank enemies after"), Space(10)] public int tankRoundStart = 5;
    [Tooltip("How many rounds have to pass before another one spawns")] public int tankRoundFrequency = 5;

    [HideInInspector]public List<EnemySpawnBehaviour> enemySpawns;
    private List<Node> m_outerEdgeNodes; //Set to the nodes on the outer edge of the map, used when spawning enemies.
    private int m_currentEnemies;
    private int m_tankCount = 1;


    [Header("Gold Management"), Space(10)]
    [SerializeField,Tooltip("Gold the player should start with.")] private int m_startingGold = 10; //Starting gold for the player
    [SerializeField, Tooltip("Amount of gold for a player to earn per interval.")] private int m_minedGoldPerRound = 20; //Amount of gold per tick
    [SerializeField, Tooltip("Amount of gold for a player to earn from killing an enemy")] private int m_goldPerKill = 20; //Amount of enemyKilled

    public int wallsCost = 10; //Cost of placing a wall
    public int barracksCost = 100; // Cost of placing a barracks
    public int mineCost = 50; //Cost of placing a mine
    private int m_mineCount;


    [HideInInspector] public TextMeshProUGUI moneyBookText, timerBookText, moneyBookText2, timerBookText2;

    [Header ("Debug Wall Displays"), Space(10)]
    [SerializeField] private TextMeshProUGUI m_goldValue; //Debug Wall Text object that temporarily displays money
    [SerializeField] private TextMeshProUGUI m_round; //Debug Wall Text object that temporarily displays round no.
    [SerializeField] private TextMeshProUGUI m_timerLeft; //Debug Wall Text object that temporarily displays time


    public int currentGold; //The current gold of the player.
    private int m_roundCounter;
    private float m_buildingPhaseTimer; //Track the current value of the countdown timer.
    private bool m_menuVisible;

    private Phases m_currentPhase;
    private Node m_gameGemNode; // The Gem Gameobject, set by GameBoardGeneration.

    #endregion
    #region Accessors
    public static GameManager Instance { get => s_instance; set => s_instance = value; }
    public float GameSpeed { get => m_gameSpeed; set => m_gameSpeed = value; }
    public Node GameGemNode { get => m_gameGemNode; set => m_gameGemNode = value; }
    internal Phases CurrentPhase { get => m_currentPhase; set => m_currentPhase = value; }
    public bool GameOver { get => m_gameOver; set => m_gameOver = value; }
    public int CurrentEnemies { get => m_currentEnemies; set => m_currentEnemies = value; }
    public int RoundCounter { get => m_roundCounter; set => m_roundCounter = value; }
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
        StartBuildingPhase();

    }

    void Update()
    {
        //Update Book Display
        try
        {
            moneyBookText.text = currentGold.ToString();
            timerBookText.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString();
            moneyBookText2.text = currentGold.ToString();
            timerBookText2.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString();
        }
        catch { }


        if (m_gameOver == true)
        {
            m_round.text = "GAME OVER";
            m_gameSpeed = 0;
            if(m_menuVisible == false)
            {
                m_menuVisible = true;
                m_mainMenuAnim.Play("ShowMenu");
            }
            return;
        }
        if(CurrentPhase == Phases.Building)
        {
            m_buildingPhaseTimer -= Time.deltaTime * m_gameSpeed;
            if(m_buildingPhaseTimer % 1.0f < 0.01f) // Plays a ticking sound with the timer.
            {
                AudioManager.Instance.PlaySound("clockTick", AudioLists.UI, AudioMixers.UI, false, true, true, this.gameObject, 0.1f);
            }
        }
        if (m_currentEnemies <= 0 && CurrentPhase == Phases.Building && m_buildingPhaseTimer <= 0.0f)
        {
            StartAttackPhase();
        }
        if(m_currentEnemies <=0 && CurrentPhase == Phases.Attack && m_buildingPhaseTimer <= 0.0f)
        {
            StartBuildingPhase();
        }
        //Updating Debug Displays
        m_goldValue.text = currentGold.ToString();
        m_timerLeft.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString() + "s";

    }

    #region Phase Control
    void StartBuildingPhase()
    {
        CurrentPhase = Phases.Building;
        m_round.text = "Building";
        StartCoroutine(generateMineGold());

        if(m_roundCounter != 1) //Stops this happening on the first round.
        {
            currentGold += (m_mineCount * m_minedGoldPerRound) + 30;
        }



        m_buildingPhaseTimer = m_buildingPhaseTime/2;
    }
    void StartAttackPhase()
    {
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
        AudioManager.Instance.PlaySound("generateMoney", AudioLists.Building, AudioMixers.Effects, true, true, true, this.gameObject, 0.15f);
    }


    IEnumerator generateMineGold()
    {
        currentGold += 30;
        yield return new WaitForSeconds(0.3f);
        AudioManager.Instance.PlaySound("generateMoney",  AudioLists.Building, AudioMixers.Effects, true, true, true, this.gameObject, 0.15f);

        //Play a sound when the mines generate money, up to 5 mines.
        int _count = 0;
        for (int i = 0; i < m_mineCount; i++)
        {
            if(_count < 5)
            {
                yield return new WaitForSeconds(0.3f);
                AudioManager.Instance.PlaySound("generateMoney", AudioLists.Building, AudioMixers.Effects, true, true, true, this.gameObject, 0.15f);
            }
            currentGold += m_minedGoldPerRound + 30;
            _count++;
        }

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
        if (enemyStartingSpawns > _maximumSpawns)
        {
            Debug.LogError("There are too many spawns set in the GameManager. Make the board larger or decrease the amount of spawns from " + enemyStartingSpawns + " to " + _maximumSpawns);
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
        for (int i = 0; i < enemyStartingSpawns; i++)
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
            if(RoundCounter != 1)
            {
                //Increase the count of enemies based on Enemy Counter;
                enemyAmount = Mathf.RoundToInt(3 * Mathf.Sqrt(RoundCounter));
            }

            CurrentEnemies = enemyAmount;
            m_round.text = "Round " + m_roundCounter;

            //Create a list of the enemies to spawn this round.
            List<enemyTypes> _roundEnemies = new List<enemyTypes>(enemyAmount);

            for (int i = 0; i < enemyAmount; i++)
            {
                _roundEnemies.Add(enemyTypes.Regular);
            }


            //Work out the amount of Tank enemies to spawn from the current Round.
            int _tanksToSpawn = 0;
            if ( RoundCounter == tankRoundStart)
            {_tanksToSpawn = m_tankCount; }
            else if (RoundCounter % tankRoundFrequency == 0 && RoundCounter > tankRoundStart)
            {
                m_tankCount++;
                _tanksToSpawn = m_tankCount;
            }

            //Add those tanks to the list of roundEnemies.
            for (int i = 0; i < _tanksToSpawn; i++)
            {
                int _randomIndex = Random.Range(0, _roundEnemies.Count - 1);
                if(_roundEnemies[_randomIndex] == enemyTypes.Regular)
                {
                    _roundEnemies[_randomIndex] = enemyTypes.Tank;
                }
                else
                {
                    i--;
                }
            }



            
            //For each enemy to spawn, randomly choose a spawn and run spawnEnemy
            for (int i = 0; i < _roundEnemies.Count; i++)
            {
                if(_roundEnemies[i] == enemyTypes.Regular)
                {
                    //Spawn a normal enemy;
                    if (!enemySpawns[Random.Range(0, enemySpawns.Count)].spawnEnemy()) //If it fails to spawn an enemy, try again.
                    {
                        i--;
                    }
                }
                else
                {
                    //Spawn a tank
                    if (!enemySpawns[Random.Range(0, enemySpawns.Count)].spawnTank()) //If it fails to spawn an enemy, try again.
                    {
                        i--;
                    }
                }

                yield return new WaitForSeconds(Random.Range(enemySpawnDelay/2,enemySpawnDelay)); //Random delay in spawning enemies, staggers them out.
            }
            m_roundCounter++;
        }
    }




    #endregion

    #region Main Menu Controls

    public void ExitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }


    public void restartGame()
    {
        Debug.Log("Restarting Game");
        m_mainMenuAnim.Play("HideMenu");
        m_menuVisible = false;
        m_gameOver = false;
        m_currentEnemies = 0;
        enemySpawns = new List<EnemySpawnBehaviour>();
        GameBoardGeneration.Instance.generate();
        InputManager.Instance.updateWorldHeight();
        m_roundCounter = 1;
        m_tankCount = 1;
        m_gameSpeed = 1;
        currentGold = m_startingGold;
        m_buildingPhaseTimer = m_buildingPhaseTime;
        CurrentPhase = Phases.Building;
        //Invoke on a delay so GameBoard Graph has been created.
        Invoke("instantiateGem", 0.05f);
        Invoke("instantiateSpawns", 0.05f);
        StartBuildingPhase();
    }
    #endregion


    #region Pathfinding Control

    public List<Node> RunPathfinding(enemyTargets _target, float _aggression,int _currentX,int _currentY)
    {
        //Find the X & Y of a goal node.

        int _goalX = 0;
        int _goalY = 0;

        switch (_target)
        {
            case enemyTargets.Gem:
                _goalX = GameGemNode.x;
                _goalY = GameGemNode.y;
                break;
            case enemyTargets.randomDestructableBuilding:
                //Search all adjacent nodes for a building, if not, search adjecent of those. 
                Node _targetNode = null;
                //Search all nodes and adjacent nodes until it finds a mine.
                for (int i = 0; i < GameBoardGeneration.Instance.Graph.GetLength(0); i++)
                {
                    for (int j = 0; j < GameBoardGeneration.Instance.Graph.GetLength(1); j++)
                    {
                        if (GameBoardGeneration.Instance.Graph[i, j].navigability == navigabilityStates.mine || GameBoardGeneration.Instance.Graph[i, j].navigability == navigabilityStates.wall)
                        {
                            _targetNode = GameBoardGeneration.Instance.Graph[i, j];
                            break;
                        }
                    }
                    if (_targetNode != null)
                    {
                        break;
                    }
                }

                //If there are no buildings, head for the gem.
                if (_targetNode == null)
                {
                    _goalX = GameGemNode.x;
                    _goalY = GameGemNode.y;
                }
                else
                {
                    _goalX = _targetNode.x;
                    _goalY = _targetNode.y;
                }
                break;
            case enemyTargets.randomMine:
                //Search all adjacent nodes for a building, if not, search adjecent of those. 
                _targetNode = null;
                //Search all nodes and adjacent nodes until it finds a mine.
                for (int i = 0; i < GameBoardGeneration.Instance.Graph.GetLength(0); i++)
                {
                    for (int j = 0; j < GameBoardGeneration.Instance.Graph.GetLength(1); j++)
                    {
                        if(GameBoardGeneration.Instance.Graph[i,j].navigability == navigabilityStates.mine)
                        {
                            _targetNode = GameBoardGeneration.Instance.Graph[i, j];
                            break;
                        }
                    }
                    if(_targetNode != null)
                    {
                        break;
                    }
                }

                //If there are no buildings, head for the gem.
                if (_targetNode == null)
                {
                    _goalX = GameGemNode.x;
                    _goalY = GameGemNode.y;
                }
                else
                {
                    _goalX = _targetNode.x;
                    _goalY = _targetNode.y;
                }
                break;
        }

        

        if (_currentX == _goalX && _currentY == _goalY)
        {
            return null;
        }
        List <Node> _groupPath = new List<Node>();
        var graph = GameBoardGeneration.Instance.Graph;
        var search = new Search(GameBoardGeneration.Instance.Graph);
        search.Start(graph[_currentX, _currentY], graph[_goalX, _goalY], _aggression);
        while (!search.finished)
        {
            search.Step();
        }

        Transform[] _pathPositions = new Transform[search.path.Count];
        for (int i = 0; i < search.path.Count; i++)
        {
            _groupPath.Add(search.path[i]);
        }

        if (search.path.Count == 0)
        {
            Debug.Log("Search Failed");
            return null;
        }

        return _groupPath;
    }


    #endregion



}
