using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search
{
    public Node[,] graph; //Graph of all nodes
    public List<Node> reachable; //Complete list of reachable nodes
    public List<Node> explored; //Complete list of explored nodes within the search
    public List<Node> path; //The Chosen path after the search has occured
    public Node goalNode; //The end goal of the search
    public int iterations; //Amount of Iterations taken to find the path
    public bool finished; //Whether the search is finished
    public float unitAggression;

    public Search(Node[,] _graph)
    {
        this.graph = _graph;
    }

    public void Start(Node start, Node goal, float aggression)
    {
        reachable = new List<Node>();
        explored = new List<Node>();
        path = new List<Node>();
        reachable.Add(start);
        goalNode = goal;
        unitAggression = aggression;
        iterations = 0;

        //Clear all previously associated nodes
        for (int i = 0; i < graph.GetLength(0); i++)
        {
            for (int j = 0; j < graph.GetLength(1); j++)
            {
                graph[i, j].Clear();
            }
        }
    }

    public void Step()
    {
        //If either of these conditions are met, the search is over and it has either failed or completed.
        if (path.Count > 0){return;}
        if (reachable.Count == 0)
        {
            finished = true;
            return;
        }

        iterations++;

        //If the search has completed and was successfull, rebuild the path from the list of previous nodes from each node.
        var node = ChooseNode();
        if (node == goalNode)
        {
            while (node != null)
            {
                path.Insert(0, node);
                node = node.previous;
            }
            finished = true;
            return;
        }

        //If not, remove the current node from reachable, add it to explored and then continue to search for the goal.
        reachable.Remove(node);
        explored.Add(node);

        for (int i = 0; i < node.adjecant.Count; i++)
        {
            AddAdjacent(node, node.adjecant[i]);
        }
    }

    /// <summary>
    /// Add adjecent nodes to search and also define reachable nodes.
    /// </summary>
    /// <param name="node">Current node</param>
    /// <param name="adjacent">Adjecent nodes of current Node</param>
    public void AddAdjacent(Node node, Node adjacent)
    {
        if (FindNode(adjacent, explored) || FindNode(adjacent, reachable))
        {
            return;
        }

        adjacent.previous = node;

        if (unitAggression == 1.0f)
        {
            if (adjacent.navigability == navigabilityStates.navigable || adjacent.navigability == navigabilityStates.destructable || adjacent.navigability == navigabilityStates.gem || adjacent.navigability == navigabilityStates.playerUnit)
            {
                reachable.Add(adjacent);
            }
        }
        else
        {
            if (adjacent.navigability == navigabilityStates.navigable  ||  adjacent.navigability == navigabilityStates.gem || adjacent.navigability == navigabilityStates.playerUnit)
            {
                reachable.Add(adjacent);
            }
        }

        if (Random.Range(0.0f, 1.0f) < 0.5f)
        {
            if (adjacent.navigability == navigabilityStates.enemyUnit)
            {
                reachable.Add(adjacent);
            }
        }
    }

    /// <summary>
    /// Returns true if the node is found within the passed in list.
    /// </summary>
    /// <param name="node">Search Node</param>
    /// <param name="list">List to Search through</param>
    /// <returns></returns>
    public bool FindNode(Node node, List<Node> list)
    {
        return GetNodeIndex(node, list) >= 0;
    }


    /// <summary>
    /// Get the Index of a node in the chosen list 
    /// </summary>
    /// <param name="node">Search Node</param>
    /// <param name="list">List to search through</param>
    /// <returns></returns>
    public int GetNodeIndex(Node node, List<Node> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (node == list[i])
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Chooses a node from associated adjecent nodes, and always picks the one that reduces the distance between the current node and the goal node the most.
    /// </summary>
    /// <returns></returns>
    public Node ChooseNode()
    {

        Node _closestNode = reachable[Random.Range(0, reachable.Count)];
        Vector3 _goalNodePos = goalNode.hex.transform.position;
        float _minDistance = Mathf.Infinity;

        //Find the smallest distance from the goal node.
        for (int i = 0; i < reachable.Count-1; i++)
        {
            //explored.Add(reachable[i]);
            Vector3 _nodePos = reachable[i].hex.transform.position;
            if (Vector3.Distance(_nodePos, _goalNodePos) < _minDistance)
            {
                _minDistance = Vector3.Distance(_nodePos, _goalNodePos);
                _closestNode = reachable[i];
            }
        }

        return _closestNode;
    }

}