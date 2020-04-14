using System.Collections.Generic;
using UnityEngine;

public class PathfindingData 
{
    public bool isNavigable;

    public Node parentNode; // The previously searched node.
    public float G; //The length of the path from the start node to this node.
    public float H; //The straight-line distance from this node to the end node.
    public float F; //An estimate of the total distance if taking this route.It’s calculated simply using F = G + H.

    public PathfindingData(Node _parent, float _G, float _H, float _F)
    {
        this.parentNode = _parent;
        this.G = _G;
        this.H = _H;
        this.F = _F;
    }

}