using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager s_instance;

    [SerializeField, Range(0, 1), Tooltip("The speed of objects in the game on a scale of 0-1")] private float m_gameSpeed = 1.0f;


    [Header("Gold")]
    [SerializeField] private int m_startingGold = 10;
    [SerializeField] private float m_tickInterval = 1.0f;
    [SerializeField] private int m_goldPerTick = 1;


    [Header ("Debug Wall Displays")]
    [SerializeField,Space(5)] private TextMeshProUGUI m_goldValue;


    private int m_currentGold;





    #region Accessors
    public static GameManager Instance { get => s_instance; set => s_instance = value; }
    public float GameSpeed { get => m_gameSpeed; set => m_gameSpeed = value; }
    public int Money { get => m_currentGold;}
    public float TickInterval { get => m_tickInterval; set => m_tickInterval = value; }
    #endregion

  
    void Awake()
    {
        //Singleton Implementation
        if (s_instance == null)
            s_instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }
   


    void Start()
    {
        m_currentGold = m_startingGold;
    }



    void Update()
    {
        try { m_goldValue.text = m_currentGold.ToString(); } catch { Debug.LogWarning("Debug Wall Gold text not set in GameManager."); }
    }


    #region Money
    public void IncrementGold()
    {
        m_currentGold += m_goldPerTick;
    }
    #endregion



}
