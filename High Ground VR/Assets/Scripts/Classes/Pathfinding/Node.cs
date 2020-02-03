using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public enum navigabilityStates { navigable, nonNavigable, destructable };

    public List<Node> adjecant = new List<Node>();
    public string label = "";
    public int weighting;
    public int x;
    public int y;

    public GameObject hex;
    public navigabilityStates navigability;
}
