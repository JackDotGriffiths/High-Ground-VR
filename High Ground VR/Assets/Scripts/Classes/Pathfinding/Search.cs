using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SearchTypes {Aggressive,Passive};
public class Search
{
    public Node[,] graph = GameBoardGeneration.Instance.Graph; //The entire game board representation as a 2D array of Nodes.
    
    
    public List<Node> path; //The Chosen path after the search has occured
    private List<Node> openNodes;
    private List<Node> closedNodes;
    private int m_straightCost = 8;
    private int m_diagonalCost = 8;

    private int m_enemyInSiegeCost = 10; // This encourages enemies to go around those in siege. If a new route is open, they'll all notice it as it appears.
    private int m_enemyInCombatCost = 10; //An enemy should try and avoid enemies in combat, possibly creating paths around them if possible.
    private int m_enemyUnitCost = 5; // Cost of an enemy being in the way. Promotes moving around them if possible. 
    private int m_adjacentToPlayerUnitCost = 10; //This is also definite combat, so should try and be avoided by the pathfinding.
    private int m_playerUnitCost = 15; // The cost of navigating through a player. This means definite combat so the pathfinding may try and avoid that.
    private int m_destructableBuildingCost = 1; // This is low because the only time it's used is by the tank, which needs to be destructive




    //public float unitAggression;

    //////////////////////////////////////////////////// NEW IMPLEMENTATION

    public void StartSearch(Node _startNode, Node _endNode, SearchTypes _searchType)
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
                if (_node.searchData.F < _lowestF && isNavigable(_node,_searchType))
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
                if (!closedNodes.Contains(_adjacentNode) && isNavigable(_adjacentNode, _searchType))
                {
                   if (!openNodes.Contains(_adjacentNode))
                    {
                        openNodes.Add(_adjacentNode);
                        _adjacentNode.searchData.parentNode = _currentNode;
                        _adjacentNode.searchData.G = _adjacentNode.searchData.parentNode.searchData.G + calculateDirectionalCost(_currentNode, _adjacentNode) + nodeCost(_adjacentNode);
                        _adjacentNode.searchData.H = hexagonalHeuristicCost(_adjacentNode, _endNode) * m_straightCost;
                        _adjacentNode.searchData.F = _adjacentNode.searchData.G + _adjacentNode.searchData.H;
                    }
                    else if (openNodes.Contains(_adjacentNode))
                    {
                        //If G cost of adjacent is LOWER than G cost of current
                        if (_adjacentNode.searchData.G < _currentNode.searchData.G + calculateDirectionalCost(_currentNode, _adjacentNode))
                        {
                            _adjacentNode.searchData.parentNode = _currentNode;
                            _adjacentNode.searchData.G = _adjacentNode.searchData.parentNode.searchData.G + calculateDirectionalCost(_currentNode, _adjacentNode) + nodeCost(_adjacentNode); ;
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

    private bool isNavigable(Node _targetNode, SearchTypes _searchType)
    {
        if(_targetNode.navigability == nodeTypes.barracks || _targetNode.navigability == nodeTypes.enemySpawn)
        {
            return false;
        }
        else if (_targetNode.navigability == nodeTypes.navigable || _targetNode.navigability == nodeTypes.gem || _targetNode.navigability == nodeTypes.enemyUnit || _targetNode.navigability == nodeTypes.playerUnit)
        {
            return true;
        }
        else if (_searchType== SearchTypes.Aggressive)
        {
            if(_targetNode.navigability == nodeTypes.mine || _targetNode.navigability == nodeTypes.wall || _targetNode.navigability == nodeTypes.navigable || _targetNode.navigability == nodeTypes.gem || _targetNode.navigability == nodeTypes.enemyUnit)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns a cost for the a node. This is based on it's Type.
    /// </summary>
    /// <param name="_targetNode"></param>
    /// <returns></returns>
    private int nodeCost(Node _targetNode)
    {
        switch (_targetNode.navigability)
        {
            case nodeTypes.enemyUnit:
                if (inCombat(_targetNode))
                {
                    return m_enemyInCombatCost;
                }
                else if (inSiege(_targetNode))
                {
                    return m_enemyInSiegeCost;
                }
                else
                {
                    return m_enemyUnitCost;
                }
            case nodeTypes.playerUnit:
                return m_playerUnitCost;
            case nodeTypes.mine:
                return m_destructableBuildingCost;
            case nodeTypes.wall:
                return m_destructableBuildingCost;
        }
        foreach(Node _adjNode in _targetNode.adjecant)
        {
            if(_adjNode.navigability == nodeTypes.playerUnit)
            {
                return m_adjacentToPlayerUnitCost;
            }
        }
        return 0; //0 Cost for empty navigable areas.
    }

    /// <summary>
    /// Returns whether a node is in Combat currently.
    /// </summary>
    /// <param name="_targetNode"></param>
    /// <returns></returns>
    private bool inCombat(Node _targetNode)
    {
        if(_targetNode.hex.transform.GetChild(0).TryGetComponent(out EnemyBehaviour _enemyBehaviour)) //Get the enemyBehaviour of the target Hex
        {
            if(_enemyBehaviour.inCombat == true)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Returns whether a node is in Siege currently.
    /// </summary>
    /// <param name="_targetNode"></param>
    /// <returns></returns>
    private bool inSiege(Node _targetNode)
    {
        if (_targetNode.hex.transform.GetChild(0).TryGetComponent(out EnemyBehaviour _enemyBehaviour)) //Get the enemyBehaviour of the target Hex
        {
            if (_enemyBehaviour.inSiege == true)
            {
                return true;
            }
        }
        return false;
    }
}
