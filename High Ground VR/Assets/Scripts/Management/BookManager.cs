﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class BookManager : MonoBehaviour
{
    private static BookManager s_instance;



    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI timerText;


    [SerializeField, Tooltip("Object within which the colliders, buttons and displays for the actions is held")] private GameObject m_actionsMenu;
    [Tooltip("All available buildings from the book menu")]public BuildingOption[] buildingOptions; //A list of all available buildings from the player's menu.


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
    }
}
