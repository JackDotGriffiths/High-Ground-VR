using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search
{
    public Node[,] graph; //Graph of all nodes
    public List<Node> explored; //Complete list of explored nodes within the search
    public List<Node> path; //The Chosen path after the search has occured



    public float unitAggression;


    //////////////////////////////////////////////////// NEW IMPLEMENTATION
    
    private float heuristicSearch(Node a,Node b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public void StartSearch(Node _start, Node _goal)
    {
        path = new List<Node>();
        explored = new List<Node>();




        Node[,] _graph = GameBoardGeneration.Instance.Graph; //Get the graph from GameBoard Generation
        path.Add(_start); //Add the first position.
        explored.Add(_start); 
        int _costSoFar = 0;
        while(path.Count > 0)
        {
            Node _currentNode = path[path.Count-1];

            if(_currentNode == _goal)
            {
                break;
            }

            foreach (Node _next in _currentNode.adjecant)
            {
                int _newCost = _costSoFar + 2; //CURRENTLY, EACH NODE IS WORTH 2. THIS CAN BE ALTERED/UPDATED.
                if (!explored.Contains(_next) || _newCost < _costSoFar)
                {
                    _costSoFar = _newCost;
                    float priority = _newCost + heuristicSearch(_next, _goal);
                    path.Add(_next);
                }
            }
            explored.Add(_currentNode);
        }


    }




}