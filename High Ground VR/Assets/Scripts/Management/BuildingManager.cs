using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BuildingManager : MonoBehaviour
{
    private static BuildingManager s_instance;
    public enum buildingTypes {Barracks, Mine, Wall};


    public BuildingOption[] buildingOptions;



    #region Accessors
    public static BuildingManager Instance { get => s_instance; set => s_instance = value; }
    #endregion
    private void Awake()
    {
        if (s_instance == null)
            s_instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public BuildingOption GetBuilding(Collider _collider)
    {
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
}
