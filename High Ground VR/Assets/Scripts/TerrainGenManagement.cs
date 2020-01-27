using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrainGenManagement : MonoBehaviour
{
    public GameObject hexBlock;
    public float hexGapSize; //Will eventually be changed to WIDTH of the HEX model

    [Header ("Materials")]
    public Material water;
    public Material bridge;

    //[Header ("Hex Terrain Values")]
    //public int radius;

    [Header("Rec Terrain Values"), Space(10)]
    public int width;
    public int length;
    public int bridgeCount;
    private List<GameObject> nodes;

    private List<GameObject> terrainMarkersBelow;
    private List<GameObject> terrainMarkersAbove;

    [Header("Random Stuff")]
    public bool randomExpansion;

    private float hexagonalWidth;
    private float hexagonalHeight;
    private int lengthOfRowCount;
    private int resetCount;

    private int count;
    private float currentX;
    private float currentY;
    private float currentZ;
    private float resetPointX;
    private float resetPointY;
    private float xOffset;
    private bool genRivers;
    private Ray ray;
    private GameObject _randomBelow;
    private GameObject _randomAbove;


    private void Start()
    {
        generate();
    }

    public void generate()
    {
        hexagonalWidth = 2 * hexGapSize;
        hexagonalHeight = Mathf.Sqrt(3) * hexGapSize;
        resetPointX = 0;
        xOffset = 0;
        currentX = 0;
        currentY = 0;
        currentZ = 0;
        destroyAll();
        generateRec();
    }

    public void generateRec()
    {
        bool offsetColumn = false;
        float halfWay = width / 2;
        float differenceFromHalfWay;
        float percentageFromEdge;
        for(int i = 0; i< width; i++)
        {
            currentZ = 0;
            currentX = i * hexagonalWidth;
            //Extra nodes at the bottom
            differenceFromHalfWay = Mathf.Abs(i - halfWay);
            percentageFromEdge = differenceFromHalfWay / halfWay;
            percentageFromEdge = 1 - percentageFromEdge;
            if (chanceRoll(percentageFromEdge) && randomExpansion == true) { currentZ -= hexagonalHeight; currentX += hexagonalWidth / 2; placeNode(i.ToString(), "Extra Gen"); }

            currentX = i * hexagonalWidth;
            for (int j=0; j < length; j++)
            {
                currentX = i * hexagonalWidth;
                currentZ = j * hexagonalHeight;
                if (offsetColumn == true){currentX += hexagonalWidth / 2;}
                placeNode(i.ToString(), j.ToString());
                offsetColumn = !offsetColumn;
            }
            offsetColumn = false;
            //Extra nodes at the top
            if (chanceRoll(percentageFromEdge) && randomExpansion == true) { currentZ += hexagonalHeight; currentX += hexagonalWidth / 2; placeNode(i.ToString(), "Extra Gen");}
        }
    }

    private void placeNode(string x, string z)
    {
        GameObject _point = Instantiate(hexBlock);


        _point.name = "-Point number : " + x + "," + z;
        _point.transform.SetParent(this.transform);
        _point.tag = "location";
        _point.transform.position = new Vector3(currentX, currentY, currentZ);
        nodes.Add(_point);
    }

    #region Functionality
    public void destroyAll()
    {
        GameObject[] _points = GameObject.FindGameObjectsWithTag("location");
        foreach(GameObject gameObj in _points){
            DestroyImmediate(gameObj);
        }
        try {
            terrainMarkersBelow = new List<GameObject>();
            terrainMarkersAbove = new List<GameObject>();
        } catch { }

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
}
