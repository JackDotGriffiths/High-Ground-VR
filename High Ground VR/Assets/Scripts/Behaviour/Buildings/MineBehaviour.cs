using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBehaviour : MonoBehaviour
{
    private float m_currentTimer; //Keeps track of the value of the current Timer.

    void Start()
    {
        GetTickDelay();
    }

    void Update()
    {
        //Minus deltaTime, multiplied by GameSpeed for reasons of pausing, slow mo etc.
        m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;

        //If timer reaches 0 or less, increment the gold by the amount determined in GameManager. Then restart timer.
        if (m_currentTimer <= 0.0f)
        {
            try { GameManager.Instance.IncrementGold(); } catch { Debug.LogWarning("GameManager not found within MineBehaviour.");  }
            GetTickDelay();
        }
    }

    /// <summary>
    /// Sets m_currentTimer to the TickInterval value within GameManager.
    /// </summary>
    void GetTickDelay()
    {
        //Try/Catch checks for any missing scripts, and will log a warning.
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
