using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarracksBehaviour : MonoBehaviour
{
    [SerializeField, Tooltip ("The Unit Prefab, used for spawning from the Barracks.")] private GameObject m_unitPrefab;
    [SerializeField, Tooltip ("The maximum amount of Units allowed from this Barracks.")] private int m_unitCount;
    [SerializeField, Tooltip("Time between respawning units.")] private int m_unitRespawnDelay;


    public bool inCombat;  //Tracks whether the unit is in combat or not.

    private ValidateBuildingLocation m_buildingValidation; //Script used to validate a building position, it also stores the Y offset of all placed objects.
    private Node m_barracksPlacedNode; //The node on which the barracks is placed.
    private Node m_barracksUnitNode; //The node on which the units are placed.
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

        //If the raycast hits a node, assign m_barracksUnitNode to this located node.
        Debug.DrawRay(_raycastPos, _raycastDir, Color.green);
        if (Physics.Raycast(_raycastPos, _raycastDir, out _hit))
        {
            if (_hit.collider.tag == "Environment")
            {
                m_barracksUnitNode = _hit.collider.gameObject.GetComponent<NodeComponent>().node;
                m_barracksUnitNode.navigability = navigabilityStates.playerUnit;
            }
        }
        //Spawn the correct amount of enemies 
        for (int i = 0; i < m_unitCount; i++)
        {
            Vector3 _unitSpawnPos = new Vector3(m_barracksUnitNode.hex.transform.position.x, m_barracksUnitNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_barracksUnitNode.hex.transform.position.z);
            GameObject _newUnit = Instantiate(m_unitPrefab, _unitSpawnPos, transform.rotation, m_barracksUnitNode.hex.transform);
            m_barracksUnitNode.navigability = navigabilityStates.playerUnit;
            _newUnit.GetComponent<UnitComponent>().playerUnitConstructor();
            m_units.Add(_newUnit);
        }
        m_currentUnits = m_unitCount;
        EvaluateUnitPositions();
    }

    void Update()
    {
        //Remove null objects from m_units so that dead units don't stay in the list.
        foreach (GameObject _unit in m_units)
        {
            if(_unit == null)
            {
                m_units.Remove(_unit);
            }
        }
        m_currentUnits = m_units.Count;

        if(m_currentUnits == 0)
        {
            m_barracksUnitNode.navigability = navigabilityStates.navigable;
            if(m_respawning == false)
            {
                m_respawning = true;
                StartCoroutine("RespawnUnits");
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
        Vector3 _hexPosition = new Vector3(m_barracksUnitNode.hex.transform.position.x, m_barracksUnitNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_barracksUnitNode.hex.transform.position.z);


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
        foreach(Node _node in m_barracksUnitNode.adjecant)
        {
            if (_node.navigability == navigabilityStates.enemyUnit)
            {
                //If it detects an enemy group in any adjecent nodes, start combat
                EnemyGroupBehaviour _enemy = _node.hex.transform.GetChild(0).GetComponent<EnemyGroupBehaviour>();
                if(_enemy.inCombat == false)
                {
                    List<Unit> _friendlyUnits = new List<Unit>();
                    List<Unit> _enemyUnits = new List<Unit>();

                    foreach (GameObject _gameObj in m_units)
                    {
                        _friendlyUnits.Add(_gameObj.GetComponent<UnitComponent>().unit);
                    }
                    foreach (GameObject _gameObj in _enemy.m_units)
                    {
                        _enemyUnits.Add(_gameObj.GetComponent<UnitComponent>().unit);
                    }

                    BattleBehaviour _battle = this.gameObject.AddComponent<BattleBehaviour>();
                    _battle.StartBattle(_friendlyUnits, _enemyUnits);
                    _battle.m_friendlyGroups.Add(this);
                    _battle.m_enemyGroups.Add(_enemy);

                    _enemy.inCombat = true;
                    inCombat = true;
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
        foreach (Node _node in m_barracksUnitNode.adjecant)
        {
            if (_node.navigability == navigabilityStates.enemyUnit)
            {
                //If it detects an enemy group in any adjecent nodes, get all of the units from that enemy group
                EnemyGroupBehaviour _enemy = _node.hex.transform.GetChild(0).GetComponent<EnemyGroupBehaviour>();
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
                        if (_battle.m_enemyUnits.Contains(_enemyUnit))
                        {
                            _groupAlreadyInBattle = true;
                        }
                    }

                    if (_groupAlreadyInBattle == false)
                    {
                        _battle.JoinBattle(_enemyUnits);
                        _battle.m_enemyGroups.Add(_enemy);
                        _enemy.inCombat = true;
                    }
                }
            }
        }
    }


    IEnumerator RespawnUnits()
    {
        int _difference = m_unitCount - m_currentUnits;
        for (int i = 0; i < _difference; i++)
        {
            if (m_barracksUnitNode.navigability == navigabilityStates.navigable)
            {
                //Correctly position units around the hex - Only needs to happen when one dies/respawns
                m_barracksUnitNode.navigability = navigabilityStates.playerUnit;
                Vector3 _unitSpawnPos = new Vector3(m_barracksUnitNode.hex.transform.position.x, m_barracksUnitNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_barracksUnitNode.hex.transform.position.z);
                GameObject _newUnit = Instantiate(m_unitPrefab, _unitSpawnPos, transform.rotation, m_barracksUnitNode.hex.transform);
                m_barracksUnitNode.navigability = navigabilityStates.playerUnit;
                _newUnit.GetComponent<UnitComponent>().playerUnitConstructor();
                m_units.Add(_newUnit);
                EvaluateUnitPositions();
            }
            else
            {
                i--;
            }
            yield return new WaitForSeconds(m_unitRespawnDelay);
        }
        m_respawning = false;
    }


}
