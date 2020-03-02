using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ValidateBuildingLocation : MonoBehaviour
{
    /// <summary>
    /// Does not account for player being Large or Small. Should only be used on methods ONLY ran on Start.
    /// </summary>
    [SerializeField, Tooltip ("Y Distance from the floor on which buildings should spawn")]public float buildingHeightOffset; //Y Distance from the floor to spawn the buildings.

    //Building prefabs
    [SerializeField, Space(10),Tooltip("Barracks prefab to spawn")] private GameObject m_barracks;
    [SerializeField, Space(1), Tooltip("Mine prefab to spawn")] private GameObject m_mine;
    [SerializeField, Space(1), Tooltip("Wall prefab to spawn")] private GameObject m_walls;
    [SerializeField, Space(1), Tooltip("Enemy Spawn prefab to spawn")] private GameObject m_enemySpawn;

    /// <summary>
    /// Offset from the center to the top of the hex. Use this to place buildings/objects at runtime.
    /// </summary>
    public float CurrentHeightOffset { get => buildingHeightOffset; set => buildingHeightOffset = value; }


    /// <summary>
    /// Verifies the position of any BuildingOption placed at a targetNode. 
    /// </summary>
    /// <param name="_building">Chosen Building to verify</param>
    /// <param name="_targetNode">Node on which to spawn the building.</param>
    /// <returns>Bool returns true if the building location is verified, false if not.</returns>
    public bool verifyBuilding(BuildingOption _building,Node _targetNode,float _buildingAngle)
    {
        if(_building.type == buildingTypes.Barracks)
        {
            return verifyBarracks(_targetNode, _buildingAngle);
        }
        else if (_building.type == buildingTypes.Mine)
        {
            return verifyMine(_targetNode, _buildingAngle);
        }
        else if (_building.type == buildingTypes.Wall)
        {
            return verifyWall(_targetNode, _buildingAngle);
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
    public void placeBuilding(BuildingOption _building, Node _targetNode, float _buildingAngle)
    {
        if (_building.type == buildingTypes.Barracks)
        {
            placeBarracks(_targetNode, _buildingAngle);
        }
        else if (_building.type == buildingTypes.Mine)
        {
            placeMine(_targetNode, _buildingAngle);
        }
        else if (_building.type == buildingTypes.Wall)
        {
            placeWall(_targetNode, _buildingAngle);
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
    public bool verifyBarracks(Node _targetNode, float _angle)
    {
        bool _validLocation = false;
        if (!nodeEmpty(_targetNode) || !checkAdjecentBarracks(_targetNode))
        {
            return false ;
        }
        if (nodeEmpty(_targetNode) && !adjacentToEnemySpawn(_targetNode) && !adjacentToGem(_targetNode))
        {
            _validLocation = true;
        }
        // Finding the node on which the units should be placed.
        Vector3 _raycastPos;
        Vector3 _raycastDir;

        //Raycast out of the door and downwards to find the correct node on which units should spawn.
        _raycastDir = (Quaternion.Euler(0, _angle, 0) * _targetNode.hex.transform.forward - _targetNode.hex.transform.up) * 100;
        RaycastHit _hit;

        //Based on the size of the player (Small or large), change the position of which the raycast comes from.
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.large)
        {
            _raycastPos = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + 1f, _targetNode.hex.transform.position.z);
        }
        else
        {
            _raycastPos = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + InputManager.Instance.LargestScale.y + 20, _targetNode.hex.transform.position.z);
        }

        //If the raycast hits a node, the location is valid, otherwise it isn't
        Debug.DrawRay(_raycastPos, _raycastDir, Color.green);
        if (Physics.Raycast(_raycastPos, _raycastDir, out _hit))
        {
            if (_hit.collider.tag == "Environment")
            {
                Node _hitNode = _hit.collider.gameObject.GetComponent<NodeComponent>().node;
                if(nodeEmpty(_hitNode) && !adjacentToEnemySpawn(_hitNode) && !adjacentToGem(_targetNode))
                {
                    _validLocation = true;
                }
                else
                {
                    _validLocation = false;
                }
            }
            else
            {
                _validLocation = false;
            }
        }

        foreach (Node _adjNode in _targetNode.adjecant)
        {
            if(_adjNode.navigability == navigabilityStates.enemySpawn)
            {
                _validLocation = false;
            }
        }


        if (!checkGemAccessible(_targetNode))
        {
            _validLocation = false;
        }


        return _validLocation;
    }

    /// <summary>
    /// Verify the position of a Mine
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Mine</param>
    /// <returns>True or false based on whether the targetNode can accept a Mine.</returns>
    public bool verifyMine(Node _targetNode, float _angle)
    {
        bool _validLocation = false;
        if (nodeEmpty(_targetNode) && !adjacentToEnemySpawn(_targetNode) && !adjacentToGem(_targetNode) && checkGemAccessible(_targetNode))
        {
            _validLocation = true;
        }

        return _validLocation;
    }

    /// <summary>
    /// Verify the position of a Wall
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Wall</param>
    /// <returns>True or false based on whether the targetNode can accept a Wall.</returns>
    public bool verifyWall(Node _targetNode, float _angle)
    {
        bool _validLocation = false;
        if (nodeEmpty(_targetNode) && !adjacentToEnemySpawn(_targetNode) && !adjacentToGem(_targetNode) && checkGemAccessible(_targetNode))
        {
            _validLocation = true;
        }

        return _validLocation;

    }

    /// <summary>
    /// Verify the position of a enemySpawn
    /// </summary>
    /// <param name="_targetNode">Node on which to place a enemySpawn</param>
    /// <returns>True or false based on whether the targetNode can accept a enemySpawn.</returns>
    public bool verifyEnemySpawn(Node _targetNode, float _angle)
    {
        bool _validLocation;
        if (nodeEmpty(_targetNode))
        {
            _validLocation = true;
        }
        else
        {
            _validLocation = false;
        }

        //Checks for any nonPlaceable adjecent nodes. This helps with ensuring spawns are spread out.
        foreach (Node _adjNode in _targetNode.adjecant)
        {
            if(_adjNode.navigability == navigabilityStates.enemySpawn)
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
    public void placeBarracks(Node _targetNode, float _angle)
    {
        if (!GameManager.Instance.spendGold(100))
        {
            Debug.Log("Not Enough Money");
            return;
        }
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.barracks;
        //Instantiate Relevant Prefab & Position Accordingly, based on the players current size.
        Vector3 _position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + buildingHeightOffset, _targetNode.hex.transform.position.z);
        Vector3 _rotation = new Vector3(0.0f, _angle, 0.0f);
        GameObject _building = Instantiate(m_barracks,_position,Quaternion.Euler(_rotation), _targetNode.hex.transform);


        AudioManager.Instance.PlaySound(SoundLists.placeBuildings, true, 1, _targetNode.hex, true, false, true);
    }


    /// <summary>
    /// Takes in a targetNode and places a Mine at that position
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Mine</param>
    public void placeMine(Node _targetNode, float _angle)
    {
        if (!GameManager.Instance.spendGold(50))
        {
            Debug.Log("Not Enough Money");
            return;
        }
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.mine;
        //Instantiate Relevant Prefab & Position Accordingly, based on the players current size.
        Vector3 _position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + buildingHeightOffset, _targetNode.hex.transform.position.z);
        Vector3 _rotation = new Vector3(0.0f, _angle, 0.0f);
        GameObject _building = Instantiate(m_mine, _position, Quaternion.Euler(_rotation), _targetNode.hex.transform);
        AudioManager.Instance.PlaySound(SoundLists.placeBuildings, true, 1, _targetNode.hex, true, false, true);
    }

    /// <summary>
    /// Takes in a targetNode and places a Wall at that position
    /// </summary>
    /// <param name="_targetNode">Node on which to place a Wall</param>
    public void placeWall(Node _targetNode, float _angle)
    {
        if (!GameManager.Instance.spendGold(10))
        {
            Debug.Log("Not Enough Money");
            return;
        }
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.wall;
        //Instantiate Relevant Prefab & Position Accordingly, based on the players current size.
        GameObject _building = Instantiate(m_walls, _targetNode.hex.transform);
        _building.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + buildingHeightOffset, _targetNode.hex.transform.position.z);
        AudioManager.Instance.PlaySound(SoundLists.placeBuildings, true, 1, _targetNode.hex, true, false, true);
    }

    /// <summary>
    /// Takes in a targetNode and places a enemySpawn at that position
    /// </summary>
    /// <param name="_targetNode">Node on which to place a enemySpawn</param>
    public void placeEnemySpawn(Node _targetNode, float _angle)
    {
        //Update Node navigability and surrounding nodes
        _targetNode.navigability = navigabilityStates.enemySpawn;
        //foreach (Node _adjNode in _targetNode.adjecant)
        //{
        //    _adjNode.navigability = navigabilityStates.nonPlaceable;
        //}
        //Instantiate Relevant Prefab. Position + Scale Based on size of the environment.
        GameObject _spawn = Instantiate(m_enemySpawn, _targetNode.hex.transform);
        _spawn.transform.position = new Vector3(_targetNode.hex.transform.position.x, _targetNode.hex.transform.position.y + buildingHeightOffset, _targetNode.hex.transform.position.z);
        _spawn.GetComponent<EnemySpawnBehaviour>().thisNode = _targetNode;
        GameManager.Instance.enemySpawns.Add(_spawn.GetComponent<EnemySpawnBehaviour>());
    }
    #endregion



    #region gameRules

    /// <summary>
    /// Checks whether a node is empty.
    /// </summary>
    /// <param name="_targetNode">The node to be checked</param>
    /// <returns>True/False based on whether the node is empty.</returns>
    private bool nodeEmpty(Node _targetNode)
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
    /// Checks whether the node to adjacent to any enemy spawns.
    /// </summary>
    /// <param name="_targetNode">The node to check</param>
    /// <returns>True/False depending on whether the _targetNode is adjacent to an enemy spawn.</returns>
    private bool adjacentToEnemySpawn(Node _targetNode)
    {
        foreach (Node _adjNode in _targetNode.adjecant)
        {
            if (_adjNode.navigability == navigabilityStates.enemySpawn)
            {
                return true;
            }
        }

        return false;
    }
    /// <summary>
    /// Checks whether the node to adjacent to the gem
    /// </summary>
    /// <param name="_targetNode">The node to check</param>
    /// <returns>True/False depending on whether the _targetNode is adjacent to the gem</returns>
    private bool adjacentToGem(Node _targetNode)
    {
        foreach (Node _adjNode in _targetNode.adjecant)
        {
            if (_adjNode.navigability == navigabilityStates.gem)
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// Checks whether or not the gem is still accessible.
    /// </summary>
    /// <returns></returns>
    private bool checkGemAccessible(Node _targetNode)
    {
        bool _result = true;

        //Assign the target node temporarily as a wall as to test the patfinding.
        _targetNode.navigability = navigabilityStates.wall;

        foreach(EnemySpawnBehaviour _spawn in GameManager.Instance.enemySpawns)
        {
            Node _spawnNode = _spawn.thisNode;
            bool _spawnCanAccessGem = false;
            int _accessibleNodes = 0;
            //For each of the nodes adjacent to that spawn, check whether the enemy can reach the gem.
            foreach (Node _adjecentNode in _spawnNode.adjecant)
            {
                List<Node> _path = new List<Node>();
                var graph = GameBoardGeneration.Instance.Graph;
                var search = new Search(GameBoardGeneration.Instance.Graph);
                search.Start(graph[_adjecentNode.x, _adjecentNode.y], graph[GameManager.Instance.GameGemNode.x, GameManager.Instance.GameGemNode.y], 0.0f);
                while (!search.finished)
                {
                    search.Step();
                }
                Transform[] _pathPositions = new Transform[search.path.Count];
                for (int i = 0; i < search.path.Count; i++)
                {
                    _path.Add(search.path[i]);
                }
                if (search.path.Count != 0)
                {
                    _accessibleNodes++;
                }
            }

            if(_accessibleNodes == 0)
            {
                _result = false;
            }


        }
        _targetNode.navigability = navigabilityStates.navigable;
        return _result;
    }

    /// <summary>
    /// Checks whether the target node is adjacent to any barracks or player units. Prevents placement of lots of barracks next to eachother
    /// </summary>
    /// <param name="_targetNode">Node to check</param>
    /// <returns>Whenther the node is adjacent to a barracks or playerunit</returns>
    private bool checkAdjecentBarracks(Node _targetNode)
    {
        bool _validLocation = true;
        foreach(Node _node in _targetNode.adjecant)
        {
            if(_node.navigability == navigabilityStates.barracks || _node.navigability == navigabilityStates.playerUnit)
            {
                _validLocation = false;
            }
        }
        return _validLocation;

    }

    #endregion
}
