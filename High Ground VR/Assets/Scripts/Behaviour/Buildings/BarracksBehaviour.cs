using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarracksBehaviour : MonoBehaviour
{
    [SerializeField, Tooltip ("The Unit Prefab, used for spawning from the Barracks.")] private GameObject m_unitPrefab;
    [SerializeField, Tooltip ("The maximum amount of Units allowed from this Barracks.")] private int m_unitCount;
    [SerializeField, Tooltip("Time between respawning units.")] private int m_unitRespawnDelay;


    public bool inCombat;  //Tracks whether the unit is in combat or not.
    public Node BarracksUnitNode; //The node on which the units are placed.

    private ValidateBuildingLocation m_buildingValidation; //Script used to validate a building position, it also stores the Y offset of all placed objects.
    private Node m_barracksPlacedNode; //The node on which the barracks is placed.
    private List<GameObject> m_units; //A list of all units associated with this barracks.
    private int m_currentUnits;  //The current amount of units associated with this barracks.
    private bool m_respawning = false;


    void Start()
    {
        m_units = new List<GameObject>();
        //Assigning the node on which the barrack is placed. Try/Catch checks for any missed associations.
        try { 
            m_barracksPlacedNode = transform.GetComponentInParent<NodeComponent>().node;
            m_buildingValidation = GameBoardGeneration.Instance.BuildingValidation;
        }
        catch { 
            Debug.LogWarning("Error in assigning variables in BarracksBehaviour."); 
            return; 
        }

        
        // Finding the node on which the units should be placed.
        Vector3 _raycastPos;
        Vector3 _raycastDir;

        //Raycast out of the door and downwards to find the correct node on which units should spawn.
        _raycastDir = (transform.forward - transform.up) * 100;
        RaycastHit _hit;

        //Based on the size of the player (Small or large), change the position of which the raycast comes from.
         _raycastPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

        //If the raycast hits a node, assign BarracksUnitNode to this located node.
        Debug.DrawRay(_raycastPos, _raycastDir, Color.green);
        if (Physics.Raycast(_raycastPos, _raycastDir, out _hit))
        {
            if (_hit.collider.tag == "Environment")
            {
                BarracksUnitNode = _hit.collider.gameObject.GetComponent<NodeComponent>().node;
                BarracksUnitNode.navigability = nodeTypes.playerUnit;
            }
        }
        //Spawn the correct amount of enemies 
        for (int i = 0; i < m_unitCount; i++)
        {
            Vector3 _unitSpawnPos = new Vector3(BarracksUnitNode.hex.transform.position.x, BarracksUnitNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, BarracksUnitNode.hex.transform.position.z);
            GameObject _newUnit = Instantiate(m_unitPrefab, _unitSpawnPos, transform.rotation, BarracksUnitNode.hex.transform);
            BarracksUnitNode.navigability = nodeTypes.playerUnit;
            _newUnit.GetComponent<UnitComponent>().playerUnitConstructor();
            m_units.Add(_newUnit);
        }
        m_currentUnits = m_unitCount;
        EvaluateUnitPositions();
    }
    void Update()
    {
        //Remove null objects from m_units so that dead units don't stay in the list.
        for (int i = 0; i < m_units.Count; i++)
        {
            if (m_units[i] == null)
            {
                m_units.Remove(m_units[i]);
            }
        }
        m_currentUnits = m_units.Count;

        if(m_currentUnits < m_unitCount)
        {
            if (m_respawning == false)
            {
                m_respawning = true;
                StartCoroutine("RespawnUnits");
            }

            if(m_currentUnits == 0 && BarracksUnitNode.hex.transform.childCount == 0) // If there are no player units and there is nothing on the tile, make it navigable.
            {
                BarracksUnitNode.navigability = nodeTypes.navigable;
            }
        }

        //Check for any enemies in adjecent nodes to the friendly units.
        if(inCombat == false && m_currentUnits != 0)
        {
            CheckLocalNodes();
        }
        //If an enemy approaches on an adjecent node, add it into the battle.
        if (inCombat == true && m_currentUnits != 0)
        {
            AddEnemyToBattle();
        }
    }

    /// <summary>
    /// Takes all of the associated units within a barracks and positions them correctly around the hex based upon how many there are.
    /// </summary>
    void EvaluateUnitPositions()
    {
        //Based on size of the environment, the position on which the units should be moved to has a different Y value.
        Vector3 _hexPosition = new Vector3(BarracksUnitNode.hex.transform.position.x, BarracksUnitNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, BarracksUnitNode.hex.transform.position.z);


        //Divide the hex into angles, based on the amount of units associated with this barracks.
        float _angleDifference = 360 / (m_currentUnits + 1);
        int _index = 0;
        foreach(GameObject _gameObj in m_units)
        {
            //Place the unit at a certain position along that angle, dividing the units into equal sectors of the hexagon.
            Quaternion _angle = Quaternion.Euler(0, _angleDifference * (_index+1), 0);

            Vector3 _unitPostion = _hexPosition + (_angle * (Vector3.forward * 0.4f));
            _gameObj.transform.position = _unitPostion;
            _index++;
        }

    }

    /// <summary>
    /// Looks for enemies in adjecent nodes and creates a battle event if it finds any, adding the located enemies.
    /// </summary>
    void CheckLocalNodes()
    {
        foreach(Node _node in BarracksUnitNode.adjecant)
        {
            if (_node.navigability == nodeTypes.enemyUnit)
            {
                //If it detects an enemy group in any adjecent nodes, start combat
                if (_node.hex.transform.GetChild(0).TryGetComponent<EnemyBehaviour>(out EnemyBehaviour _enemy))
                {
                    //If the enemy is currently destroying a wall, stop it from doing so.
                    if (_enemy.inSiege == true)
                    {
                        Destroy(_enemy.gameObject.GetComponent<SiegeBehaviour>());
                        _enemy.inSiege = false;
                    }
                    if (_enemy.inCombat == false)
                    {
                        List<Unit> _friendlyUnits = new List<Unit>();
                        List<Unit> _enemyUnits = new List<Unit>();

                        for (int i = 0; i < m_units.Count; i++)
                        {
                            if (m_units[i] != null)
                            {
                                if (m_units[i].TryGetComponent(out UnitComponent _unitComp))
                                {
                                    _friendlyUnits.Add(_unitComp.unit);
                                }
                            }
                        }
                        for (int i = 0; i < _enemy.m_units.Count; i++)
                        {
                            if (_enemy.m_units[i] != null)
                            {
                                if (_enemy.m_units[i].TryGetComponent(out UnitComponent _unitComp))
                                {
                                    _enemyUnits.Add(_unitComp.unit);
                                }
                            }
                        }


                        BattleBehaviour _battle = this.gameObject.AddComponent<BattleBehaviour>();
                        _battle.StartBattle(_friendlyUnits, _enemyUnits);
                        _battle.friendlyGroups.Add(this);
                        _battle.enemyGroups.Add(_enemy);

                        _enemy.inCombat = true;
                        inCombat = true;
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Adds an enemy to the Battle if they're not already in it.
    /// </summary>
    void AddEnemyToBattle()
    {
        foreach (Node _node in BarracksUnitNode.adjecant)
        {
            if (_node.navigability == nodeTypes.enemyUnit)
            {
                //If it detects an enemy group in any adjecent nodes, get all of the units from that enemy group
                EnemyBehaviour _enemy = _node.hex.transform.GetChild(0).GetComponent<EnemyBehaviour>();
                if (_enemy.inCombat == false)
                {
                    BattleBehaviour _battle = this.gameObject.GetComponent<BattleBehaviour>();
                    List<Unit> _enemyUnits = new List<Unit>();

                    foreach (GameObject _gameObj in _enemy.m_units)
                    {
                        _enemyUnits.Add(_gameObj.GetComponent<UnitComponent>().unit);
                    }

                    bool _groupAlreadyInBattle = false;
                    foreach (Unit _enemyUnit in _enemyUnits)
                    {
                        //If the incoming enemy is already in the battle, it should't be added into the list.
                        if (_battle.enemyUnits.Contains(_enemyUnit))
                        {
                            _groupAlreadyInBattle = true;
                        }
                    }

                    if (_groupAlreadyInBattle == false)
                    {
                        _battle.JoinBattle(_enemyUnits);
                        _battle.enemyGroups.Add(_enemy);
                        _enemy.inCombat = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Controls the respawn of units from a barracks.
    /// </summary>
    /// <returns></returns>
    IEnumerator RespawnUnits()
    {
        yield return new WaitForSeconds(m_unitRespawnDelay);
        //If the target node is completely clear , Spawn a unit.
        if (BarracksUnitNode.hex.transform.childCount != 0)
        {
            if (BarracksUnitNode.hex.transform.GetChild(0).GetComponent<EnemyBehaviour>() == null)
            {
                AudioManager.Instance.PlaySound("barracksRespawn", AudioLists.Building, AudioMixers.Effects, false, true, false, this.gameObject, 0.1f);
                BarracksUnitNode.navigability = nodeTypes.playerUnit;
                SpawnAUnit();
            }
        }
        else if (BarracksUnitNode.navigability == nodeTypes.navigable || BarracksUnitNode.navigability == nodeTypes.playerUnit)
        {
            AudioManager.Instance.PlaySound("barracksRespawn", AudioLists.Building, AudioMixers.Effects, false, true, false, this.gameObject, 0.1f);
            BarracksUnitNode.navigability = nodeTypes.playerUnit;
            SpawnAUnit();
        }
        m_respawning = false;
        yield return null;
    }

    /// <summary>
    /// Spawns a Unit on the Barracks Unit Node Position.
    /// </summary>
    void SpawnAUnit()
    {
        //Correctly position units around the hex - Only needs to happen when one dies/respawns
        Vector3 _unitSpawnPos = new Vector3(BarracksUnitNode.hex.transform.position.x, BarracksUnitNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, BarracksUnitNode.hex.transform.position.z);
        GameObject _newUnit = Instantiate(m_unitPrefab, _unitSpawnPos, transform.rotation, BarracksUnitNode.hex.transform);
        BarracksUnitNode.navigability = nodeTypes.playerUnit;
        _newUnit.GetComponent<UnitComponent>().playerUnitConstructor();
        m_units.Add(_newUnit);
        EvaluateUnitPositions();
    }

}
