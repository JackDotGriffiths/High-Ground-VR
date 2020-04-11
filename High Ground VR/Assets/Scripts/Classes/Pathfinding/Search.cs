using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Search
{
    public Node[,] graph = GameBoardGeneration.Instance.Graph; //The entire game board representation as a 2D array of Nodes.
    
    
    public List<Node> path; //The Chosen path after the search has occured
    private List<Node> openNodes;
    private List<Node> closedNodes;
    private int m_straightCost = 10;
    private int m_diagonalCost = 8;



    //public float unitAggression;

    //////////////////////////////////////////////////// NEW IMPLEMENTATION

    public void StartSearch(Node _startNode, Node _endNode)
    {
        path = new List<Node>();
        openNodes = new List<Node>();
        closedNodes = new List<Node>();

        openNodes.Add(_startNode);
        foreach(Node _adjacentNode in _startNode.adjecant)
        {
            openNodes.Add(_adjacentNode);
            _adjacentNode.searchData.parentNode = _startNode;
        }

        openNodes.Remove(_startNode);
        closedNodes.Add(_startNode);
        Node _currentNode;

        while (openNodes.Count > 0)
        {
            //Lowest F cost out of all of the nodes in openNodes.
            float _lowestF = Mathf.Infinity;
            _currentNode = null;
            foreach(Node _node in openNodes)
            {
                if (_node.searchData.F < _lowestF)
                {
                    _currentNode = _node;
                    _lowestF = _node.searchData.F;
                }
            }
            openNodes.Remove(_currentNode);
            if (_currentNode == _endNode)
            {
                break;
            }
            closedNodes.Add(_currentNode);

            foreach (Node _adjacentNode in _currentNode.adjecant)
            {
                if (!closedNodes.Contains(_adjacentNode))
                {
                   if (!openNodes.Contains(_adjacentNode))
                    {
                        openNodes.Add(_adjacentNode);
                        _adjacentNode.searchData.parentNode = _currentNode;
                        _adjacentNode.searchData.G = _adjacentNode.searchData.parentNode.searchData.G + calculateDirectionalCost(_currentNode, _adjacentNode);
                        _adjacentNode.searchData.H = hexagonalHeuristicCost(_adjacentNode, _endNode) * m_straightCost;
                        _adjacentNode.searchData.F = _adjacentNode.searchData.G + _adjacentNode.searchData.H;
                    }
                    else if (openNodes.Contains(_adjacentNode))
                    {
                        //If G cost of adjacent is LOWER than G cost of current
                        if (_adjacentNode.searchData.G < _currentNode.searchData.G + calculateDirectionalCost(_currentNode, _adjacentNode))
                        {
                            _adjacentNode.searchData.parentNode = _currentNode;
                            _adjacentNode.searchData.G = _adjacentNode.searchData.parentNode.searchData.G + calculateDirectionalCost(_currentNode, _adjacentNode);
                            _adjacentNode.searchData.F = _adjacentNode.searchData.G + _adjacentNode.searchData.H;
                        }
                    }
                }
            }

        }


       if(openNodes.Count == 0)
        {
            Debug.Log("Pathfinding Failed");
        }
        else
        {
            //Work out the path now
            _currentNode = _endNode;
            Node _parentNode = _endNode.searchData.parentNode;
            path.Add(_currentNode);
            while (_currentNode != _startNode)
            {
                _parentNode = _currentNode.searchData.parentNode;
                path.Add(_parentNode);
                _currentNode = _parentNode;
            }
            path.Reverse();
        }


    }

    private int calculateDirectionalCost(Node _fromNode, Node _toNode)
    {
        if(_fromNode.y == _toNode.y)
        {
            //Nodes are straight next to eachother.
            return m_straightCost;
        }
        else
        {
            //Nodes are diagonal across from eachother.
            return m_diagonalCost;
        }
    }

    private int hexagonalHeuristicCost(Node _fromNode, Node _endNode)
    {
        int _xDifference = Mathf.Abs(_fromNode.x - _endNode.x);
        int _yDifference = Mathf.Abs(_fromNode.y - _endNode.y);
        int _xyDifference = Mathf.Abs(_xDifference - _yDifference);


        int _max = Mathf.Max(_xDifference, Mathf.Max(_yDifference, _xyDifference));

        return _xDifference + _yDifference;

        //return Mathf.RoundToInt(Vector3.Distance(_fromNode.hex.transform.position, _endNode.hex.transform.position));  //Absolute Distance between two nodes.



    }
}
