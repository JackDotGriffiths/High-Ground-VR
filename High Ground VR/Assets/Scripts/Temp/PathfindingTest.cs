using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingTest : MonoBehaviour
{

    public int startPosX;
    public int startPosY;

    [Space(20)] public int endPosX;
    public int endPosY;


    private List<Transform> path = new List<Transform>();


    [ContextMenu("Run Test")]
    void RunPathfinding()
    {
        path = new List<Transform>();
        var graph = GameBoardGeneration.Instance.Graph;
        var search = new Search(GameBoardGeneration.Instance.Graph);
        search.Start(graph[startPosX, startPosY], graph[endPosX, endPosY]);

        while (!search.finished)
        {
            search.Step();
        }
        Debug.Log("Search done. Path length : " + search.path.Count + ". Iterations : " + search.iterations);

        Transform[] _pathPositions = new Transform[search.path.Count];
        for (int i = 0; i < search.path.Count; i++)
        {
            path.Add(search.path[i].hex.transform);
        }

    }

    // Update is called once per frame
    void Update()
    {
        DrawPath(path.ToArray());
    }

    void DrawPath(Transform[] _positions)
    {

        try
        {
            Vector3 _startingPoint = new Vector3(_positions[0].position.x, _positions[0].position.y + 1.5f, _positions[0].position.z);
            Vector3 _finishingPoint = new Vector3(_positions[_positions.Length - 1].position.x, _positions[_positions.Length - 1].position.y + 1.5f, _positions[_positions.Length - 1].position.z);

            Debug.DrawLine(_startingPoint, _finishingPoint, Color.green);

            for (int i = 1; i < _positions.Length; i++)
            {
                _startingPoint = new Vector3(_positions[i-1].position.x, _positions[i - 1].position.y + 1.5f, _positions[i - 1].position.z);
                _finishingPoint = new Vector3(_positions[i].position.x, _positions[i].position.y + 1.5f, _positions[i].position.z);

                Debug.DrawLine(_startingPoint, _finishingPoint, Color.blue);
            }


        }
        catch{

        }

    }
}
