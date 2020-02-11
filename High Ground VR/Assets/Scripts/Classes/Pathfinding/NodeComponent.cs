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
        //If the proposed position of the barrack is verified, place the mine.
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
        //If the proposed position of the barrack is verified, place the wall.
        if (buildingPlacementValidation.verifyWall(node))
        {
            buildingPlacementValidation.placeWall(node);
            Debug.Log("Walls Placed at " + node.label);
        }
        else
        {
            Debug.Log("Node not Valid");
        }
    }
}
