using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class BuildingOption
{
    [Tooltip("Type of building associated.")]public buildingTypes type; //Type of building associated.
    [Tooltip("GameObject of the building")]public GameObject prefab; //GameObject of the building
    [Tooltip("The collider of the button in the UI associated with this building.")]public Collider buttonCollider; //The collider of the button in the UI associated with this.
    [Tooltip("Price of this building."),Space(10)] public int price; //Price of this building.
}
