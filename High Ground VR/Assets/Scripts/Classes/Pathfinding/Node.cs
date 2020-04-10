using System.Collections.Generic;
using UnityEngine;
public enum navigabilityStates {navigable,barracks, mine, wall, gem, enemySpawn , enemyUnit, playerUnit};
public class Node
{
    public string label = ""; //Label of the node. Essentially the name.
    public navigabilityStates navigability; //The navigability state of the node;
    public GameObject hex; //GameObject hexagon associated with this node.
    public List<Node> adjecant = new List<Node>(); //Adjecent nodes to this node.
    public int x; //x position of the node in the graph.
    public int y; //y position of the node in the graph
    public PathfindingData searchData;

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
        this.searchData = new PathfindingData();
    }
}
