using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidateBuildingLocation : MonoBehaviour
{
    public float buildingHeightOffset;
    [SerializeField, Space(10)] private GameObject m_barracks;
    [SerializeField,Space(1)]private GameObject m_mine, m_walls,m_enemySpawn;

    //These need to verify the adjecent nodes to the target one, and may need to do pathfinding to ensure that the enemies can still reach the player's gem.


    public bool verifyBuilding(BuildingOption _building,Node _targetNode)
    {
        if(_building.type == BuildingManager.buildingTypes.Barracks)
        {
            return verifyBarracks(_targetNode);
        }
        else if (_building.type == BuildingManager.buildingTypes.Mine)
        {
            return verifyMine(_targetNode);
        }
        else if (_building.type == BuildingManager.buildingTypes.Wall)
        {
            return verifyWall(_targetNode);
        }
        else
        {
            Debug.LogError("Building Selection not setup in ValidateBuildingLocation.cs  script on EnvironmentObject");
            return false;
        }
    }
    public void placeBuilding(BuildingOption _building, Node _targetNode)
    {
        if (_building.type == BuildingManager.buildingTypes.Barracks)
        {
            placeBarracks(_targetNode);
        }
        else if (_building.type == BuildingManager.buildingTypes.Mine)
        {
            placeMine(_targetNode);
        }
        else if (_building.type == BuildingManager.buildingTypes.Wall)
        {
            placeWall(_targetNode);
        }
        else
        {
            Debug.LogError("Building Selection not setup in ValidateBuildingLocation.cs  script on EnvironmentObject");
        }
    }



    public bool verifyBarracks(Node _targetNode)
    {
        if(_targetNode.navigability == navigabilityStates.navigable)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool verifyMine(Node _targetNode)
    {
        if (_targetNode.navigability == navigabilityStates.navigable)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool verifyWall(Node _targetNode)
    {
        if (_targetNode.navigability == navigabilityStates.navigable)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool verifyEnemySpawn(Node _targetNode)
    {
        if (_targetNode.navigability == navigabilityStates.navigable)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void placeBarracks(Node _targetNode)
    {
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.destructable;
        //Instantiate Relevant Prefab & Position Accordingly.
        GameObject _building = Instantiate(m_barracks, _targetNode.hex.transform);
        float _yOffset = buildingHeightOffset;
        if (InputManager.Instance.m_currentSize == InputManager.SizeOptions.small)
        {
            _yOffset = buildingHeightOffset * InputManager.Instance.LargestScale.y + 20;
        }
        _building.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + _yOffset, _targetNode.hex.transform.position.z);
    }
    public void placeMine(Node _targetNode)
    {
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.destructable;
        //Instantiate Relevant Prefab
        GameObject _building = Instantiate(m_mine, _targetNode.hex.transform);
        float _yOffset = buildingHeightOffset;
        if (InputManager.Instance.m_currentSize == InputManager.SizeOptions.small)
        {
            _yOffset = buildingHeightOffset * InputManager.Instance.LargestScale.y + 20;
        }
        _building.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + _yOffset, _targetNode.hex.transform.position.z);
    }
    public void placeWall(Node _targetNode)
    {
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.destructable;
        //Instantiate Relevant Prefab
        GameObject _building = Instantiate(m_walls, _targetNode.hex.transform);
        float _yOffset = buildingHeightOffset;
        if (InputManager.Instance.m_currentSize == InputManager.SizeOptions.small)
        {
            _yOffset = buildingHeightOffset * InputManager.Instance.LargestScale.y + 20;
        }
        _building.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + _yOffset, _targetNode.hex.transform.position.z);
    }
    public void placeEnemySpawn(Node _targetNode)
    {
        //Update Node navigability and surrounding nodes
        //Instantiate Relevant Prefab
        //Position + Scale Based on size of the environment.
    }
}
