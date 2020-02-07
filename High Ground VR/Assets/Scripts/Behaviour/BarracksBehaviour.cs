using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarracksBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject m_unitPrefab;
    [SerializeField] private int m_unitCount;

    private ValidateBuildingLocation m_buildingValidation;
    private Node m_barracksPlacedNode;
    private Node m_barracksUnitNode;
    private List<GameObject> m_units;
    private int m_currentUnits; 


    void Start()
    {
        m_units = new List<GameObject>();
        try { 
            m_barracksPlacedNode = transform.GetComponentInParent<NodeComponent>().node;
            m_buildingValidation = GameBoardGeneration.Instance.BuildingValidation;
        }
        catch { 
            Debug.LogWarning("Error in assigning variables in BarracksBehaviour."); 
            return; 
        }

        //In order to find the correct Node, check ALL adjecent, and check which one lines up with transform.right?
        Vector3 _raycastPos;
        Vector3 _raycastDir;
        RaycastHit _hit;
        if (InputManager.Instance.m_currentSize == InputManager.SizeOptions.large)
        {
            _raycastPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
        }
        else
        {
            _raycastPos = new Vector3(transform.position.x, transform.position.y + InputManager.Instance.LargestScale.y, transform.position.z);
        }
        _raycastDir = (transform.right - transform.up) * 100;
        Debug.DrawRay(_raycastPos, _raycastDir, Color.green);
        if (Physics.Raycast(_raycastPos, _raycastDir, out _hit))
        {
            if (_hit.collider.tag == "Environment")
            {
                m_barracksUnitNode = _hit.collider.gameObject.GetComponent<NodeComponent>().node;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {  
        Debug.DrawLine(transform.position, m_barracksUnitNode.hex.transform.position,Color.blue);
        m_currentUnits = m_units.Count;
        if(m_currentUnits != m_unitCount)
        {
            Vector3 _unitSpawnPos = new Vector3(m_barracksUnitNode.hex.transform.position.x, m_barracksUnitNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.buildingHeightOffset, m_barracksUnitNode.hex.transform.position.z);
            GameObject _newUnit = Instantiate(m_unitPrefab, _unitSpawnPos, transform.rotation, m_barracksUnitNode.hex.transform);
            m_barracksUnitNode.navigability = navigabilityStates.playerUnit;
            m_unitPrefab.GetComponent<UnitComponent>().playerUnitConstructor();
            m_units.Add(_newUnit);
        }

        EvaluateUnitPositions();


    }
    void EvaluateUnitPositions()
    {
        //Based on amount of current units, split the hex into angles
        Vector3 _hexPosition = new Vector3(m_barracksUnitNode.hex.transform.position.x, m_barracksUnitNode.hex.transform.position.y + m_buildingValidation.buildingHeightOffset, m_barracksUnitNode.hex.transform.position.z);
        float _angleDifference = 360 / (m_currentUnits + 1);
        int _index = 0;
        foreach(GameObject _gameObj in m_units)
        {
            Quaternion _angle = Quaternion.Euler(0, _angleDifference * (_index+1), 0);
            Vector3 _unitPostion = _hexPosition + (_angle * (Vector3.forward * 0.4f));
            _gameObj.transform.position = _unitPostion;
            _index++;
        }

    }
}
