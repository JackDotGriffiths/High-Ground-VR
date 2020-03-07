﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class BookManager : MonoBehaviour
{

    private static BookManager s_instance;

    [SerializeField, Tooltip("Object within which the colliders, buttons and displays for the actions is held")] private GameObject m_actionsMenu;
    [Tooltip("All available buildings from the book menu")] public BuildingOption[] buildingOptions; //A list of all available buildings from the player's menu.



    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI moneyText2;
    public TextMeshProUGUI timerText2;

    private bool m_isShowingSpells;


    #region Accessors
    public static BookManager Instance { get => s_instance; set => s_instance = value; }
    #endregion
    private void Awake()
    {
        //Singleton Implementation
        if (s_instance == null)
            s_instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        HideActions();
    }

    /// <summary>
    /// Sets the current building through a button press.
    /// </summary>
    /// <param name="_building">Sets the current building to this option.</param>
    public void SetBuilding(int _index)
    {
        try
        {
            InputManager.Instance.CurrentlySelectedBuilding = buildingOptions[_index];
            InputManager.Instance.CurrentlySelectedSpell = (spellTypes)0;
        }
        catch
        {
            Debug.LogWarning("Error trying to find the InputManager or error with the chosen index.");
        }
    }

    /// <summary>
    /// Sets the current spell through a button press.
    /// </summary>
    /// <param name="_spell"></param>
    public void SetSpell(int _spellIndex)
    {
        try
        {
            InputManager.Instance.CurrentlySelectedBuilding = null;
            InputManager.Instance.CurrentlySelectedSpell = (spellTypes)_spellIndex;
        }
        catch
        {
            Debug.LogWarning("Error trying to find the InputManager.");
        }
    }


    /// <summary>
    /// Displays the actions Menu.
    /// </summary>
    public void ShowActions()
    {
        m_actionsMenu.SetActive(true);
    }

    /// <summary>
    /// Hides the action menu.
    /// </summary>
    public void HideActions()
    {
        m_actionsMenu.SetActive(false);
        InputManager.Instance.CurrentlySelectedSpell = (spellTypes)0;
        if (m_isShowingSpells == true)
        {
            TurnToBuildings();
        }
    }

    /// <summary>
    /// Turns the book to show the spells
    /// </summary>
    public void TurnToSpells()
    {
        gameObject.GetComponent<Animator>().Play("TurnPageToSpells");
        InputManager.Instance.CurrentlySelectedBuilding = null;
        InputManager.Instance.CurrentlySelectedSpell = (spellTypes)0;
        m_isShowingSpells = true;
    }

    /// <summary>
    /// Turns the book to show the buildings
    /// </summary>
    public void TurnToBuildings()
    {
        gameObject.GetComponent<Animator>().Play("TurnPageToBuildings");
        InputManager.Instance.CurrentlySelectedBuilding = null;
        InputManager.Instance.CurrentlySelectedSpell = (spellTypes)0;
        Debug.Log("Show Buildings");
        m_isShowingSpells = false;
    }
}
