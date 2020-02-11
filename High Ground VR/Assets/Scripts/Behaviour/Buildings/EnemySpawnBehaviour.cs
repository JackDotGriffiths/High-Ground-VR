using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnBehaviour : MonoBehaviour
{
    [SerializeField, Tooltip("Prefab for the enemy unit")] private GameObject m_enemyUnit;

    public Node thisNode; //The current Node.
    public void spawnEnemy()
    {
        Vector3 _spawnPosition = Vector3.zero;
        //Choose a random hex around the edge of the map.
        bool _validHex = false;
        Node _spawnNode = null;
        do
        {
            _spawnNode = thisNode.adjecant[Random.Range(0, thisNode.adjecant.Count - 1)];
            if(_spawnNode.navigability == navigabilityStates.nonPlaceable)
            {
                _validHex = true;
                float _yOffset = GameBoardGeneration.Instance.BuildingValidation.buildingHeightOffset;

                if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.small)
                {
                    _yOffset = GameBoardGeneration.Instance.BuildingValidation.buildingHeightOffset * InputManager.Instance.LargestScale.y + 20;
                }

                _spawnPosition = new Vector3(_spawnNode.hex.transform.position.x, _spawnNode.hex.transform.position.y + _yOffset, _spawnNode.hex.transform.position.z);
            }
        } while (_validHex == false);

        GameObject _enemy = Instantiate(m_enemyUnit, _spawnPosition,Quaternion.identity);
        _enemy.GetComponent<EnemyGroupBehaviour>().currentX = _spawnNode.x;
        _enemy.GetComponent<EnemyGroupBehaviour>().currentY = _spawnNode.y;
        _enemy.GetComponent<EnemyGroupBehaviour>().goalX = GameManager.Instance.GameGemNode.x;
        _enemy.GetComponent<EnemyGroupBehaviour>().goalY = GameManager.Instance.GameGemNode.y;





    }
}
