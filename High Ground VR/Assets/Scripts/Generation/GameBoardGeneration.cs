using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GameBoardGeneration : MonoBehaviour
{

    private static GameBoardGeneration s_instance;
    public GameObject hexBlock;
    public float hexGapSize = 0.86f;
    [SerializeField] private ValidateBuildingLocation m_buildingValidation;


    [Header("Game Board Size"), Space(10)]
    public int width;
    public int length;
    private List<GameObject> nodes;
    private Node[,] graph;


    private float hexagonalWidth;
    private float hexagonalHeight;
    private float currentX;
    private float currentY;
    private float currentZ;
    private GameObject parentObject;


    public static GameBoardGeneration Instance { get => s_instance; set => s_instance = value; }
    public Node[,] Graph { get => graph; set => graph = value; }
    public ValidateBuildingLocation BuildingValidation { get => m_buildingValidation; set => m_buildingValidation = value; }

    void Start()
    {
        try
        {
            BuildingValidation = this.GetComponent<ValidateBuildingLocation>();
        }
        catch
        {
            Debug.LogWarning("ValidateBuildingLocation not found on GameBoard object.");
        }
        generate();
        InputManager.Instance.updateWorldHeight();
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
        try
        {
            InputManager.Instance.updateWorldHeight();
        }
        catch
        {
            //Game not currently in Play mode. This doesn't need to be caught.
        }
        parentObject = new GameObject("nodesScalingParent");
        parentObject.transform.SetParent(this.transform);

        hexagonalWidth = 2 * hexGapSize;
        hexagonalHeight = Mathf.Sqrt(3) * hexGapSize;
        currentX = 0;
        currentY = 0;
        currentZ = 0;
        destroyAll();
        generateRec();
        populateGraph();
        centralizeGameBoard();
        //setParent();
    }

    #region Game Board Generation
    public void generateRec()
    {
        bool offsetColumn = false;
        float halfWay = width / 2;
        for (int i = 0; i < length; i++)
        {
            currentZ = 0;
            currentX = i * hexagonalWidth;

            currentX = i * hexagonalWidth;
            for (int j = 0; j < width; j++)
            {
                currentX = i * hexagonalWidth;
                currentZ = j * hexagonalHeight;
                if (offsetColumn == true) { currentX += hexagonalWidth / 2; }
                placeHex(i.ToString(), j.ToString());
                offsetColumn = !offsetColumn;
            }
            offsetColumn = false;

        }
    }
    private void placeHex(string x, string z)
    {
        GameObject _point = Instantiate(hexBlock);

        _point.name = x + "," + z;
        _point.transform.tag = "Environment";
        _point.transform.position = new Vector3(currentX, currentY, currentZ);
        setParent(_point);
        nodes.Add(_point);
    }
    private void centralizeGameBoard()
    {
        Vector3 _offset = (nodes[nodes.Count -1 ].transform.position - nodes[0].transform.position)/2;
        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).transform.position -= _offset;
        }
    }
    private void setParent(GameObject _hex)
    {
        if(GameObject.Find("nodesScalingParent") == null)
        {
            parentObject = new GameObject("nodesScalingParent");
            parentObject.transform.SetParent(this.transform);
            _hex.transform.SetParent(parentObject.transform);
        }
        else
        {
            _hex.transform.SetParent(parentObject.transform);
        }
    }

    #endregion

    #region Pathfinding Population
    private void populateGraph()
    {
        //Creating the 2D array of Nodes.
        graph = new Node[length,width];

        int hexCount = 0;

        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var node = new Node();
                node.label = i + "," + j;
                node.hex = nodes[hexCount];
                node.navigability = navigabilityStates.navigable;
                node.x = i;
                node.y = j;


                NodeComponent _nodeComp =  nodes[hexCount].AddComponent<NodeComponent>();
                _nodeComp.node = node;
                try { _nodeComp.buildingPlacementValidation = this.GetComponent<ValidateBuildingLocation>(); }
                catch { Debug.LogError("Does GameBoardGeneration contain a ValidateBuildingLocation Component?"); }
                graph[i, j] = node;
                hexCount++;
            }
        }


        //Assigning the adjecent nodes to each node in the 2D Array
        for (int x = 0; x < graph.GetLength(0); x++)
        {
            for (int y = 0; y < graph.GetLength(1); y++)
            {

                //Adding relevant adjacent nodes to all positions
                //Debug.Log("Checking Node " + x + "," + y );

                //LEFT
                if (x!= 0) {graph[x, y].adjecant.Add(graph[x - 1, y]);}
                //RIGHT
                if(x!= graph.GetLength(0)-1){graph[x, y].adjecant.Add(graph[x + 1, y]);}


                //UPPER AND LOWER LEFT AND RIGHT
                if (y % 2 != 0)
                {
                    //ODD
                    if((y!= graph.GetLength(1)-1) && (x!= graph.GetLength(0) - 1)) { graph[x, y].adjecant.Add(graph[x + 1, y+1]); } //Upper Right
                    if((y != graph.GetLength(1) - 1) && x!= 0) { graph[x, y].adjecant.Add(graph[x , y + 1]); } //Upper Left
                    if(x != graph.GetLength(0) - 1) { graph[x, y].adjecant.Add(graph[x + 1, y - 1]); } //Downwards Right
                    graph[x, y].adjecant.Add(graph[x, y - 1]); //Downwards Left
                }
                else
                {
                   //EVEN
                   if(y != graph.GetLength(1) - 1) { graph[x, y].adjecant.Add(graph[x, y + 1]); } //Upper Right
                   if((y != graph.GetLength(1) - 1) && (x != graph.GetLength(0) - 1) && x != 0) { graph[x, y].adjecant.Add(graph[x - 1, y + 1]); } //Upper Left
                   if(y != 0) { graph[x, y].adjecant.Add(graph[x, y - 1]); } //Downwards Right
                   if(x!= 0 && y!= 0) { graph[x, y].adjecant.Add(graph[x-1, y - 1]); } //Downwards Left
                }
            }
        }

    }
    #endregion

    #region Functional Methods
    public void destroyAll()
    {
        GameObject[] _points = GameObject.FindGameObjectsWithTag("Environment");
        foreach(GameObject gameObj in _points){
            DestroyImmediate(gameObj);
        }

        DestroyImmediate(GameObject.Find("nodesScalingParent"));
        nodes = new List<GameObject>();
    }

    #endregion

    #region Gizmos
    void OnDrawGizmos()
    {
        try
        {
            foreach (GameObject _go in nodes)
            {

                Vector3 _labelPos = new Vector3(_go.transform.position.x, _go.transform.position.y + 1.3f, _go.transform.position.z);
                GUIStyle _labels = new GUIStyle();
                _labels.fontSize = 150000;
                Handles.Label(_labelPos, _go.transform.name);
            }
        }
     
        catch { }
    }
    #endregion
}
