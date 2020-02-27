using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeBehaviour : MonoBehaviour
{
    private bool m_siegeStarted = false;
    private float m_siegeTimer = 1f; //Time between each attack
    private float m_currentTimer;


    public List<EnemyGroupBehaviour> enemyGroups;
    public BuildingHealth buildingHealth;

    public List<Unit> enemyUnits;


    /// <summary>
    /// Starts the battle.
    /// </summary>
    /// <param name="_friendlyUnits">List of friendly units to have in the battle.</param>
    /// <param name="_enemyUnits">List of enemy units to have in the battle.</param>
    public void StartSiege(BuildingHealth _building, List<Unit> _enemyUnits)
    {
        enemyGroups = new List<EnemyGroupBehaviour>();
        buildingHealth = _building;

        enemyUnits = _enemyUnits;
        buildingHealth = _building;

        m_currentTimer = m_siegeTimer;
        m_siegeStarted = true;
    }

    /// <summary>
    /// Allows an enemy group to join the battle.
    /// </summary>
    /// <param name="_enemyUnits">List of Units to join the battle </param>
    public void JoinSiege(List<Unit> _enemyUnits)
    {
        //Add incoming enemies into the battle.
        enemyUnits.AddRange(_enemyUnits);
    }
    void Update()
    {
        for (int i = 0; i < enemyGroups.Count; i++)
        {
            if (enemyGroups[i] == null)
            {
                enemyGroups.Remove(enemyGroups[i]);
            }
        }


        if (m_siegeStarted == true)
        {

            drawDebugLines();
            m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;
            if (buildingHealth == null)
            {
                siegeOver();
            }
            if (m_currentTimer < 0)
            {
                enemyAttack();
                m_currentTimer = m_siegeTimer;
            }
        }
    }

    void enemyAttack()
    {
        StartCoroutine("delayEnemyAttackAnim");
        float _totalDamage = 0;
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            _totalDamage += enemyUnits[i].damage;
        }
        buildingHealth.takeDamage(_totalDamage);
    }

    /// <summary>
    /// Tell all groups that the units belong to that the battle is over.
    /// </summary>
    void siegeOver()
    {
        for (int i = 0; i < enemyGroups.Count; i++)
        {
            EnemyGroupBehaviour _enemy = enemyGroups[i];
            _enemy.inSiege = false;
            //Make the enemies slightly less aggressive after they destroy a building.
            _enemy.groupAggression = _enemy.groupAggression * 0.85f;
            _enemy.RunPathfinding(_enemy.groupAggression);
        }
        Destroy(this);
    }


    void drawDebugLines()
    {
        for (int j = 0; j < enemyGroups.Count; j++)
        {
            try
            {
                Debug.DrawLine(buildingHealth.transform.position, enemyGroups[j].transform.position, Color.red);
            }
            catch { }
        }
    }


    /// <summary>
    /// Delay the animations between attacking.
    /// </summary>
    /// <returns></returns>
    IEnumerator delayEnemyAttackAnim()
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            yield return new WaitForSeconds(Random.Range(0, 0.2f));
            //All units may be dead by this point.
            try
            {
                //Rotate towards a random target
                enemyUnits[i].unitComp.transform.LookAt(buildingHealth.transform.position);

                //Run attack animation
                enemyUnits[i].unitComp.gameObject.GetComponent<Animator>().Play("UnitAttack");

                //Play an appropriate sound
                AudioManager.Instance.PlaySound(SoundLists.weaponClashes, true, 1, enemyUnits[i].unitComp.gameObject, true, false, true);
            }
            catch { }
        }
    }
}
