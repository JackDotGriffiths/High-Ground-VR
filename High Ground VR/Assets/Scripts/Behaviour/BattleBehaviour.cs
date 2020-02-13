using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : MonoBehaviour
{
    private bool m_battleStarted = false;
    private float m_battleTimer = 2.0f; //Time between each attack
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
        foreach (EnemyGroupBehaviour _enemy in m_enemyGroups)
        {
            if(_enemy == null)
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

        if (m_battleStarted == true)
        {
            m_currentTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;
            if (m_currentTimer < 0)
            {
                Debug.Log("Attack");
         
                friendlyAttack();
                enemyAttack();
                m_currentTimer = m_battleTimer;
            }
            if (m_enemyUnits.Count == 0)
            {
                //End the battle, Player Won
                battleOver();
                Destroy(this);
            }
            if (m_friendlyUnits.Count == 0)
            {
                //End the battle, Enemy Won
                battleOver();
                Destroy(this);
            }
        }
    }


    void friendlyAttack()
    {
        float _totalDamage = 0;
        foreach(Unit _unit in m_friendlyUnits)
        {
            _totalDamage += _unit.damage;
        }
        float _distributedDamage = _totalDamage / m_enemyUnits.Count;
        foreach (Unit _unit in m_enemyUnits)
        {
            _unit.health -= _distributedDamage;
            if(_unit.health < 0)
            {
                m_enemyUnits.Remove(_unit);
                _unit.unitComp.Die();
            }
        }



    }

    void enemyAttack()
    {
        float _totalDamage = 0;
        foreach (Unit _unit in m_enemyUnits)
        {
            _totalDamage += _unit.damage;
        }
        float _distributedDamage = _totalDamage / m_friendlyUnits.Count;
        foreach (Unit _unit in m_friendlyUnits)
        {
            _unit.health -= _distributedDamage;
            if (_unit.health < 0)
            {
                m_friendlyUnits.Remove(_unit);
                _unit.unitComp.Die();
            }
        }
    }

    /// <summary>
    /// Tell all groups that the unit
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
}
