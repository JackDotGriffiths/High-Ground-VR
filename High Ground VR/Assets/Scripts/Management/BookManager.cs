using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum buildingTypes { Barracks, Mine, Wall };
public class BookManager : MonoBehaviour
{
    private static BookManager s_instance;

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
    /// Pass in a collider and will return the BuildingOption. Will return null if there isn't a building associated
    /// </summary>
    /// <param name="_collider">Collider chosen by the player. Collider must have a BuildingOption associated with it in the User Interface.</param>
    /// <returns>The Chosen Building</returns>
    public BuildingOption GetBuilding(Collider _collider)
    {
        //From the passed in collider, return the type of building the player has chosen.
        BuildingOption _selectedBuilding = new BuildingOption();
        for (int i = 0; i < buildingOptions.Length; i++)
        {
            if (buildingOptions[i].buttonCollider == _collider)
            {
                _selectedBuilding = buildingOptions[i];
            }
        }
        return _selectedBuilding;
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
