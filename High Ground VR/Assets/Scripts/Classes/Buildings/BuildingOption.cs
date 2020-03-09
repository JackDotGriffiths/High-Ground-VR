using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum buildingTypes { Barracks, Mine, Wall };

[Serializable]
public class BuildingOption
{ 

    [Tooltip("Name of the building")] public string name;
    [Tooltip("Type of the building")] public buildingTypes type;
    [Tooltip("GameObject of the building")]public GameObject prefab; //GameObject of the building
    [Tooltip("The collider of the button in the UI associated with this building.")]public Collider buttonCollider; //The collider of the button in the UI associated with this.
    [Tooltip("Price of this building."),Space(10)] public int price; //Price of this building.
}
