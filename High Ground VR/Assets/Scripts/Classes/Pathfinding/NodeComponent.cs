using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeComponent : MonoBehaviour
{
    public Node node;

    [ContextMenu("Expose Node Data")]
    void PrintNodeInfo()
    {
        Debug.Log(node.adjecant);
    }

    public void PlaceBarracks()
    {
        Debug.Log("Barracks Placed at " + node.label);
    }
    public void PlaceMine()
    {
        Debug.Log("Mine Placed at " + node.label);
    }
    public void PlaceWalls()
    {
        Debug.Log("Walls Placed at " + node.label);
    }
}
