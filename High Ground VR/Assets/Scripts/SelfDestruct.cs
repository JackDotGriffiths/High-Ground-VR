using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float m_timer;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, m_timer);
    }

}
