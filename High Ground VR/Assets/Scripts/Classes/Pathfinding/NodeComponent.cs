using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeComponent : MonoBehaviour
{
    public Node node;

    [Header("Node Testing")]
    [SerializeField] private Node.navigabilityStates navigability = Node.navigabilityStates.navigable;

    private void Update()
    {
        if(node.navigability != navigability)
        {
            node.navigability = navigability;
        }
    }
    private void OnDrawGizmos()
    {
        if (navigability == Node.navigabilityStates.nonNavigable)
        {
            Gizmos.color = Color.black;
            Vector3 _newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            Gizmos.DrawCube(_newPos,Vector3.one);

        }
        if (navigability == Node.navigabilityStates.destructable)
        {
            Gizmos.color = Color.red;
            Vector3 _newPos = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            Gizmos.DrawWireCube(_newPos, Vector3.one);

        }
    }
}
