using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarracksBehaviour : MonoBehaviour
{
    [SerializeField, Tooltip ("The Unit Prefab, used for spawning from the Barracks.")] private GameObject m_unitPrefab;
    [SerializeField, Tooltip ("The maximum amount of Units allowed from this Barracks.")] private int m_unitCount;


    public bool inCombat;  //Tracks whether the unit is in combat or not.

    private ValidateBuildingLocation m_buildingValidation; //Script used to validate a building position, it also stores the Y offset of all placed objects.
    private Node m_barracksPlacedNode; //The node on which the barracks is placed.
    private Node m_barracksUnitNode; //The node on which the units are placed.
    private List<GameObject> m_units; //A list of all units associated with this barracks.
    private int m_currentUnits;  //The current amount of units associated with this barracks.
   


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
        _raycastDir = (transform.right - transform.up) * 100;
        RaycastHit _hit;

        //Based on the size of the player (Small or large), change the position of which the raycast comes from.
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.large)
        {
            _raycastPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        }
        else
        {
            _raycastPos = new Vector3(transform.position.x, transform.position.y + InputManager.Instance.LargestScale.y + 20, transform.position.z);
        }

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


    }

    void Update()
    {  

        //Temporary code checking the amount of units required and instantiating the correct amount. This will eventually be on a spawning delay.
        //Debug.DrawLine(transform.position, m_barracksUnitNode.hex.transform.position,Color.blue);
        m_currentUnits = m_units.Count;
        if(m_currentUnits != m_unitCount)
        {
            Vector3 _unitSpawnPos = new Vector3(m_barracksUnitNode.hex.transform.position.x, m_barracksUnitNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, m_barracksUnitNode.hex.transform.position.z);
            GameObject _newUnit = Instantiate(m_unitPrefab, _unitSpawnPos, transform.rotation, m_barracksUnitNode.hex.transform);
            m_barracksUnitNode.navigability = navigabilityStates.playerUnit;
            m_unitPrefab.GetComponent<UnitComponent>().playerUnitConstructor();
            m_units.Add(_newUnit);
        }

        //Correctly position units around the hex - Only needs to happen when one dies/respawns
        EvaluateUnitPositions();

        //Check for any enemies in adjecent nodes to the friendly units.
        CheckLocalNodes();
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

            float _multiplier = 1;
            if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.small)
            {
                _multiplier = InputManager.Instance.LargestScale.y + 20;
            }

            Vector3 _unitPostion = _hexPosition + (_angle * (Vector3.forward * 0.4f) * _multiplier);
            _gameObj.transform.position = _unitPostion;
            _index++;
        }

    }


    void CheckLocalNodes()
    {
        foreach(Node _node in m_barracksUnitNode.adjecant)
        {
            if (_node.navigability == navigabilityStates.enemyUnit)
            {
                //If it detects an enemy unit in any adjecent nodes, start combat
                this.gameObject.AddComponent()
            }
        }
    }
}
