using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeComponent : MonoBehaviour
{
    public Node node;
    public ValidateBuildingLocation buildingPlacementValidation;

    [ContextMenu("Expose Node Data")]
    void PrintNodeInfo()
    {
        Debug.Log(node.adjecant);
    }

    public void PlaceBarracks()
    {
        if (buildingPlacementValidation.verifyBarracks(node))
        {
            buildingPlacementValidation.placeBarracks(node);
            Debug.Log("Barracks Placed at " + node.label);
        }
        else
        {
            Debug.Log("Node Occupied");
        }
    }
    public void PlaceMine()
    {
        if (buildingPlacementValidation.verifyMine(node))
        {
            buildingPlacementValidation.placeMine(node);
            Debug.Log("Mine Placed at " + node.label);
        }
        else
        {
            Debug.Log("Node Occupied");
        }
    }
    public void PlaceWalls()
    {
        if (buildingPlacementValidation.verifyWall(node))
        {
            buildingPlacementValidation.placeWall(node);
            Debug.Log("Walls Placed at " + node.label);
        }
        else
        {
            Debug.Log("Node Occupied");
        }
    }
}
