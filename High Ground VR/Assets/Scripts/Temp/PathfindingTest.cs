using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTest : MonoBehaviour
{

    public int startPosX;
    public int startPosY;

    [Space(20)]public int endPosX;
    public int endPosY;
    // Start is called before the first frame update
    void Start()
    {
        var graph = GameBoardManagement.Instance.graph;
        var search = new Search(GameBoardManagement.Instance.graph);
        search.Start(graph[startPosX,startPosY], graph[endPosX, endPosY]);

        while (!search.finished)
        {
            search.Step();
        }
        Debug.Log("Search done. Path length " + search.path.Count + "iterations " + search.iterations);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
