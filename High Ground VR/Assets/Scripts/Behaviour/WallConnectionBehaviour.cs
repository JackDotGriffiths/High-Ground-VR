using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallConnectionBehaviour : MonoBehaviour
{
    public GameObject connectorObject; 
    public List<GameObject> connectedWalls; //A list of all connected walls to this object.


    private Node thisNode;

   void Start()
   {
        thisNode = this.GetComponentInParent<NodeComponent>().node;
        Debug.Log("ThisNode is " + thisNode.hex.name);
        foreach(Node _node in thisNode.adjecant)
        {
            //If the adjacent node contains a building that is worth connecting to, place a connector.
            if(_node.navigability == navigabilityStates.wall)
            {
                //Create a connector
                Vector3 _thisPos = thisNode.hex.transform.position;
                Vector3 _goalPos = _node.hex.transform.position;
                Vector3 _spawnPos = _goalPos + (_thisPos - _goalPos) / 2;
                _spawnPos = new Vector3(_spawnPos.x, _spawnPos.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, _spawnPos.z);

                Quaternion _rotation = Quaternion.FromToRotation(transform.forward, _thisPos - _goalPos);
                GameObject _wall = Instantiate(connectorObject,_spawnPos, _rotation,transform);
                connectedWalls.Add(_wall);
                if(_node.navigability == navigabilityStates.wall)
                {
                    WallConnectionBehaviour _wallBehaviour = _node.hex.GetComponentInChildren<WallConnectionBehaviour>();
                    _wallBehaviour.connectedWalls.Add(_wall);
                }
            }
        }


   }

    private void OnDestroy()
    {
        foreach (GameObject _wall in connectedWalls)
        {
            Destroy(_wall);
        }
    }

}
