using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum navigabilityStates {navigable,barracks, mine, wall, gem, enemySpawn , enemyUnit, playerUnit};
public class Node
{

    public List<Node> adjecant = new List<Node>(); //Adjecent nodes to this node.
    public Node previous; //The previous node along the pathfinding.
    public string label = ""; //Label of the node.
    public int x; //x position of the node in the graph.
    public int y; //y position of the node in the graph

    public GameObject hex; //GameObject hexagon associated with this node.
    public navigabilityStates navigability; //The navigability of the node;

    /// <summary>
    /// Node Constructor
    /// </summary>
    /// <param name="label">Label, or name of the node</param>
    /// <param name="x">x position of the node in the graph.</param>
    /// <param name="y">y position of the node in the graph</param>
    /// <param name="hex">GameObject hexagon associated with this node.</param>
    /// <param name="navigability">The navigability of the node</param>
    public Node(string label, int x, int y, GameObject hex, navigabilityStates navigability)
    {
        this.label = label;
        this.x = x;
        this.y = y;
        this.hex = hex;
        this.navigability = navigability;
    }




    /// <summary>
    /// Used to clear the previous node of this node for pathfinding.
    /// </summary>
    /// 
    public void Clear() => previous = null;
}
