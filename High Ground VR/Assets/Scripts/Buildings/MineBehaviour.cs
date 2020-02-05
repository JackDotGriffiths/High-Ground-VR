using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBehaviour : MonoBehaviour
{
    private float m_currentTimer;


    // Start is called before the first frame update
    void Start()
    {
        GetTickDelay();
    }

    // Update is called once per frame
    void Update()
    {
        m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;

        if (m_currentTimer <= 0.0f)
        {
            try { GameManager.Instance.IncrementGold(); } catch { Debug.LogWarning("GameManager not found within MineBehaviour.");  }
            GetTickDelay();
        }
    }

    void GetTickDelay()
    {
        try
        {
            m_currentTimer = GameManager.Instance.TickInterval;
        }
        catch
        {
            Debug.LogWarning("GameManager not found within MineBehaviour.");
            m_currentTimer = 1.0f;
        }
    }
}
