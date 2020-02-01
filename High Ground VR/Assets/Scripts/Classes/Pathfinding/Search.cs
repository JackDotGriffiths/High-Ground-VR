using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search
{
    public Node[,] graph;
    public List<Node> reachable;
    public List<Node> explored;
    public List<Node> path;
    public Node goalNode;
    public int iterations;
    public bool finished;

    public Search(Node[,] _graph)
    {
        this.graph = _graph;
    }

    public void Start(Node start, Node goal)
    {
        reachable = new List<Node>();
        reachable.Add(start);
        goalNode = goal;

        explored = new List<Node>();
        path = new List<Node>();
        iterations = 0;


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
        if(path.Count > 0)
        {
            return;
        }
        if(reachable.Count == 0)
        {
            finished = true;
            return;
        }

        iterations++;

        var node = ChoseNode();
        if(node == goalNode)
        {
            while(node != null)
            {
                path.Insert(0, node);
                node = node.previous;
            }
            finished = true;
            return;
        }

        reachable.Remove(node);
        explored.Add(node);

        for (int i = 0; i < node.adjecant.Count; i++)
        {
            Addadjacent(node, node.adjecant[i]);
        }
    }

    public void Addadjacent(Node node, Node adjacent)
    {
        if(FindNode(adjacent,explored) || FindNode(adjacent, reachable))
        {
            return;
        }

        adjacent.previous = node;
        if(adjacent.navigability == Node.navigabilityStates.navigable || adjacent.navigability == Node.navigabilityStates.destructable)
        {
            reachable.Add(adjacent);
        }
    }

    public bool FindNode(Node node, List<Node> list)
    {
        return GetNodeIndex(node, list) >= 0;
    }


    public int GetNodeIndex(Node node, List<Node> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if(node == list[i])
            {
                return i;
            }
        }
        return -1;
    }

    public Node ChoseNode()
    {
        //Ask about the efficiency/alternatives to this.


        int _nodeCount = reachable.Count; //Equal to the amount of reachable nodes around the current Node.
        List<Node> _weightedReachable = new List<Node>(); //Used to store weighted node decisions.

        if (explored.Count > 0)
        {
            Node _currentNode = explored[explored.Count - 1];
            //FOR each node in reachable, add it a certain amount of times based on
            foreach (Node _node in reachable)
            {
                //Add all nodes to the weighted list
                _weightedReachable.Add(_node);

                //Add duplicates of nodes that would promote movement in the correct direction.
                if (_node.x > _currentNode.x && goalNode.x > _currentNode.x)//IF the goal is right and the searching node is right, add it twice.
                {
                    _weightedReachable.Add(_node);
                    _weightedReachable.Add(_node);
                }
                if (_node.x < _currentNode.x && goalNode.x < _currentNode.x)//IF the goal is left and the searching node is left, add it twice.
                {
                    _weightedReachable.Add(_node);
                    _weightedReachable.Add(_node);
                }
                if (_node.y > _currentNode.y && goalNode.y > _currentNode.y)//IF the goal is up and the searching node is up
                { 
                    _weightedReachable.Add(_node);
                }
                if (_node.y < _currentNode.y && goalNode.y < _currentNode.y)//IF the goal is down and the searching node is down
                {
                    _weightedReachable.Add(_node);
                }
            }
            return _weightedReachable[Random.Range(0, _weightedReachable.Count)];
        }
        

        return reachable[Random.Range(0, reachable.Count)];

    }

}
