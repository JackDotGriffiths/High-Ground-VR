using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBehaviour : MonoBehaviour
{
    private float m_currentTimer; //Keeps track of the value of the current Timer.

    void Start()
    {
        GameManager.Instance.IncrementMines();
    }
}
