using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Node
{
    public enum navigabilityStates { navigable, nonNavigable, destructable };

    public List<Node> adjecant = new List<Node>();
    public Node previous;
    public string label = "";
    public int x;
    public int y;

    public GameObject hex;
    public navigabilityStates navigability;

    public void Clear()
    {
        previous = null;
    }
}
