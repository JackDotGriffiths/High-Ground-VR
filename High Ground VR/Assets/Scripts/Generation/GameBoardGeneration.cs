using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GameBoardGeneration : MonoBehaviour
{

    private static GameBoardGeneration s_instance;
    [Header("Game Board Configuration")]
    [SerializeField,Tooltip("The Hex GameObject to Use")] private GameObject m_hexBlock;
    [SerializeField, Tooltip("The Gem GameObject to Use")] private GameObject m_gem;
    [SerializeField, Tooltip("Distance between Hexes. Tested to be around 0.86"),Space(5)] private float m_hexGapSize = 0.86f;
    [SerializeField, Space(10),Tooltip("Amount of hexes wide the terrain will be. Usual game board is 17x17"),Range(1,100)] private int m_width = 1;
    [SerializeField, Tooltip("Amount of hexes long the terrain will be. Usual game board is 17x17"), Range(1, 100)] private int m_length = 1;

    [SerializeField, Space(10), Tooltip("Generate ambient assets like rocks and grass, placing them on the board")] private bool m_ambientGeneration = true;
    [SerializeField, Tooltip("Distance between Hexes. Tested to be around 0.86"), Range(0,100)] private int m_ambientNatureFrequency = 30;
    [SerializeField, Tooltip("Limit of the ambient nature assets allowed per hex"), Range (0,5)] private int m_ambientNatureLimit = 2;
    [SerializeField,Tooltip("List of ambient nature assets")]private List<GameObject> m_ambientNatureAssets;


    private float m_hexagonalWidth; //Calculated width of a hexagon from the hexGapSize
    private float m_hexagonalHeight;//Calculated height of a hexagon from the hexGapSize
    private float m_currentX; //Tracks the current X position while generating and placing Hexagons.
    private float m_currentY; //Tracks the current Y position while generating and placing Hexagons.
    private float m_currentZ; //Tracks the current Z position while generating and placing Hexagons.
    private List<GameObject> nodes; //List of all nodes created while generating
    private GameObject m_parentObject; //Empty object that gets filled with nodes so that the nodes scale properly while remaining at (1,1,1) in scale.


    #region Accessors
    public static GameBoardGeneration Instance { get => s_instance; set => s_instance = value; }
    public Node[,] Graph { get; set; }
    public ValidateBuildingLocation BuildingValidation { get; set; }
 
    #endregion


    void Start()
    {
        generate();
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

    public void generate()
    {
        try{InputManager.Instance.updateWorldHeight();}
        catch
        {
            //Game not currently in Play mode. This doesn't need to be caught as this script sometimes runs not in play mode and doens't need this method to run.
        }

        //Try/Catch used to check any missed associations.
        try { BuildingValidation = this.GetComponent<ValidateBuildingLocation>(); }
        catch { Debug.LogWarning("Assigning BuildingValidation failed on GameBoard object."); }


        //Create the parent of all subsequently spawned nodes that will aid with scaling
        m_parentObject = new GameObject("nodesScalingParent");
        m_parentObject.transform.SetParent(this.transform);

        m_hexagonalWidth = 2 * m_hexGapSize; //Width of the hexagon
        m_hexagonalHeight = Mathf.Sqrt(3) * m_hexGapSize; //Height of the hexagon
        m_currentX = 0;
        m_currentY = 0;
        m_currentZ = 0;
        destroyAll(); //Destroy all current nodes 
        generateRec(); //Generate a rectangle out of hexagons
        populateGraph(); //Populate the Node Graph
        centralizeGameBoard(); //Centralise the Game Board on the object's point by moving all of the children.
        if(m_ambientGeneration == true)
        {
            placeAmbientNature();
        }
    }

    #region Game Board Generation


    /// <summary>
    /// Calculates the position of each hexagon required from the m_hexGapSize and the current index in the graph.
    /// </summary>
    public void generateRec()
    {
        bool _offsetColumn = false;

        for (int i = 0; i < m_length; i++)
        {
            m_currentZ = 0;
            m_currentX = i * m_hexagonalWidth;

            m_currentX = i * m_hexagonalWidth;
            for (int j = 0; j < m_width; j++)
            {
                m_currentX = i * m_hexagonalWidth;
                m_currentZ = j * m_hexagonalHeight;
                if (_offsetColumn == true) { m_currentX += m_hexagonalWidth / 2; }
                placeHex(i.ToString(), j.ToString());
                _offsetColumn = !_offsetColumn;
            }
            _offsetColumn = false;
        }
    }



    /// <summary>
    /// Place a hexagon at the current position of the terrain generation.
    /// </summary>
    /// <param name="x"> X index to use for the name.</param>
    /// <param name="z"> Z index to use for the name.</param>
    private void placeHex(string x, string z)
    {
        GameObject _point = Instantiate(m_hexBlock);

        _point.name = x + "," + z;
        _point.transform.tag = "Environment";
        _point.transform.position = new Vector3(m_currentX, m_currentY, m_currentZ);
        setParent(_point);
        nodes.Add(_point);
    }



    /// <summary>
    /// Centralises the game board on the spawn point.
    /// </summary>
    private void centralizeGameBoard()
    {
        //Taking the vector between the bottom left and top right hex, halving it, and applying that to all of the nodes so that the game board is in the middle.
        Vector3 _offset = (nodes[nodes.Count -1 ].transform.position - nodes[0].transform.position)/2;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).transform.position -= _offset;
        }
    }



    /// <summary>
    /// Sets the parent of the passed in _hex to the m_parentObject.
    /// </summary>
    /// <param name="_hex">The hexagon to set the parent of.</param>
    private void setParent(GameObject _hex)
    {
        //Check if the object exists, if not create it. Otherwise, just set the parent. This was done so that all of the Hexagons stay at 1,1,1 in scale, which is easier to work with when instantiating objects on.
        if(GameObject.Find("nodesScalingParent") == null)
        {
            m_parentObject = new GameObject("nodesScalingParent");
            m_parentObject.transform.SetParent(this.transform);
            _hex.transform.SetParent(m_parentObject.transform);
        }
        else
        {
            _hex.transform.SetParent(m_parentObject.transform);
        }
    }

    /// <summary>
    /// Places the Gem in the center of the board. If there is not a center it will pick a random node that's towards the center.
    /// </summary>
    public void placeGem()
    {
        Node _gemNode = Graph[ Mathf.RoundToInt(m_length / 2f), Mathf.RoundToInt(m_width / 2f)];
        _gemNode.navigability = navigabilityStates.gem;
        Vector3 _gemPosition = new Vector3(_gemNode.hex.transform.position.x, _gemNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, _gemNode.hex.transform.position.z);
        Quaternion _gemRotation = Quaternion.Euler(45, 45, 45);
        Instantiate(m_gem, _gemPosition, _gemRotation, _gemNode.hex.transform);
        GameManager.Instance.GameGemNode = _gemNode;
    }

    /// <summary>
    /// Places random nature assets on the map.
    /// </summary>
    public void placeAmbientNature()
    {
        if(m_ambientNatureAssets.Count != 0)
        {
            GameObject _ambientParent = new GameObject("ambientNature");
            _ambientParent.transform.SetParent(this.transform);
            foreach (GameObject _hex in nodes)
            {
                for (int i = 0; i < m_ambientNatureLimit; i++)//Loop of how many assets are on each hex.
                {
                    int _rand = Random.Range(0, 100); //Random chance of placing an asset
                    if(_rand < m_ambientNatureFrequency)
                    {
                        Vector3 _spawnPosition = (Random.insideUnitSphere * 0.7f) + _hex.transform.position;
                        _spawnPosition.y = BuildingValidation.buildingHeightOffset;
                        Vector3 _spawnRotation = new Vector3(0, Random.Range(0.0f, 360.0f), 0);


                        GameObject _randObject = m_ambientNatureAssets[Random.Range(0, m_ambientNatureAssets.Count)];//Choose a random object
                        Instantiate(_randObject, _spawnPosition, Quaternion.Euler(_spawnRotation), _ambientParent.transform);//Create the gameObject
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("No Ambient Assets Available in GameBoardGeneration");
        }
    }

    #endregion

    #region Pathfinding Population

    /// <summary>
    /// Takes all of the spawned hexagons and generates the graph of Nodes to use for the game.
    /// </summary>
    private void populateGraph()
    {
        //Creating the 2D array of Nodes.
        Graph = new Node[m_length,m_width];

        int hexCount = 0;

        //For each item in the node list, go through and create a Node class for that.
        for (int i = 0; i < m_length; i++)
        {
            for (int j = 0; j < m_width; j++)
            {
                //Create a node, and attach a NodeComponent object to the GameObject and store the node within that.
                var _node = new Node(i + "," + j,i,j, nodes[hexCount], navigabilityStates.navigable);
                NodeComponent _nodeComp =  nodes[hexCount].AddComponent<NodeComponent>();
                _nodeComp.node = _node;
                try { _nodeComp.buildingPlacementValidation = this.GetComponent<ValidateBuildingLocation>(); }
                catch { Debug.LogError("Does GameBoardGeneration contain a ValidateBuildingLocation Component?"); }
                Graph[i, j] = _node;
                hexCount++;
            }
        }


        //Assigning the adjecent nodes to each node in the 2D Array
        for (int x = 0; x < Graph.GetLength(0); x++)
        {
            for (int y = 0; y < Graph.GetLength(1); y++)
            {

                //Adding relevant adjacent nodes to all positions, based on the position of the hex on an odd or even row.
                //Debug.Log("Checking Node " + x + "," + y );

                //LEFT
                if (x!= 0) {Graph[x, y].adjecant.Add(Graph[x - 1, y]);}
                //RIGHT
                if(x!= Graph.GetLength(0)-1){Graph[x, y].adjecant.Add(Graph[x + 1, y]);}


                //UPPER AND LOWER LEFT AND RIGHT
                if (y % 2 != 0)
                {
                    //ODD
                    if((y!= Graph.GetLength(1)-1) && (x!= Graph.GetLength(0) - 1)) { Graph[x, y].adjecant.Add(Graph[x + 1, y+1]); } //Upper Right
                    if((y != Graph.GetLength(1) - 1) && x!= 0) { Graph[x, y].adjecant.Add(Graph[x , y + 1]); } //Upper Left
                    if(x != Graph.GetLength(0) - 1) { Graph[x, y].adjecant.Add(Graph[x + 1, y - 1]); } //Downwards Right
                    Graph[x, y].adjecant.Add(Graph[x, y - 1]); //Downwards Left
                }
                else
                {
                   //EVEN
                   if(y != Graph.GetLength(1) - 1) { Graph[x, y].adjecant.Add(Graph[x, y + 1]); } //Upper Right
                   if((y != Graph.GetLength(1) - 1) && (x != Graph.GetLength(0) - 1) && x != 0) { Graph[x, y].adjecant.Add(Graph[x - 1, y + 1]); } //Upper Left
                   if(y != 0) { Graph[x, y].adjecant.Add(Graph[x, y - 1]); } //Downwards Right
                   if(x!= 0 && y!= 0) { Graph[x, y].adjecant.Add(Graph[x-1, y - 1]); } //Downwards Left
                }
            }
        }

    }
    #endregion

    #region Functional Methods

    /// <summary>
    /// Destroys all children of the game board. 
    /// </summary>
    public void destroyAll()
    {
        GameObject[] _points = GameObject.FindGameObjectsWithTag("Environment");
        foreach(GameObject _gameObj in _points){
            DestroyImmediate(_gameObj);
        }

        DestroyImmediate(GameObject.Find("nodesScalingParent"));
        DestroyImmediate(GameObject.Find("ambientNature"));
        nodes = new List<GameObject>();
    }


    /// <summary>
    /// Clears the ambient Nature off the game board.
    /// </summary>
    public void destroyAmbientNature()
    {
        DestroyImmediate(GameObject.Find("ambientNature"));
    }

    #endregion

    #region Gizmos
    //void OnDrawGizmos()
    //{
    //    try
    //    {
    //        foreach (GameObject _go in nodes)
    //        {

    //            Vector3 _labelPos = new Vector3(_go.transform.position.x, _go.transform.position.y + 1.3f, _go.transform.position.z);
    //            GUIStyle _labels = new GUIStyle();
    //            _labels.fontSize = 150000;
    //            Handles.Label(_labelPos, _go.transform.name);
    //        }
    //    }

    //    catch { }
    //}
    #endregion
}
