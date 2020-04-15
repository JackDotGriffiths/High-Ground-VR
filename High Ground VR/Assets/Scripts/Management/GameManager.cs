using System.Collections;
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

    [Header ("UI Management")]
    [SerializeField, Tooltip("Main Menu GameObject")] private GameObject m_mainMenu;
    [SerializeField, Tooltip("Game Menu GameObject")] private GameObject m_gameMenu;
    [SerializeField, Tooltip("Score UI Element")] private TextMeshProUGUI m_scoreUI;
    [SerializeField, Tooltip("Gold UI Element")] private TextMeshProUGUI m_goldUI;
    [SerializeField, Tooltip("Time UI Element")] private TextMeshProUGUI m_timeUI;
    [SerializeField, Tooltip("Round UI Element")] private TextMeshProUGUI m_roundUI;

    private bool m_gameOver = false;
    public bool GameStarted = false;

    [Header("Enemy Spawn Management"), Space(10)]
    [Tooltip("Amount of spawns the game rounds should start with")] public int enemyStartingSpawns = 1;
    [Tooltip("Amount of enemies to spawn at the start of Round One.")] public int enemyAmount = 1;
    [Tooltip("Enemy Spawn Delay")] public int enemySpawnDelay = 10;
    [Tooltip("Which round number to start spawning tank enemies after"), Space(10)] public int tankRoundStart = 5;
    [Tooltip("How many rounds have to pass before another one spawns")] public int tankRoundFrequency = 5;

    [HideInInspector] public List<EnemySpawnBehaviour> enemySpawns;
    private List<Node> m_outerEdgeNodes; //Set to the nodes on the outer edge of the map, used when spawning enemies.
    private int m_currentEnemies;
    private int m_tankCount = 1;


    [Header("Gold Management"), Space(10)]
    [SerializeField, Tooltip("Gold the player should start with.")] private int m_startingGold = 10; //Starting gold for the player
    [SerializeField, Tooltip("Amount of gold for a player to earn per interval.")] private int m_minedGoldPerRound = 20; //Amount of gold per tick
    [SerializeField, Tooltip("Amount of gold for a player to earn from killing an enemy")] private int m_goldPerKill = 20; //Amount of enemyKilled

    public int wallsCost = 10; //Cost of placing a wall
    public int barracksCost = 100; // Cost of placing a barracks
    public int mineCost = 50; //Cost of placing a mine
    private int m_mineCount;


    [HideInInspector] public TextMeshProUGUI moneyBookText, timerBookText, roundBookText, moneyBookText2, timerBookText2, roundBookText2;



    public int currentGold; //The current gold of the player.
    public int currentScore; //The current score of the player.
    private int visibleScore;
    private int m_roundCounter = 1;
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
        m_mainMenu.SetActive(true);
        m_gameMenu.SetActive(false);
        //Singleton Implementation
        if (s_instance == null)
            s_instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }


    void Update()
    {
        //Update Book Display
        try
        {
            moneyBookText.text = currentGold.ToString();
            timerBookText.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString();
            m_goldUI.text = currentGold.ToString();
            m_timeUI.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString();
            moneyBookText2.text = currentGold.ToString();
            timerBookText2.text = Mathf.RoundToInt(m_buildingPhaseTimer).ToString();

            if (CurrentPhase == Phases.Building)
            {
                roundBookText.text = "Building";
                roundBookText2.text = "Building";
                m_roundUI.text = "Building";
            }
        }
        catch { }


        if (m_gameOver == true)
        {
            m_gameSpeed = 0;
            roundBookText.text = "Game Over";
            roundBookText2.text = "Game Over";
            m_roundUI.text = "Game Over";
            return;
        }
        else if (m_gameOver == false && GameStarted == true)
        {
            if (CurrentPhase == Phases.Building)
            {
                m_buildingPhaseTimer -= Time.deltaTime * m_gameSpeed;
                if (m_buildingPhaseTimer % 1.0f < 0.01f) // Plays a ticking sound with the timer.
                {
                    AudioManager.Instance.PlaySound("clockTick", AudioLists.UI, AudioMixers.UI, false, true, true, this.gameObject, 0.1f);
                }
            }
            if (m_currentEnemies <= 0 && CurrentPhase == Phases.Building && m_buildingPhaseTimer <= 0.0f)
            {
                StartAttackPhase();
            }
            if (m_currentEnemies <= 0 && CurrentPhase == Phases.Attack && m_buildingPhaseTimer <= 0.0f)
            {
                StartBuildingPhase();
            }
        }
    }

    #region Phase Control
    void StartBuildingPhase()
    {
        CurrentPhase = Phases.Building;
        StartCoroutine(generateMineGold());

        if (m_roundCounter != 1) //Stops this happening on the first round.
        {
            currentGold += 30;
            AudioManager.Instance.PlaySound("buildingPhaseStarted", AudioLists.Combat, AudioMixers.Music, false, true, true, this.gameObject, 0.1f);
            addScore(300);
        }



        m_buildingPhaseTimer = m_buildingPhaseTime / 2;
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
                _node.navigability = nodeTypes.navigable;
            }
        }
        AudioManager.Instance.PlaySound("buildingPhaseStarted", AudioLists.Combat, AudioMixers.Music, false, true, true, this.gameObject, 0.1f);
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
        if (_cost <= currentGold)
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
        yield return new WaitForSeconds(0.3f);
        AudioManager.Instance.PlaySound("generateMoney", AudioLists.Building, AudioMixers.Effects, true, true, true, this.gameObject, 0.15f);

        //Play a sound when the mines generate money, up to 5 mines.
        int _count = 0;
        for (int i = 0; i < m_mineCount; i++)
        {
            if (_count < 5)
            {
                yield return new WaitForSeconds(0.3f);
                AudioManager.Instance.PlaySound("generateMoney", AudioLists.Building, AudioMixers.Effects, true, true, true, this.gameObject, 0.15f);
            }
            currentGold += m_minedGoldPerRound;
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
        for (int i = 0; i < GameBoardGeneration.Instance.Graph.GetLength(0) - 1; i++)
        {
            m_outerEdgeNodes.Add(GameBoardGeneration.Instance.Graph[i, 0]);
            m_outerEdgeNodes.Add(GameBoardGeneration.Instance.Graph[i, GameBoardGeneration.Instance.Graph.GetLength(1) - 1]);
        }

        //Left and Right of the Game Board
        for (int i = 0; i < GameBoardGeneration.Instance.Graph.GetLength(1) - 1; i++)
        {
            m_outerEdgeNodes.Add(GameBoardGeneration.Instance.Graph[0, i]);
            m_outerEdgeNodes.Add(GameBoardGeneration.Instance.Graph[GameBoardGeneration.Instance.Graph.GetLength(0) - 1, i]);
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
        if (m_gameOver == false && GameStarted == true)
        {
            roundBookText.text = "Round " + m_roundCounter;
            roundBookText2.text = "Round " + m_roundCounter;
            m_roundUI.text = "Round " + m_roundCounter;
            if (RoundCounter != 1)
            {
                //Increase the count of enemies based on Enemy Counter;
                enemyAmount = Mathf.RoundToInt(3 * Mathf.Sqrt(RoundCounter));
            }

            CurrentEnemies = enemyAmount;

            //Create a list of the enemies to spawn this round.
            List<enemyTypes> _roundEnemies = new List<enemyTypes>(enemyAmount);

            for (int i = 0; i < enemyAmount; i++)
            {
                _roundEnemies.Add(enemyTypes.Regular);
            }


            //Work out the amount of Tank enemies to spawn from the current Round.
            int _tanksToSpawn = 0;
            if (RoundCounter == tankRoundStart)
            { _tanksToSpawn = m_tankCount; }
            else if (RoundCounter % tankRoundFrequency == 0 && RoundCounter > tankRoundStart)
            {
                m_tankCount++;
                _tanksToSpawn = m_tankCount;
            }

            //Add those tanks to the list of roundEnemies.
            for (int i = 0; i < _tanksToSpawn; i++)
            {
                int _randomIndex = Random.Range(0, _roundEnemies.Count - 1);
                if (_roundEnemies[_randomIndex] == enemyTypes.Regular)
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
                if (_roundEnemies[i] == enemyTypes.Regular)
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

                yield return new WaitForSeconds(Random.Range(enemySpawnDelay / 2, enemySpawnDelay)); //Random delay in spawning enemies, staggers them out.
            }
            m_roundCounter++;
        }
    }




    #endregion

    #region Play/Exit/Restart

    public void playGame()
    {
        AudioManager.Instance.PlaySound("gameStarted/Over", AudioLists.Combat, AudioMixers.Music, false, true, true, this.gameObject, 0.1f);
        InputManager.Instance.SpawnBook();
        Debug.Log("Play Game");
        resetScore();
        m_mainMenu.SetActive(false);
        m_gameMenu.SetActive(true);
        m_gameOver = false;
        GameStarted = true;
        m_currentEnemies = 0;
        enemySpawns = new List<EnemySpawnBehaviour>();
        GameBoardGeneration.Instance.generate();
        InputManager.Instance.updateWorldHeight();
        m_roundCounter = 1;
        m_tankCount = 1;
        m_mineCount = 0;
        m_gameSpeed = 1;
        currentGold = m_startingGold;
        m_buildingPhaseTimer = m_buildingPhaseTime;
        CurrentPhase = Phases.Building;
        //Invoke on a delay so GameBoard Graph has been created.
        Invoke("instantiateGem", 0.05f);
        Invoke("instantiateSpawns", 0.05f);
        StartBuildingPhase();
    }
    public void exitGame()
    {
        Debug.Log("Exit Game");
        Application.Quit();
    }
    public void restartGame()
    {
        AudioManager.Instance.PlaySound("gameStarted/Over", AudioLists.Combat, AudioMixers.Music, false, true, true, this.gameObject, 0.1f);
        InputManager.Instance.SpawnBook();
        Debug.Log("Restart Game");
        resetScore();
        m_gameOver = false;
        GameStarted = true;
        m_currentEnemies = 0;
        enemySpawns = new List<EnemySpawnBehaviour>();
        GameBoardGeneration.Instance.generate();
        InputManager.Instance.updateWorldHeight();
        m_roundCounter = 1;
        m_tankCount = 1;
        m_gameSpeed = 1;
        m_mineCount = 0;
        currentGold = m_startingGold;
        m_buildingPhaseTimer = m_buildingPhaseTime;
        CurrentPhase = Phases.Building;
        //Invoke on a delay so GameBoard Graph has been created.
        Invoke("instantiateGem", 0.05f);
        Invoke("instantiateSpawns", 0.05f);
        StartBuildingPhase();
    }

    public void GoToMainMenu()
    {
        AudioManager.Instance.PlaySound("gameStarted/Over", AudioLists.Combat, AudioMixers.Music, false, true, true, this.gameObject, 0.1f);
        InputManager.Instance.RemoveBook();
        Debug.Log("Go To Main Menu");
        resetScore();
        m_mainMenu.SetActive(true);
        m_gameMenu.SetActive(false);
        m_gameOver = true;
        GameStarted = false;
        GameBoardGeneration.Instance.destroyAll();
    }


    #endregion

    #region Pathfinding Control

    /// <summary>
    /// General Pathfinding, used for getting around the board to a passed in location.
    /// </summary>
    /// <param name="_currentNode"> Start of the pathfinding</param>
    /// <param name="_goalNode"> End of the pathfinding</param>
    /// <param name="_unitAggression">Current Aggression value.</param>
    /// <returns></returns>
    public List<Node> RunPathfinding(Node _currentNode, Node _goalNode, float _unitAggression)
    {
        SearchTypes _searchType;
        float _currentAggressionBoundary = Mathf.Clamp(1.0f - (RoundCounter / 8),0.6f,1.0f); //It becomes more likely a unit will get aggressive over time. By round 10, 40% of enemies will be aggressive instantly.
        //Debug.Log("Current Aggression Boundary is " + _currentAggressionBoundary);
        if (_unitAggression > _currentAggressionBoundary)
        {
            _searchType = SearchTypes.Aggressive;
        }
        else
        {
            _searchType = SearchTypes.Passive;
        }

        var search = new Search();
        search.StartSearch(_currentNode, _goalNode, _searchType);
        return search.path;
    }

    /// <summary>
    /// Alternative pathfinding which can override the searchType.
    /// </summary>
    /// <param name="_currentNode"></param>
    /// <param name="_goalNode"></param>
    /// <param name="_searchType"></param>
    /// <returns></returns>
    public List<Node> RunPathfinding(Node _currentNode, Node _goalNode, SearchTypes _searchType)
    {
        var search = new Search();
        search.StartSearch(_currentNode, _goalNode, _searchType);
        return search.path;
    }


    /// <summary>
    /// Runs fully aggressive pathfinding for the tank. Navigates through destructable buildings.
    /// </summary>
    /// <param name="_currentNode"></param>
    /// <returns></returns>
    public List<Node> RunTankPathfinding(Node _currentNode)
    {
        Node _goalNode = null;
        SearchTypes _searchType = SearchTypes.Aggressive;

        //Random chance the tank will go towards a random building.
        float _rand = Random.Range(0.0f, 1.0f);
        //40% chance of picking a random building
        if (_rand < 0.4f)
        {

            int _count = 0;
            while(_goalNode == null & _count < 15) // 15 Attempts at finding a random building
            {
                //Randomly pick a node.
                Node _chosenNode = GameBoardGeneration.Instance.Graph[Random.Range(0, GameBoardGeneration.Instance.Graph.GetLength(0)), Random.Range(0, GameBoardGeneration.Instance.Graph.GetLength(1))];
                if(_chosenNode.navigability == nodeTypes.wall || _chosenNode.navigability == nodeTypes.mine)
                {
                    _goalNode = _chosenNode;
                }
            }
            if(_goalNode == null) //If it failed to find anything, set the goal to the gem.
            {
                _goalNode = GameGemNode;
            }
        }
        else
        {
            _goalNode = GameGemNode; // Gem as the tank's goal
        }

        var search = new Search();
        search.StartSearch(_currentNode, _goalNode, _searchType);
        return search.path;
    }


    #endregion

    #region Score Control

    /// <summary>
    /// Adds to the player's current score
    /// </summary>
    /// <param name="_amount">Amount to Add</param>
    public void addScore(int _amount)
    {
        currentScore += _amount;
        StartCoroutine(updateScoreScreen());
    }

    /// <summary>
    /// Resets the players score to 0.
    /// </summary>
    private void resetScore()
    {
        currentScore = 0;
        m_scoreUI.text = currentScore.ToString("000000");
    }


    IEnumerator updateScoreScreen()
    {
        do
        {
            visibleScore++;
            yield return new WaitForSeconds(0.03f);
            m_scoreUI.text = visibleScore.ToString("000000");
        } while (visibleScore != currentScore);
        yield return null;
    }
    #endregion

    #region Usability Testing Control - Removed for release

    //private void setPlayerPrefs()
    //{
    //    PlayerPrefs.SetInt("Score", 0);
    //    PlayerPrefs.SetInt("Round Reached", 0);
    //    PlayerPrefs.SetInt("Barracks Placed", 0);
    //    PlayerPrefs.SetInt("Walls Placed", 0);
    //    PlayerPrefs.SetInt("Mine Placed", 0);
    //    PlayerPrefs.SetInt("Regular Spell", 0);
    //    PlayerPrefs.SetInt("Speed Spell", 0);
    //    PlayerPrefs.SetInt("Slow Spell", 0);
    //}
    //private void submitData()
    //{
    //    if(currentScore == 0)
    //    {
    //        return;
    //    }
    //    PlayerPrefs.SetInt("Score", currentScore);
    //    PlayerPrefs.SetInt("Round Reached", m_roundCounter);
    //    StartCoroutine(Post(PlayerPrefs.GetInt("Score").ToString(), PlayerPrefs.GetInt("Round Reached").ToString(), PlayerPrefs.GetInt("Barracks Placed").ToString(), PlayerPrefs.GetInt("Walls Placed").ToString(),PlayerPrefs.GetInt("Mine Placed").ToString(), PlayerPrefs.GetInt("Regular Spell").ToString(), PlayerPrefs.GetInt("Speed Spell").ToString(), PlayerPrefs.GetInt("Slow Spell").ToString()));

    //}

    //IEnumerator Post(string score, string maxRound, string barracks, string walls, string mine, string regularSpell, string speedSpell, string slowSpell)
    //{
    //    WWWForm form = new WWWForm();
    //    form.AddField("entry.1404377732", score);
    //    form.AddField("entry.224105009", maxRound);
    //    form.AddField("entry.1340497501", barracks);
    //    form.AddField("entry.938290357", walls);
    //    form.AddField("entry.713143027", mine);
    //    form.AddField("entry.705423035", regularSpell);
    //    form.AddField("entry.1687568442", speedSpell);
    //    form.AddField("entry.401115139", slowSpell);
    //    byte[] rawData = form.data;
    //    WWW www = new WWW("https://docs.google.com/forms/u/0/d/e/1FAIpQLSeC2ytHm1RVTDnlJAh8AeXjf0CJ2rnSVdRvqsENml7nAbsXbg/formResponse", rawData);
    //    yield return www;
    //}


    #endregion


}
