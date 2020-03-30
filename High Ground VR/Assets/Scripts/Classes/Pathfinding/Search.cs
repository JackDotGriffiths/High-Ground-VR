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

        //---Unit aggression
        //1.0 = MAX anger
        //0.0 = MIN anger


        if (adjacent.navigability == navigabilityStates.navigable || adjacent.navigability == navigabilityStates.gem || adjacent.navigability == navigabilityStates.playerUnit || adjacent.navigability == navigabilityStates.enemyUnit)
        {
            reachable.Add(adjacent);
        }

        //Creates a decreasing value, meaning it's EASIER for a unit to be aggressive as the rounds go on.
        float _aggressionChance = 1.0f - (GameManager.Instance.aggressionPercentage * (GameManager.Instance.RoundCounter/2.0f));
        //If the aggression is 1, always add the destructible node. This is used for checking of pathfinding and super aggressive enemies.
        if (unitAggression == 1.0f)
        {
            if (adjacent.navigability == navigabilityStates.wall || adjacent.navigability == navigabilityStates.mine )
            {
                reachable.Add(adjacent);
            }
        }
        else if(unitAggression > _aggressionChance)
        {
            if (adjacent.navigability == navigabilityStates.wall || adjacent.navigability == navigabilityStates.mine)
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
        //float _minDistance = Mathf.Infinity;
        //for (int i = 0; i < reachable.Count - 1; i++)
        //    Vector3 _nodePos = reachable[i].hex.transform.position;
        //if (Vector3.Distance(_nodePos, _goalNodePos) < _minDistance)
        //    _minDistance = Vector3.Distance(_nodePos, _goalNodePos);
        //_closestNode = reachable[i];
        //return _closestNode;

        //Randomly chooses a node which is in the correct direction, giving some variety to enemy paths. The commented out section above is the perfect pathfinding.

        //List of Nodes from reachable
        List<Node> _searchNodes = reachable;

        //If theres only one available
        if (_searchNodes.Count == 1)
        {
            return _searchNodes[0];
        }
        else if (_searchNodes.Count == 2) //Randomly choose if there's two available
        {
            if (checkIfInCombat(_searchNodes[0])) // Check if it's in combat, if it is, avoid it.
            {
                return _searchNodes[1];
            }
            else
            {
                return _searchNodes[0];
            }
        }


        Vector3 _goalNodePos = goalNode.hex.transform.position;
        //Bubble Sort in distance from _it to the gem
        Node _tempNode;
        for (int j = 0; j < _searchNodes.Count - 2; j++)
        {
            for (int i = 0; i < _searchNodes.Count - 2; i++)
            {
                Vector3 _node1Pos = _searchNodes[i].hex.transform.position;
                Vector3 _node2Pos = _searchNodes[i + 1].hex.transform.position;
                float _distance1 = Vector3.Distance(_node1Pos, _goalNodePos);
                float _distance2 = Vector3.Distance(_node2Pos, _goalNodePos);
                if (_distance1 > _distance2)
                {
                    _tempNode = _searchNodes[i + 1];
                    _searchNodes[i + 1] = _searchNodes[i];
                    _searchNodes[i] = _tempNode;
                }
            }
        }


        if (checkIfInCombat(_searchNodes[0])) // Check if it's in combat, if it is, avoid it.
            {
            return _searchNodes[1];
        }
        else
        {
            return _searchNodes[0];
        }

    }

    /// <summary>
    /// Checks whether a node has a unit that's in combat on it, if so, it will try and avoid creating a queue.
    /// </summary>
    /// <param name="_node">The node to check</param>
    /// <returns>True if the node contains a unit in combat.</returns>
    private bool checkIfInCombat(Node _node)
    {
        bool _nodeInCombat = false;
        if(_node.navigability == navigabilityStates.enemyUnit)
        {
            if(_node.hex.GetComponentInChildren<EnemyBehaviour>().inCombat == true || _node.hex.GetComponentInChildren<EnemyBehaviour>().inSiege == true)
            {
                _nodeInCombat = true;
            }
        }
        return _nodeInCombat;
    }
}