using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnBehaviour : MonoBehaviour
{
    [SerializeField, Tooltip("Prefab for the enemy unit")] private GameObject m_enemyUnit;

    public Node thisNode; //The current Node.
    public bool spawnEnemy()
    {
        Vector3 _spawnPosition = Vector3.zero;
        //Choose a random hex around the edge of the map.
        int _index = 0;
        Node _spawnNode = null;
        do
        {
            _spawnNode = thisNode.adjecant[Random.Range(0, thisNode.adjecant.Count)];
            if(_spawnNode.navigability == navigabilityStates.nonPlaceable || _spawnNode.navigability == navigabilityStates.navigable)
            {
                _spawnPosition = new Vector3(_spawnNode.hex.transform.position.x, _spawnNode.hex.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, _spawnNode.hex.transform.position.z);
            }
            _index++;
        } while (_index < thisNode.adjecant.Count);

        if(_index == thisNode.adjecant.Count-1)
        {
            return false; //Failed to spawn enemy, try again.
        }
        if(_spawnNode.navigability == navigabilityStates.navigable && _spawnNode.hex.transform.childCount == 0)
        {
            GameObject _enemy = Instantiate(m_enemyUnit, _spawnPosition, Quaternion.identity, _spawnNode.hex.transform);
            _enemy.GetComponent<EnemyGroupBehaviour>().currentX = _spawnNode.x;
            _enemy.GetComponent<EnemyGroupBehaviour>().currentY = _spawnNode.y;
            _enemy.GetComponent<EnemyGroupBehaviour>().goalX = GameManager.Instance.GameGemNode.x;
            _enemy.GetComponent<EnemyGroupBehaviour>().goalY = GameManager.Instance.GameGemNode.y;
            AudioManager.Instance.PlaySound(SoundLists.enemySpawning, false, 0, _enemy, true, false, true);
            return true; //Succeeded spawning an enemy, continuing spawning.
        }
        else
        {
            return false;
        }


    }
}
