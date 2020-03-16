using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleBehaviour : MonoBehaviour
{
    private bool m_battleStarted = false;
    private float m_battleTimer = 2f; //Time between each attack
    private float m_currentFriendlyTimer;
    private float m_currentEnemyTimer;
    private float m_enemyTimePerception;


    public List<EnemyBehaviour> enemyGroups;
    public List<BarracksBehaviour> friendlyGroups;

    public List<Unit> enemyUnits;
    public List<Unit> friendlyUnits;


    /// <summary>
    /// Starts the battle.
    /// </summary>
    /// <param name="_friendlyUnits">List of friendly units to have in the battle.</param>
    /// <param name="_enemyUnits">List of enemy units to have in the battle.</param>
    public void StartBattle(List<Unit> _friendlyUnits, List<Unit> _enemyUnits)
    {
        enemyGroups = new List<EnemyBehaviour>();
        friendlyGroups = new List<BarracksBehaviour>();

        friendlyUnits = _friendlyUnits;
        enemyUnits = _enemyUnits;
        m_currentFriendlyTimer = m_battleTimer;
        m_currentEnemyTimer = m_battleTimer;
        m_battleStarted = true;
    }

    /// <summary>
    /// Allows an enemy group to join the battle.
    /// </summary>
    /// <param name="_enemyUnits">List of Units to join the battle </param>
    public void JoinBattle(List<Unit> _enemyUnits)
    {
        //Add incoming enemies into the battle.
        enemyUnits.AddRange(_enemyUnits);
    }

    void Update()
    {
        //Clear lists of units that may have been destroyed by the player.
        for (int i = 0; i < enemyGroups.Count; i++)
        {
            if (enemyGroups[i] == null)
            {
                enemyGroups.Remove(enemyGroups[i]);
            }
        }

        for (int i = 0; i < friendlyGroups.Count; i++)
        {
            if (friendlyGroups[i] == null)
            {
                friendlyGroups.Remove(friendlyGroups[i]);
            }
        }

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            if (enemyUnits[i] == null)
            {
                enemyUnits.Remove(enemyUnits[i]);
            }
        }

        for (int i = 0; i < friendlyUnits.Count; i++)
        {
            if (friendlyUnits[i] == null)
            {
                friendlyUnits.Remove(friendlyUnits[i]);
            }
        }





        if (m_battleStarted == true)
        {

            float _totalTimePerception = 0;
            for (int i = 0; i < enemyGroups.Count; i++)
            {
                _totalTimePerception += enemyGroups[i].timePerception;
            }
            m_enemyTimePerception = _totalTimePerception / enemyGroups.Count;

            drawDebugLines();
            m_currentFriendlyTimer -= Time.deltaTime * GameManager.Instance.GameSpeed;
            m_currentEnemyTimer -= Time.deltaTime * GameManager.Instance.GameSpeed * m_enemyTimePerception;


            if (enemyUnits.Count == 0)
            {
                //End the battle, Player Won
                battleOver();
                StartCoroutine(playFriendlyWinningSounds());

            }
            if(friendlyUnits.Count == 0)
            {
                //End the battle, Player Won
                battleOver();
                StartCoroutine(playEnemyWinningSounds());
            }




            if (m_currentFriendlyTimer < 0)
            {
                StartCoroutine("friendlyAttack");
                m_currentFriendlyTimer = m_battleTimer;
            }
            if(m_currentEnemyTimer < 0)
            {
                StartCoroutine("enemyAttack");
                m_currentEnemyTimer = m_battleTimer;
            }
        }
    }

    /// <summary>
    /// Tell all groups that the units belong to that the battle is over.
    /// </summary>
    void battleOver()
    {

        for (int i = 0; i < enemyGroups.Count; i++)
        {
            EnemyBehaviour _enemy = enemyGroups[i];
            _enemy.inCombat = false;
        }

        for (int i = 0; i < friendlyGroups.Count; i++)
        {
            friendlyGroups[i].inCombat = false;
        }
    }


    void drawDebugLines()
    {
        for (int i = 0; i < friendlyGroups.Count; i++)
        {
            BarracksBehaviour _friendly = friendlyGroups[i];
            for (int j = 0; j < enemyGroups.Count; j++)
            {
                Debug.DrawLine(_friendly.transform.position, enemyGroups[j].transform.position, Color.red);
            }


        }
    }

    /// <summary>
    /// Delay the animations between attacking.
    /// </summary>
    /// <returns></returns>
    IEnumerator friendlyAttack()
    {
        for (int i = 0; i < friendlyUnits.Count; i++)
        {
            yield return new WaitForSeconds(Random.Range(0, 0.2f));
            //All units may be dead by this point.
            try
            {
                //Rotate towards a random target
                Vector3 _randomTarget = enemyUnits[Random.Range(0, enemyUnits.Count)].unitComp.transform.position;
                friendlyUnits[i].unitComp.transform.LookAt(_randomTarget);

                //Run attack animation
                friendlyUnits[i].unitComp.gameObject.GetComponent<Animator>().Play("UnitAttack");
                //Play an appropriate sound
                //AudioManager.Instance.Play3DSound(SoundLists.weaponClashes, true, 1, friendlyUnits[i].unitComp.gameObject, true, false, true);
                AudioManager.Instance.PlaySound("weaponClash" + Random.Range(1, 2), AudioLists.Combat, AudioMixers.Effects, true, true, false, friendlyUnits[i].unitComp.gameObject, 0.2f);
            }
            catch { }
        }

        yield return new WaitForSeconds(0.5f);
        float _totalDamage = 0;

        for (int i = 0; i < friendlyUnits.Count; i++)
        {
            _totalDamage += friendlyUnits[i].damage;
        }
        float _distributedDamage = _totalDamage / enemyUnits.Count;
        for (int i = 0; i < enemyUnits.Count; i++)
        {

            enemyUnits[i].health -= _distributedDamage;
            if (enemyUnits[i].health < 0 && enemyUnits[i].unitComp != null)
            {
                enemyUnits[i].unitComp.Die();
                enemyUnits.Remove(enemyUnits[i]);
            }
        }
        yield return null;
    }

    /// <summary>
    /// Delay the animations between attacking.
    /// </summary>
    /// <returns></returns>
    IEnumerator enemyAttack()
    {
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            //All units may be dead by this point.
            try
            {
                //Rotate towards a random target
                Vector3 _randomTarget = friendlyUnits[Random.Range(0, friendlyUnits.Count)].unitComp.transform.position;
                enemyUnits[i].unitComp.transform.LookAt(_randomTarget);

                //Run attack animation
                enemyUnits[i].unitComp.gameObject.GetComponent<Animator>().Play("UnitAttack");

                //Play an appropriate sound
                //AudioManager.Instance.Play3DSound(SoundLists.weaponClashes, true, 1, enemyUnits[i].unitComp.gameObject, true, false, true);
                AudioManager.Instance.PlaySound("weaponClash" + Random.Range(1, 2), AudioLists.Combat, AudioMixers.Effects, true, true, false, enemyUnits[i].unitComp.gameObject, 0.2f);
            }
            catch { }
            yield return new WaitForSeconds(Random.Range(0, 0.2f));
        }

        yield return new WaitForSeconds(1.0f);
        float _totalDamage = 0;
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            _totalDamage += enemyUnits[i].damage;
        }
        float _distributedDamage = _totalDamage / friendlyUnits.Count;
        for (int i = 0; i < friendlyUnits.Count; i++)
        {

            friendlyUnits[i].health -= _distributedDamage;
            if (friendlyUnits[i].health < 0 && friendlyUnits[i].unitComp != null)
            {
                friendlyUnits[i].unitComp.Die();
                friendlyUnits.Remove(friendlyUnits[i]);
            }
        }
        yield return null;

    }



    /// <summary>
    /// Plays sounds for each of the friendly units once they've won a battle.
    /// </summary>
    /// <returns></returns>
    IEnumerator playFriendlyWinningSounds()
    {
        //Stop timers
        m_battleStarted = false;
        for (int i = 0; i < friendlyUnits.Count; i++)
        {
            AudioManager.Instance.PlaySound("battleCheer" + Random.Range(1, 5), AudioLists.Combat, AudioMixers.Effects, true, true, false, friendlyUnits[i].unitComp.gameObject, 0.2f);
            yield return new WaitForSeconds(0.06f);
        }
        yield return null;
        Destroy(this);
    }

    /// <summary>
    /// Plays sounds for each of the enemy units once they've won a battle.
    /// </summary>
    /// <returns></returns>
    IEnumerator playEnemyWinningSounds()
    {
        //Stop timers
        m_battleStarted = false;
        for (int i = 0; i < enemyUnits.Count; i++)
        {
            AudioManager.Instance.PlaySound("battleCheer" + Random.Range(1, 5), AudioLists.Combat, AudioMixers.Effects, true, true, false, enemyUnits[i].unitComp.gameObject, 0.2f);
            yield return new WaitForSeconds(0.06f);
        }
        yield return null;
        Destroy(this);
    }



}
