﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class GameBoardManagement : MonoBehaviour
{

    private static GameBoardManagement s_instance;
    public GameObject hexBlock;
    public float hexGapSize = 0.86f;


    [Header("Rec Terrain Values"), Space(10)]
    public int width;
    public int length;
    private List<GameObject> nodes;
    public Node[,] graph;


    [Header("Randomisation")]
    public bool randomExpansion;



    private float hexagonalWidth;
    private float hexagonalHeight;
    private int lengthOfRowCount;
    private int resetCount;

    private int count;
    private float currentX;
    private float currentY;
    private float currentZ;
    private float resetPointY;
    private GameObject _randomBelow;
    private GameObject _randomAbove;

    public static GameBoardManagement Instance { get => s_instance; set => s_instance = value; }

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
        hexagonalWidth = 2 * hexGapSize;
        hexagonalHeight = Mathf.Sqrt(3) * hexGapSize;
        currentX = 0;
        currentY = 0;
        currentZ = 0;
        destroyAll();
        generateRec();
        populateGraph();
    }

    #region Game Board Generation
    public void generateRec()
    {
        bool offsetColumn = false;
        float halfWay = width / 2;
        float differenceFromHalfWay;
        float percentageFromEdge;
        for (int i = 0; i < length; i++)
        {
            currentZ = 0;
            currentX = i * hexagonalWidth;
            //Extra nodes at the bottom
            differenceFromHalfWay = Mathf.Abs(i - halfWay);
            percentageFromEdge = differenceFromHalfWay / halfWay;
            percentageFromEdge = 1 - percentageFromEdge;
            if (chanceRoll(percentageFromEdge) && randomExpansion == true) { currentZ -= hexagonalHeight; currentX += hexagonalWidth / 2; placeHex(i.ToString(), "Extra Gen"); }

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
            //Extra nodes at the top
            if (chanceRoll(percentageFromEdge) && randomExpansion == true) { currentZ += hexagonalHeight; currentX += hexagonalWidth / 2; placeHex(i.ToString(), "Extra Gen"); }
        }
    }

    private void placeHex(string x, string z)
    {
        GameObject _point = Instantiate(hexBlock);


        _point.name = x + "," + z;
        _point.transform.SetParent(this.transform);
        _point.tag = "location";
        _point.transform.position = new Vector3(currentX, currentY, currentZ);
        nodes.Add(_point);
    }

    #endregion

    #region Pathfinding Population
    private void populateGraph()
    {
        //Creating the 2D array of Nodes.
        graph = new Node[length,width];
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var node = new Node();
                node.label = i + "," + j;
                graph[i, j] = node;
            }
        }


        //Assigning the adjecent nodes to each node in the 2D Array
        for (int x = 0; x < graph.GetLength(0); x++)
        {
            for (int y = 0; y < graph.GetLength(1); y++)
            {
                Debug.Log("Checking Node " + x + "," + y );
                //LEFT
                if (x!= 0)
                {
                    graph[x, y].adjecant.Add(graph[x - 1, y]);
                }
                //RIGHT
                if(x!= graph.GetLength(0)-1)
                {
                    graph[x, y].adjecant.Add(graph[x + 1, y]);
                }


                //UPPER AND LOWER LEFT AND RIGHT
                if (y % 2 != 0)
                {
                    //EVEN
                    Debug.Log("Even");
                    if (y != graph.GetLength(1) - 1)
                    {
                        graph[x, y].adjecant.Add(graph[x, y + 1]);
                        graph[x, y].adjecant.Add(graph[x + 1, y + 1]);
                    }

                    //LOWER LEFT AND LOWER RIGHT
                    if (y != 0)
                    {
                        graph[x, y].adjecant.Add(graph[x, y - 1]);
                        graph[x, y].adjecant.Add(graph[x + 1, y - 1]);
                    }



                }
                else
                {
                    Debug.Log("Odd");
                    if (y != graph.GetLength(1) - 1)
                    {
                        graph[x, y].adjecant.Add(graph[x, y + 1]);
                    }
                    if (x!= 0)
                    {
                        graph[x, y].adjecant.Add(graph[x - 1, y + 1]);
                    }
                    if (y != 0)
                    {
                        graph[x, y].adjecant.Add(graph[x- 1, y-1]);
                        graph[x, y].adjecant.Add(graph[x, y-1]);
                    }
                }
            }
        }

    }
    #endregion


    #region Functional Methods
    public void destroyAll()
    {
        GameObject[] _points = GameObject.FindGameObjectsWithTag("location");
        foreach(GameObject gameObj in _points){
            DestroyImmediate(gameObj);
        }

        nodes = new List<GameObject>();
}

    private bool chanceRoll(float _percentage)
    {
        bool _result = false;
        if (_percentage > 1 || _percentage < 0)
        {
            Debug.LogWarning("chanceRoll failed. Ratio was invalid.");
            
        }
        float _random = Random.Range(0f, 1f);
        if (_random < _percentage)
        {
            _result = true;
        }
        return _result;
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
                _labels.fontSize = 200000;
                Handles.Label(_labelPos, _go.transform.name);
            }
        }
     
        catch { }
    }
    #endregion
}
