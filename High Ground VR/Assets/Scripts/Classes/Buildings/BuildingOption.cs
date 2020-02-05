using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class BuildingOption
{
    public BuildingManager.buildingTypes type;
    public GameObject prefab;
    public Collider buttonCollider;
    [Space(10)] public int price;
}
