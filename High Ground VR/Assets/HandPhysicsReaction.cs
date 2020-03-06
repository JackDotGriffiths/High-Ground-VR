using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPhysicsReaction : MonoBehaviour
{
    [SerializeField] private GameObject m_inputPosition;

    void FixedUpdate()
    {

       if( m_inputPosition.GetComponent<HandCollision>().inCollider == false)
        {
            this.transform.position = m_inputPosition.transform.position;
            this.transform.rotation = m_inputPosition.transform.rotation;
        }
        else
        {
            this.transform.position = m_inputPosition.GetComponent<HandCollision>().enterPosition;
            this.transform.rotation = m_inputPosition.transform.rotation;
        }
    }

}
