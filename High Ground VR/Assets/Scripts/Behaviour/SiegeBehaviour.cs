using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeBehaviour : MonoBehaviour
{
    private bool m_siegeStarted = false;
    private float m_siegeTimer = 3f; //Time between each attack
    private float m_currentTimer;
    private float m_timePerception;

    public List<EnemyBehaviour> enemyGroups;
    public BuildingHealth buildingHealth;

    public List<Unit> enemyUnits;


    /// <summary>
    /// Starts the battle.
    /// </summary>
    /// <param name="_friendlyUnits">List of friendly units to have in the battle.</param>
    /// <param name="_enemyUnits">List of enemy units to have in the battle.</param>
    public void StartSiege(BuildingHealth _building, List<Unit> _enemyUnits)
    {
        enemyGroups = new List<EnemyBehaviour>();
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
            float _totalTimePerception = 0;
            for (int i = 0; i < enemyGroups.Count; i++)
            {
                _totalTimePerception += enemyGroups[i].timePerception;
            }
            m_timePerception = _totalTimePerception / enemyGroups.Count;
            drawDebugLines();
            m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed * m_timePerception;
            if (buildingHealth == null)
            {
                siegeOver();
            }
            if (m_currentTimer < 0)
            {
                m_currentTimer = m_siegeTimer;
                StartCoroutine("enemyAttack");
            }
        }
    }



    /// <summary>
    /// Tell all groups that the units belong to that the battle is over.
    /// </summary>
    void siegeOver()
    {
        for (int i = 0; i < enemyGroups.Count; i++)
        {
            EnemyBehaviour _enemy = enemyGroups[i];
            _enemy.inSiege = false;
            _enemy.currentStepIndex = 0;
            //Run pathfinding, randomly choosing how the unit navigates based on their aggression and some random factors.
            float _aggressionChance = 1.0f - (GameManager.Instance.aggressionPercentage * (GameManager.Instance.RoundCounter / 2.0f));
            if (_enemy.groupAggression > _aggressionChance)
            {
                float _rand = Random.Range(0.0f, 1.0f);
                if (_rand < 0.3f)
                {
                    _enemy.RunPathfinding(enemyTargets.randomDestructableBuilding, _enemy.groupAggression);
                }
                else if (_rand > 0.3f && _rand < 0.5f)
                {
                    _enemy.RunPathfinding(enemyTargets.randomMine, _enemy.groupAggression);
                }
                else
                {
                    _enemy.RunPathfinding(enemyTargets.Gem, _enemy.groupAggression);
                }
            }
            else
            {
                _enemy.RunPathfinding(enemyTargets.Gem, _enemy.groupAggression);
            }




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
    IEnumerator enemyAttack()
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            //Rotate towards a random target
            UnitComponent _unitComp = enemyUnits[i].unitComp;
            _unitComp.transform.LookAt(buildingHealth.transform.position);

            //Run attack animation
            enemyUnits[i].unitComp.gameObject.GetComponent<Animator>().Play("UnitAttack");
            //Play an appropriate sound
            AudioManager.Instance.Play3DSound(SoundLists.weaponClashes, true, 1, enemyUnits[i].unitComp.gameObject, true, false, true);
            yield return new WaitForSeconds(Random.Range(0, 0.5f));
            buildingHealth.takeDamage(enemyUnits[i].damage);
            yield return new WaitForSeconds(Random.Range(0, 0.2f));
        }
        yield return null;
    }
}
