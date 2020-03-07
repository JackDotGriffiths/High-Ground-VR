using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollision : MonoBehaviour
{
    public bool inCollider = false;
    public Vector3 enterPosition;
    private void OnTriggerEnter(Collider other)
    {
        enterPosition = this.transform.position;
        inCollider = true;
    }
    private void OnTriggerExit(Collider other)
    {
        inCollider = false;
    }
}
