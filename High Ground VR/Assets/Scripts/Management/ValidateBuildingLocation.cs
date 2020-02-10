using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValidateBuildingLocation : MonoBehaviour
{
    [Tooltip ("Y Distance from the floor on which buildings should spawn")]public float buildingHeightOffset; //Y Distance from the floor to spawn the buildings.

    //Building prefabs
    [SerializeField, Space(10),Tooltip("Barracks prefab to spawn")] private GameObject m_barracks;
    [SerializeField, Space(1), Tooltip("Mine prefab to spawn")] private GameObject m_mine;
    [SerializeField, Space(1), Tooltip("Wall prefab to spawn")] private GameObject m_walls;
    [SerializeField, Space(1), Tooltip("Enemy Spawn prefab to spawn")] private GameObject m_enemySpawn;


    /// <summary>
    /// Verifies the position of any BuildingOption placed at a targetNode. 
    /// </summary>
    /// <param name="_building">Chosen Building to verify</param>
    /// <param name="_targetNode">Node on which to spawn the building.</param>
    /// <returns>Bool returns true if the building location is verified, false if not.</returns>
    public bool verifyBuilding(BuildingOption _building,Node _targetNode)
    {
        if(_building.type == buildingTypes.Barracks)
        {
            return verifyBarracks(_targetNode);
        }
        else if (_building.type == buildingTypes.Mine)
        {
            return verifyMine(_targetNode);
        }
        else if (_building.type == buildingTypes.Wall)
        {
            return verifyWall(_targetNode);
        }
        else
        {
            Debug.LogError("Building Selection not setup in ValidateBuildingLocation.cs  script on EnvironmentObject");
            return false;
        }
    }

    /// <summary>
    /// Places the chosen BuildingOption at the targetNode.
    /// </summary>
    /// <param name="_building">Building you wish to spawn.</param>
    /// <param name="_targetNode">Target Node</param>
    public void placeBuilding(BuildingOption _building, Node _targetNode)
    {
        if (_building.type == buildingTypes.Barracks)
        {
            placeBarracks(_targetNode);
        }
        else if (_building.type == buildingTypes.Mine)
        {
            placeMine(_targetNode);
        }
        else if (_building.type == buildingTypes.Wall)
        {
            placeWall(_targetNode);
        }
        else
        {
            Debug.LogError("Building Selection not setup in ValidateBuildingLocation.cs  script on EnvironmentObject");
        }
    }


    //Different Verification Rules that apply to each building. These must be setup in order to create a new building.
    #region Verification Methods

    /// <summary>
    /// Verify the position of a Barracks
    /// </summary>
    /// <param name="_targetNode">Node on which to place a barracks</param>
    /// <returns>True or false based on whether the targetNode can accept a Barracks.</returns>
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

    /// <summary>
    /// Verify the position of a Mine
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Mine</param>
    /// <returns>True or false based on whether the targetNode can accept a Mine.</returns>
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

    /// <summary>
    /// Verify the position of a Wall
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Wall</param>
    /// <returns>True or false based on whether the targetNode can accept a Wall.</returns>
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

    /// <summary>
    /// Verify the position of a enemySpawn
    /// </summary>
    /// <param name="_targetNode">Node on which to place a enemySpawn</param>
    /// <returns>True or false based on whether the targetNode can accept a enemySpawn.</returns>
    public bool verifyEnemySpawn(Node _targetNode)
    {
        bool _validLocation = true;
        if (_targetNode.navigability == navigabilityStates.navigable)
        {
            _validLocation = true;
        }
        else
        {
            _validLocation = false;
        }

        //Checks for any nonNavigable adjecent nodes. This helps with ensuring spawns are spread out.
        foreach(Node _adjNode in _targetNode.adjecant)
        {
            if(_adjNode.navigability == navigabilityStates.nonNavigable)
            {
                _validLocation = false;
            }
        }

        return _validLocation;
    }
    #endregion



    //Different Placing rules that apply to each building, and affecting Nodes around it in different ways.
    #region Placing Methods

    /// <summary>
    /// Takes in a targetNode and places a Barracks at that position
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Barracks</param>
    public void placeBarracks(Node _targetNode)
    {
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.destructable;
        //Instantiate Relevant Prefab & Position Accordingly, based on the players current size.
        GameObject _building = Instantiate(m_barracks, _targetNode.hex.transform);
        float _yOffset = buildingHeightOffset;
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.small)
        {
            _yOffset = buildingHeightOffset * InputManager.Instance.LargestScale.y + 20;
        }
        _building.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + _yOffset, _targetNode.hex.transform.position.z);
    }


    /// <summary>
    /// Takes in a targetNode and places a Mine at that position
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Mine</param>
    public void placeMine(Node _targetNode)
    {
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.destructable;
        //Instantiate Relevant Prefab & Position Accordingly, based on the players current size.
        GameObject _building = Instantiate(m_mine, _targetNode.hex.transform);
        float _yOffset = buildingHeightOffset;
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.small)
        {
            _yOffset = buildingHeightOffset * InputManager.Instance.LargestScale.y + 20;
        }
        _building.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + _yOffset, _targetNode.hex.transform.position.z);
    }

    /// <summary>
    /// Takes in a targetNode and places a Wall at that position
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Wall</param>
    public void placeWall(Node _targetNode)
    {
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.destructable;
        //Instantiate Relevant Prefab & Position Accordingly, based on the players current size.
        GameObject _building = Instantiate(m_walls, _targetNode.hex.transform);
        float _yOffset = buildingHeightOffset;
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.small)
        {
            _yOffset = buildingHeightOffset * InputManager.Instance.LargestScale.y + 20;
        }
        _building.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + _yOffset, _targetNode.hex.transform.position.z);
    }

    /// <summary>
    /// Takes in a targetNode and places a enemySpawn at that position
    /// </summary>
    /// <param name="_targetNode">Node on which to place a enemySpawn</param>
    public void placeEnemySpawn(Node _targetNode)
    {
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.nonNavigable;
        foreach(Node _adjNode in _targetNode.adjecant)
        {
            _adjNode.navigability = navigabilityStates.nonNavigable;
        }
        //Instantiate Relevant Prefab. Position + Scale Based on size of the environment.
        GameObject _spawn = Instantiate(m_enemySpawn, _targetNode.hex.transform);
        float _yOffset = buildingHeightOffset;
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.small)
        {
            _yOffset = buildingHeightOffset * InputManager.Instance.LargestScale.y + 20;
        }
        _spawn.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + _yOffset, _targetNode.hex.transform.position.z);
    }
    #endregion
}
