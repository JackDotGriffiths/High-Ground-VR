using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : MonoBehaviour
{
    private bool m_battleStarted = false;
    private float m_battleTimer = 1.5f; //Time between each attack
    private float m_currentTimer;


    public List<EnemyGroupBehaviour> m_enemyGroups;
    public List<BarracksBehaviour> m_friendlyGroups;

    public List<Unit> m_enemyUnits;
    public List<Unit> m_friendlyUnits;


    /// <summary>
    /// Starts the battle.
    /// </summary>
    /// <param name="_friendlyUnits">List of friendly units to have in the battle.</param>
    /// <param name="_enemyUnits">List of enemy units to have in the battle.</param>
    public void StartBattle(List<Unit> _friendlyUnits, List<Unit> _enemyUnits)
    {
        m_enemyGroups = new List<EnemyGroupBehaviour>();
        m_friendlyGroups = new List<BarracksBehaviour>();

        m_friendlyUnits = _friendlyUnits;
        m_enemyUnits = _enemyUnits;
        m_currentTimer = m_battleTimer;
        m_battleStarted = true;
    }

    /// <summary>
    /// Allows an enemy group to join the battle.
    /// </summary>
    /// <param name="_enemyUnits">List of Units to join the battle </param>
    public void JoinBattle(List<Unit> _enemyUnits)
    {
        //Add incoming enemies into the battle.
        m_enemyUnits.AddRange(_enemyUnits);
    }
    void Update()
    {
        if (m_battleStarted == true)
        {

            drawDebugLines();
            m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;
            if (m_enemyUnits.Count == 0 || m_friendlyUnits.Count == 0)
            {
                //End the battle, Player Won
                battleOver();
                Destroy(this);
            }
            if (m_currentTimer < 0)
            {
                Debug.Log("Attack");

                friendlyAttack();
                enemyAttack();
                m_currentTimer = m_battleTimer;
            }
        }

        foreach (EnemyGroupBehaviour _enemy in m_enemyGroups)
        {
            if (_enemy == null)
            {
                m_enemyGroups.Remove(_enemy);
            }
        }

        foreach (BarracksBehaviour _friendly in m_friendlyGroups)
        {
            if (_friendly == null)
            {
                m_friendlyGroups.Remove(_friendly);
            }
        }
    }


    void friendlyAttack()
    {
        StartCoroutine("delayFriendlyAttackAnim");
        float _totalDamage = 0;

        for (int i = 0; i < m_friendlyUnits.Count; i++)
        {
            _totalDamage += m_friendlyUnits[i].damage;
        }
        float _distributedDamage = _totalDamage / m_enemyUnits.Count;
        for (int i = 0; i < m_enemyUnits.Count; i++)
        {

            m_enemyUnits[i].health -= _distributedDamage;
            if (m_enemyUnits[i].health < 0)
            {
                m_enemyUnits[i].unitComp.Die();
                m_enemyUnits.Remove(m_enemyUnits[i]);
            }
        }




    }

    void enemyAttack()
    {
        StartCoroutine("delayEnemyAttackAnim");
        float _totalDamage = 0;
        for (int i = 0; i < m_enemyUnits.Count; i++)
        {
            _totalDamage += m_enemyUnits[i].damage;
        }
        float _distributedDamage = _totalDamage / m_friendlyUnits.Count;
        for (int i = 0; i < m_friendlyUnits.Count; i++)
        {

            m_friendlyUnits[i].health -= _distributedDamage;
            if (m_friendlyUnits[i].health < 0)
            {
                m_friendlyUnits[i].unitComp.Die();
                m_friendlyUnits.Remove(m_friendlyUnits[i]);
            }
        }
    }

    /// <summary>
    /// Tell all groups that the units belong to that the battle is over.
    /// </summary>
    void battleOver()
    {
        foreach (EnemyGroupBehaviour _enemy in m_enemyGroups)
        {
            _enemy.inCombat = false;
        }
        foreach (BarracksBehaviour _friendy in m_friendlyGroups)
        {
            _friendy.inCombat = false;
        }
    }


    void drawDebugLines()
    {
        foreach (BarracksBehaviour _friendly in m_friendlyGroups)
        {
            foreach(EnemyGroupBehaviour _enemy in m_enemyGroups)
            {
                Debug.DrawLine(_friendly.transform.position, _enemy.transform.position, Color.red);
            }
        }
    }

    /// <summary>
    /// Delay the animations between attacking.
    /// </summary>
    /// <returns></returns>
    IEnumerator delayFriendlyAttackAnim()
    {
        for (int i = 0; i < m_friendlyUnits.Count; i++)
        {
            yield return new WaitForSeconds(Random.Range(0, 0.4f));
            //All units may be dead by this point.
            try
            {
                //Rotate towards a random target
                Vector3 _randomTarget = m_enemyUnits[Random.Range(0, m_enemyUnits.Count)].unitComp.transform.position;
                m_friendlyUnits[i].unitComp.transform.LookAt(_randomTarget);

                //Run attack animation
                m_friendlyUnits[i].unitComp.gameObject.GetComponent<Animator>().Play("UnitAttack");
                //Play an appropriate sound
                AudioManager.Instance.PlaySound(SoundLists.weaponClashes, true, 1, m_friendlyUnits[i].unitComp.gameObject, true, false, true);
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
        for (int i = 0; i < m_enemyUnits.Count; i++)
        {
            yield return new WaitForSeconds(Random.Range(0, 0.4f));
            //All units may be dead by this point.
            try
            {
                //Rotate towards a random target
                Vector3 _randomTarget = m_friendlyUnits[Random.Range(0, m_friendlyUnits.Count)].unitComp.transform.position;
                m_enemyUnits[i].unitComp.transform.LookAt(_randomTarget);

                //Run attack animation
                m_enemyUnits[i].unitComp.gameObject.GetComponent<Animator>().Play("UnitAttack");

                //Play an appropriate sound
                AudioManager.Instance.PlaySound(SoundLists.weaponClashes, true, 1, m_enemyUnits[i].unitComp.gameObject, true, false, true);
            }
            catch { }
        }
    }
}
