using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeComponent : MonoBehaviour
{
    public Node node; //The current node of this gameObject.
    [Tooltip ("The Building Validation tool, for use with spawning buildings from inspector buttons.")] public ValidateBuildingLocation buildingPlacementValidation; //Used to validate and place buildings

    public void PlaceBarracks()
    {
        //If the proposed position of the barrack is verified, place the barracks.
        if (buildingPlacementValidation.verifyBarracks(node,0.0f))
        {
            buildingPlacementValidation.placeBarracks(node, 0.0f);
            Debug.Log("Barracks Placed at " + node.label);
        }
        else
        {
            Debug.Log("Node Occupied");
        }
    }
    public void PlaceMine()
    {
        //If the proposed position of the barrack is verified, place the mine.
        if (buildingPlacementValidation.verifyMine(node, 0.0f))
        {
            buildingPlacementValidation.placeMine(node, 0.0f);
            Debug.Log("Mine Placed at " + node.label);
        }
        else
        {
            Debug.Log("Node Occupied");
        }
    }
    public void PlaceWalls()
    {
        //If the proposed position of the barrack is verified, place the wall.
        if (buildingPlacementValidation.verifyWall(node, 0.0f))
        {
            buildingPlacementValidation.placeWall(node, 0.0f);
            Debug.Log("Walls Placed at " + node.label);
        }
        else
        {
            Debug.Log("Node not Valid");
        }
    }
}
