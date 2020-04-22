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
            //Run pathfinding
            _enemy.RunPathfinding( _enemy.groupAggression);
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
            try
            {
                //Rotate towards a random target
                UnitComponent _unitComp = enemyUnits[i].unitComp;
                _unitComp.transform.LookAt(buildingHealth.transform.position);

                //Run attack animation
                enemyUnits[i].unitComp.gameObject.GetComponent<Animator>().Play("UnitAttack");
                //Play an appropriate sound
                //AudioManager.Instance.Play3DSound(SoundLists.weaponClashes, true, 1, enemyUnits[i].unitComp.gameObject, true, false, true);
                AudioManager.Instance.PlaySound("weaponClash" + Random.Range(1, 2), AudioLists.Combat, AudioMixers.Effects, true, true, false, enemyUnits[i].unitComp.gameObject, 0.5f);
            }
            catch { }
            yield return new WaitForSeconds(Random.Range(0, 0.05f));
            buildingHealth.takeDamage(enemyUnits[i].damage);
            yield return new WaitForSeconds(Random.Range(0, 0.02f));

        }
    }
}
