using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : MonoBehaviour
{
    private float m_battleTimer = 2.0f;
    private float m_currentTimer;

    private void Start()
    {
        m_currentTimer = m_battleTimer;
    }
    void Update()
    {
        m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;
        if (m_currentTimer < 0)
        {
            Debug.Log("Attack");
            m_currentTimer = m_battleTimer;
        }
    }
}
