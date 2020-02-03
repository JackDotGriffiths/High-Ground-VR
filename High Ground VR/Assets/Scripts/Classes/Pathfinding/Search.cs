using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search
{
    public Node[,] graph;
    public List<Node> reachable;
    public List<Node> explored;
    public List<Node> path;
    public Node startNode;
    public Node goalNode;
    public int iterations;
    public bool finished;

    private int m_currentWeight = 0;
    public Search(Node[,] _graph)
    {
        this.graph = _graph;
    }
    public void Start(Node start, Node goal)
    {
        reachable = new List<Node>();
        reachable.Add(start);
        startNode = start;
        goalNode = goal;

        explored = new List<Node>();
        path = new List<Node>();
        iterations = 0;


        for (int i = 0; i < graph.GetLength(0); i++)
        {
            for (int j = 0; j < graph.GetLength(1); j++)
            {
                graph[i, j].weighting = 0;
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
            while(node != startNode)
            {
                //Remake the path based on implemented weightings.
                Node _next = FindLowestWeight(node);
                path.Insert(0,_next);
                node = _next;
            }
            finished = true;
            return;
        }

        reachable.Remove(node);
        explored.Add(node);

        m_currentWeight++;
        for (int i = 0; i < node.adjecant.Count; i++)
        {
            Addadjacent(node, node.adjecant[i]);
        }
    }

    private void Addadjacent(Node node, Node adjacent)
    {
        if (adjacent.weighting == 0)
        {
            adjacent.weighting = m_currentWeight;
        }
        if (FindNode(adjacent,explored) || FindNode(adjacent, reachable))
        {
            return;
        }
        if (adjacent.navigability == Node.navigabilityStates.navigable || adjacent.navigability == Node.navigabilityStates.destructable)
        {
            reachable.Add(adjacent);
        }
    }
    private bool FindNode(Node node, List<Node> list)
    {
        return GetNodeIndex(node, list) >= 0;
    }
    private int GetNodeIndex(Node node, List<Node> list)
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
    private Node ChoseNode()
    {
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
                }
                if (_node.x < _currentNode.x && goalNode.x < _currentNode.x)//IF the goal is left and the searching node is left, add it twice.
                {
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
    private Node FindLowestWeight(Node _node)
    {
        int _lowestWeight = int.MaxValue;
        List<Node> _possibleOptions = new List<Node>();

        foreach (Node _adjNode in _node.adjecant)
        {
            if (_adjNode == startNode)
            {
                return _adjNode;
            }
        }

        foreach (Node _adjNode in _node.adjecant)
        {
            if(_adjNode.weighting < _lowestWeight && _adjNode.weighting != 0)
            {
                _possibleOptions.Add(_adjNode);
            }
        }

        if (_possibleOptions.Count > 1)
        {
            _lowestWeight = int.MaxValue;
            //For each node option
            int[] _possibleOptionsArray = new int[_possibleOptions.Count];
            for (int i = 0; i < _possibleOptions.Count; i++)
            {
                _possibleOptionsArray[i] = int.MaxValue;
                 //Check all of that nodes adjecent nodes
                for (int j = 0; j < _possibleOptions[i].adjecant.Count; j++)
                {
                    //Find the lowest weighting of all of the adjecent nodes.
                    if(_possibleOptions[i].adjecant[j].weighting < _lowestWeight)
                    {
                        if(_possibleOptions[i].adjecant[j] != _node)
                        {
                            _possibleOptionsArray[i] = _possibleOptions[i].adjecant[j].weighting;
                            _lowestWeight = _possibleOptions[i].adjecant[j].weighting;
                        }
                    }
                }
            }
            //Whichever has the lowest weighted adjecent, move onto that chosen one.
            _lowestWeight = int.MaxValue;
            int _lowestIndex = -1;
            for (int i = 0; i < _possibleOptions.Count; i++)
            {
                if(_possibleOptionsArray[i] < _lowestWeight)
                {
                    _lowestWeight = _possibleOptionsArray[i];
                    _lowestIndex = i;
                }
            }
            return _possibleOptions[_lowestIndex];
        }
        else
        {
            return _possibleOptions[0];
        }



    }

}
