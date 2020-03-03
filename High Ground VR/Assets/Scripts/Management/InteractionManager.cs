using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField, Tooltip("Time before being able to attack again")] private float m_attackCooldown;

    [Header("Regular Attack Effects"),Space(10)]
    [SerializeField, Tooltip("Damage that should be dealt when player is large.This is PER enemy.")] private float m_playerLargeDamage;
    //Need to make damage work when player is small.


    [SerializeField, Tooltip("Number of LineRenderers to make up Lightning"),Space(3)] private int m_lightningStrikes;
    [SerializeField, Tooltip("Number of points in the Lightning that change angle.")] private int m_lightningStrikeBreakPoints;
    [SerializeField, Tooltip("Width in the Lightning effect. Size of the unit sphere used for random placement.")] private float m_lightningWidth;
    [SerializeField, Tooltip("Time between chaning the lightning position.")] private float m_lightningTiming;
    [SerializeField, Tooltip("Number of times the lightning changes position.")] private float m_lightningOccurance;
    [SerializeField, Tooltip("The material to use for the regularAttackEffect.")] private Material m_regularAttackMaterial;

    private List<Vector3> m_breakPoints;
    private bool m_currentlyAttacking; //Tracks whether an effect is currently happening.

    void Start()
    {
        m_breakPoints = new List<Vector3>();
    }
    public void regularAttack(GameObject _controller)
    {
        if (m_currentlyAttacking == true)
        {
            return;
        }

        //This currently only works if the player is large.
        RaycastHit _hit;
        if (Physics.Raycast(_controller.transform.position, _controller.transform.forward - _controller.transform.up, out _hit, 1000, 1 << LayerMask.NameToLayer("Environment")))
        {
            StartCoroutine(regularAttackEffect(_controller, _hit.point));


            //If it's a unit;
            if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.large && _hit.collider.gameObject.GetComponent<NodeComponent>().node.navigability == navigabilityStates.enemyUnit)
            {
                EnemyGroupBehaviour _enemies = _hit.collider.gameObject.GetComponentInChildren<EnemyGroupBehaviour>();
                List<Unit> _enemyUnits = new List<Unit>();
                foreach(GameObject _go in _enemies.m_units)
                {
                    _enemyUnits.Add(_go.GetComponent<UnitComponent>().unit);
                }
                foreach(Unit _unit in _enemyUnits)
                {
                    _unit.health -= m_playerLargeDamage;
                    if(_unit.health < 0)
                    {
                        _unit.unitComp.Die();
                    }
                }
            }
        }
        else
        {
            //Miss 
        }
    }

    IEnumerator regularAttackEffect(GameObject _controller, Vector3 _hitPos)
    {
        m_currentlyAttacking = true;
        AudioManager.Instance.Play2DSound(SoundLists.zapSounds, false, 0, true, false, true);
        GameObject _goLine = new GameObject("regularAttackEffect");
        StartCoroutine(destroyEffect(_goLine));
        _goLine.transform.parent = _controller.transform;

        List<LineRenderer> _lightningLines = new List<LineRenderer>();

        for (int i = 0; i < m_lightningStrikes; i++)
        {
            GameObject _go = new GameObject("regularAttackEffect");
            Destroy(_go, m_lightningTiming * m_lightningOccurance);
            _lightningLines.Add(new GameObject("regularAttackEffect").AddComponent<LineRenderer>());
            _lightningLines[i].material = m_regularAttackMaterial;
            _lightningLines[i].startWidth = 0.1f;
            _lightningLines[i].endWidth = 0.1f;
        }

        ///Break the line down into positions
        float _lineDistance = Vector3.Distance(_controller.transform.position, _hitPos);
        float _breakDistance = _lineDistance / m_lightningStrikeBreakPoints;
        Vector3 _direction = _hitPos - _controller.transform.position;
        _direction.Normalize();
        List<Vector3> _breakPositions = new List<Vector3>();
        m_breakPoints = new List<Vector3>();

        for (int i = 0; i < m_lightningStrikeBreakPoints; i++)
        {
            _breakPositions.Add(_controller.transform.position + _direction * (i * _breakDistance));
            m_breakPoints.Add(_breakPositions[i]);
        }


        for (int i = 0; i < m_lightningOccurance; i++)
        {
            foreach (LineRenderer _line in _lightningLines)
            {
                _line.positionCount = _breakPositions.Count;
                Vector3[] _linePositions = new Vector3[_breakPositions.Count];
                _linePositions[0] = _controller.transform.position;
                for (int j = 1; j < _breakPositions.Count; j++)
                {
                    _linePositions[j] = _breakPositions[j] + Random.insideUnitSphere * m_lightningWidth;
                }
                _linePositions[_breakPositions.Count-1] = _hitPos;
                _line.SetPositions(_linePositions);
            }
            yield return new WaitForSeconds(m_lightningTiming);
        }
        //Clear all of the lightning

        foreach (LineRenderer _line in _lightningLines)
        {
            Destroy(_line.gameObject);
        }

        yield return new WaitForSeconds(m_attackCooldown);
        m_currentlyAttacking = false;
    }



    IEnumerator destroyEffect(GameObject _effect)
    {
        yield return new WaitForSeconds(m_lightningTiming * m_lightningOccurance);
        Destroy(_effect);
    }
}
