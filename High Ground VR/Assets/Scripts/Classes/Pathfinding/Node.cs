using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> adjecant = new List<Node>();
    public Node previous = null;
    public string label = "";

    public void Clear()
    {
        previous = null;
    }
}
