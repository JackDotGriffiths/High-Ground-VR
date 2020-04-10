using System.Collections.Generic;
using UnityEngine;

public class PathfindingData 
{
    public bool isNavigable;

    public Node parentNode; // The previously searched node.
    public float G { get; set; } //The length of the path from the start node to this node.
    public float H { get; set; } //The straight-line distance from this node to the end node.
    public float F { get; set; } //An estimate of the total distance if taking this route.It’s calculated simply using F = G + H.

}