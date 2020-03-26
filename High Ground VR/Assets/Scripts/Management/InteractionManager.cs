using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum spellTypes { none, regularAttack, speedUpUnit, slowDownUnit};

public class InteractionManager : MonoBehaviour
{
    [SerializeField, Tooltip("Time before being able to attack again")] private float m_attackCooldown;
    [SerializeField, Tooltip("Lightning Hit Effect")] private GameObject m_lightningHitEffect;

    [Header("Lightning Effect Config"), Space(10)]
    [SerializeField, Tooltip("Number of LineRenderers to make up Lightning"), Space(3)] private int m_lightningStrikes;
    [SerializeField, Tooltip("Number of points in the Lightning that change angle.")] private int m_lightningStrikeBreakPoints;
    [SerializeField, Tooltip("Width in the Lightning effect. Size of the unit sphere used for random placement.")] private float m_lightningWidth;
    [SerializeField, Tooltip("Time between chaning the lightning position.")] private float m_lightningTiming;
    [SerializeField, Tooltip("Number of times the lightning changes position.")] private float m_lightningOccurance;
    [SerializeField, Tooltip("The material to use for the regularAttackEffect.")] private Material m_lightningMaterial;

    [Header("Regular Attack Effects"), Space(10)]
    [SerializeField, Tooltip("Damage that should be dealt when player is large.This is PER enemy.")] private float m_playerLargeDamage;
    [SerializeField, Tooltip("Damage that should be dealt when player is small.This is PER enemy.")] private float m_playerSmallDamage;


    [Header("Slow Down Attack Effects"), Space(10)]
    [SerializeField, Tooltip("Speed to slow the enemy down to."),Range(0.001f,0.999f)] private float m_slowDownTargetTime = 0.5f;
    [SerializeField, Tooltip("How long the effect should last.")] private float m_slowDownDuration = 3;

    [Header("Speed Up Attack Effects"), Space(10)]
    [SerializeField, Tooltip("Speed to speed the enemy up to."), Range(1.001f, 1.999f)] private float m_speedUpTargetTime = 1.5f;
    [SerializeField, Tooltip("How long the effect should last.")] private float m_speedUpDuration = 3;



    private List<Vector3> m_breakPoints;
    private bool m_currentlyAttacking; //Tracks whether an effect is currently happening.

    void Start()
    {
        m_breakPoints = new List<Vector3>();
    }

    /// <summary>
    /// cast a spell.
    /// </summary>
    /// <param name="_spell">Spell to use from the spellTypes enum.</param>
    public void castSpell(spellTypes _spell)
    {
        switch (_spell)
        {
            case spellTypes.regularAttack:
                regularAttack(InputManager.Instance.MainController);
                break;
            case spellTypes.speedUpUnit:
                speedUpAttack(InputManager.Instance.MainController);
                break;
            case spellTypes.slowDownUnit:
                slowDownAttack(InputManager.Instance.MainController);
                break;
        }

    }


    #region Regular Attack

    /// <summary>
    /// Runs a regular attack. Considers whether the player is large or small whilst raycasting.
    /// </summary>
    /// <param name="_controller">Controller to run the effect from.</param>
    private void regularAttack(GameObject _controller)
    {
        if (m_currentlyAttacking == true)
        {
            return;
        }
        RumbleManager.Instance.heavyVibration(InputManager.Instance.Handedness);
        //Player is large, attack a hex.
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.large)
        {
            RaycastHit _hit;
            if (Physics.Raycast(_controller.transform.position, _controller.transform.forward - _controller.transform.up, out _hit, 1000, 1 << LayerMask.NameToLayer("Environment")))
            {
                StartCoroutine(regularAttackEffect(0.7f, _controller, _hit.point));

                //Show the dust effect on the hex you hit
                Vector3 _effectPos = new Vector3(_hit.collider.gameObject.transform.position.x, _hit.collider.gameObject.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, _hit.collider.gameObject.transform.position.z);
                GameObject _dustEffect = Instantiate(m_lightningHitEffect, _effectPos, Quaternion.Euler(-90, 0, 0));
                Destroy(_dustEffect, 0.5f);

                //If you hit a collider of the environment which has an enemy on it.
                if (_hit.collider.gameObject.GetComponent<NodeComponent>().node.navigability == navigabilityStates.enemyUnit)
                {

                    //Find each unit within that group, and deal damage to them.
                    EnemyBehaviour _enemies = _hit.collider.gameObject.GetComponentInChildren<EnemyBehaviour>();
                    List<Unit> _enemyUnits = new List<Unit>();
                    foreach (GameObject _go in _enemies.m_units)
                    {
                        _enemyUnits.Add(_go.GetComponent<UnitComponent>().unit);
                    }
                    foreach (Unit _unit in _enemyUnits)
                    {
                        _unit.health -= m_playerLargeDamage;
                        if (_unit.health < 0)
                        {
                            _unit.unitComp.Die();
                        }
                    }
                }
                if (_hit.collider.gameObject.GetComponent<NodeComponent>().node.navigability == navigabilityStates.wall || _hit.collider.gameObject.GetComponent<NodeComponent>().node.navigability == navigabilityStates.mine )
                {
                    //Find each unit within that group, and deal damage to them.
                    BuildingHealth _buildingHealth = _hit.collider.gameObject.GetComponentInChildren<BuildingHealth>();
                    _buildingHealth.currentHealth -= 70.0f;
                    if(_buildingHealth.currentHealth < 0)
                    {
                        Destroy(_buildingHealth.gameObject);
                    }
                }



            }
        }
        else
        {
            RaycastHit _hit;
            if (Physics.Raycast(_controller.transform.position, _controller.transform.forward, out _hit, 1000))
            {
                StartCoroutine(regularAttackEffect(0.1f, _controller, _hit.point));
                //If it's a unit;
                if (_hit.collider.gameObject.GetComponentInParent<UnitComponent>() != null)
                {
                    Unit _unit = _hit.collider.gameObject.GetComponentInParent<UnitComponent>().unit;
                    _unit.health -= m_playerSmallDamage;
                    if (_unit.health < 0)
                    {
                        _unit.unitComp.Die();
                    }
                }
            }
            else
            {
                //Miss 
                StartCoroutine(regularAttackEffect(0.2f, _controller, _controller.transform.position + _controller.transform.forward * 20f));
            }
        }

    }

    /// <summary>
    /// Creates the lightning effect seen during the regular attack.
    /// </summary>
    /// <param name="_playerScale"></param>
    /// <param name="_controller"></param>
    /// <param name="_hitPos"></param>
    /// <returns></returns>
    IEnumerator regularAttackEffect(float _playerScale, GameObject _controller, Vector3 _hitPos)
    {
        m_currentlyAttacking = true;
        //AudioManager.Instance.Play2DSound(SoundLists.zapSounds, false, 0, true, false, true);
        AudioManager.Instance.PlaySound("regularLightning", AudioLists.Combat, AudioMixers.Effects, true, true, true, this.gameObject, 0.1f);

        //Create a seperate object to hold the zap effect.
        GameObject _goLine = new GameObject("regularAttackEffect");
        StartCoroutine(destroyEffect(_goLine, m_lightningTiming * m_lightningOccurance));
        _goLine.transform.parent = _controller.transform;

        List<LineRenderer> _lightningLines = new List<LineRenderer>();

        //Create the amount of line renderers required.
        for (int i = 0; i < m_lightningStrikes; i++)
        {
            GameObject _go = new GameObject("regularAttackEffect");
            _go.transform.parent = _goLine.transform;
            Destroy(_go, m_lightningTiming * m_lightningOccurance);
            _lightningLines.Add(new GameObject("regularAttackEffect").AddComponent<LineRenderer>());
            _lightningLines[i].material = m_lightningMaterial;
            _lightningLines[i].startWidth = 0.1f * _playerScale;
            _lightningLines[i].endWidth = 0.1f * _playerScale;
        }

        ///Break the line down into positions, equally sized.
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


        //Set the positions on the line to a random variation of those points. creates the lightning effect. 
        for (int i = 0; i < m_lightningOccurance; i++)
        {
            foreach (LineRenderer _line in _lightningLines)
            {
                _line.positionCount = _breakPositions.Count;
                Vector3[] _linePositions = new Vector3[_breakPositions.Count];
                _linePositions[0] = _controller.transform.position;
                for (int j = 1; j < _breakPositions.Count; j++)
                {
                    _linePositions[j] = _breakPositions[j] + Random.insideUnitSphere * m_lightningWidth * _playerScale;
                }
                _linePositions[_breakPositions.Count - 1] = _hitPos;
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

    #endregion

    #region Slow Down Attack
    /// <summary>
    /// Slows down the units the player is pointing at.
    /// </summary>
    /// <param name="_controller"></param>
    private void slowDownAttack(GameObject _controller)
    {
        if (m_currentlyAttacking == true)
        {
            return;
        }
        RumbleManager.Instance.heavyVibration(InputManager.Instance.Handedness);
        //Player is large, attack a hex.
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.large)
        {
            RaycastHit _hit;
            if (Physics.Raycast(_controller.transform.position, _controller.transform.forward - _controller.transform.up, out _hit, 1000, 1 << LayerMask.NameToLayer("Environment")))
            {

                StartCoroutine(slowDownAttackEffect(0.3f, _controller, _hit.point));

                //Show the dust effect on the hex you hit
                Vector3 _effectPos = new Vector3(_hit.collider.gameObject.transform.position.x, _hit.collider.gameObject.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, _hit.collider.gameObject.transform.position.z);
                GameObject _dustEffect = Instantiate(m_lightningHitEffect, _effectPos, Quaternion.Euler(-90, 0, 0));
                Destroy(_dustEffect, 0.5f);

                //If you hit a collider of the environment which has an enemy on it.
                if (_hit.collider.gameObject.GetComponent<NodeComponent>().node.navigability == navigabilityStates.enemyUnit)
                {
                    //Find each unit within that group, and deal damage to them.
                    EnemyBehaviour _enemies = _hit.collider.gameObject.GetComponentInChildren<EnemyBehaviour>();
                    StartCoroutine(changeTime(_enemies, m_slowDownTargetTime, m_slowDownDuration));
                    
                }
            }
        }
        else
        {
            RaycastHit _hit;
            if (Physics.Raycast(_controller.transform.position, _controller.transform.forward, out _hit, 1000))
            {
                StartCoroutine(slowDownAttackEffect(0.15f, _controller, _hit.point));
                //If it's a unit;
                if (_hit.collider.gameObject.GetComponentInParent<UnitComponent>() != null)
                {
                    UnitComponent _unitComp = _hit.collider.gameObject.GetComponentInParent<UnitComponent>();
                    if (_unitComp.GetComponentInParent<EnemyBehaviour>() != null)
                    {
                        StartCoroutine(changeTime(_unitComp.GetComponentInParent<EnemyBehaviour>(), m_slowDownTargetTime, m_slowDownDuration));
                    }
                }
            }
            else
            {
                //Miss 
                StartCoroutine(slowDownAttackEffect(0.15f, _controller, _controller.transform.position + _controller.transform.forward * 20f));
            }
        }

    }

    /// <summary>
    /// Creates the lightning effect seen during the regular attack.
    /// </summary>
    /// <param name="_playerScale"></param>
    /// <param name="_controller"></param>
    /// <param name="_hitPos"></param>
    /// <returns></returns>
    IEnumerator slowDownAttackEffect(float _playerScale, GameObject _controller, Vector3 _hitPos)
    {
        m_currentlyAttacking = true;
        //AudioManager.Instance.Play2DSound(SoundLists.zapSounds, false, 0, true, false, true);
        AudioManager.Instance.PlaySound("slowAttackLightning", AudioLists.Combat, AudioMixers.Effects, true, true, true, this.gameObject, 0.1f);
        //Create a seperate object to hold the zap effect.
        GameObject _goLine = new GameObject("regularAttackEffect");
        StartCoroutine(destroyEffect(_goLine, m_lightningTiming * m_lightningOccurance));
        _goLine.transform.parent = _controller.transform;

        List<LineRenderer> _lightningLines = new List<LineRenderer>();

        //Create the amount of line renderers required.
        for (int i = 0; i < m_lightningStrikes; i++)
        {
            GameObject _go = new GameObject("regularAttackEffect");
            _go.transform.parent = _goLine.transform;
            Destroy(_go, m_lightningTiming * m_lightningOccurance);
            _lightningLines.Add(new GameObject("regularAttackEffect").AddComponent<LineRenderer>());
            _lightningLines[i].material = m_lightningMaterial;
            _lightningLines[i].startWidth = 0.1f * _playerScale;
            _lightningLines[i].endWidth = 0.1f * _playerScale;
            _lightningLines[i].startColor = Color.blue;
            _lightningLines[i].endColor = Color.blue;
        }

        ///Break the line down into positions, equally sized.
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


        //Set the positions on the line to a random variation of those points. creates the lightning effect. 
        for (int i = 0; i < m_lightningOccurance; i++)
        {
            foreach (LineRenderer _line in _lightningLines)
            {
                _line.positionCount = _breakPositions.Count;
                Vector3[] _linePositions = new Vector3[_breakPositions.Count];
                _linePositions[0] = _controller.transform.position;
                for (int j = 1; j < _breakPositions.Count; j++)
                {
                    _linePositions[j] = _breakPositions[j] + Random.insideUnitSphere * m_lightningWidth * _playerScale;
                }
                _linePositions[_breakPositions.Count - 1] = _hitPos;
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

    #endregion

    #region Speed Up Attack
    /// <summary>
    /// Slows down the units the player is pointing at.
    /// </summary>
    /// <param name="_controller"></param>
    private void speedUpAttack(GameObject _controller)
    {
        if (m_currentlyAttacking == true)
        {
            return;
        }
        RumbleManager.Instance.heavyVibration(InputManager.Instance.Handedness);
        //Player is large, attack a hex.
        if (InputManager.Instance.CurrentSize == InputManager.SizeOptions.large)
        {
            RaycastHit _hit;
            if (Physics.Raycast(_controller.transform.position, _controller.transform.forward - _controller.transform.up, out _hit, 1000, 1 << LayerMask.NameToLayer("Environment")))
            {
                StartCoroutine(speedUpAttackEffect(0.3f, _controller, _hit.point));

                //Show the dust effect on the hex you hit
                Vector3 _effectPos = new Vector3(_hit.collider.gameObject.transform.position.x, _hit.collider.gameObject.transform.position.y + GameBoardGeneration.Instance.BuildingValidation.CurrentHeightOffset, _hit.collider.gameObject.transform.position.z);
                GameObject _dustEffect = Instantiate(m_lightningHitEffect, _effectPos, Quaternion.Euler(-90, 0, 0));
                Destroy(_dustEffect, 0.5f);

                //If you hit a collider of the environment which has an enemy on it.
                if (_hit.collider.gameObject.GetComponent<NodeComponent>().node.navigability == navigabilityStates.enemyUnit)
                {
                    //Find each unit within that group, and deal damage to them.
                    EnemyBehaviour _enemies = _hit.collider.gameObject.GetComponentInChildren<EnemyBehaviour>();
                    StartCoroutine(changeTime(_enemies, m_speedUpTargetTime, m_speedUpDuration));

                }
            }
        }
        else
        {
            RaycastHit _hit;
            if (Physics.Raycast(_controller.transform.position, _controller.transform.forward, out _hit, 1000))
            {
                StartCoroutine(speedUpAttackEffect(0.15f, _controller, _hit.point));
                //If it's a unit;
                if (_hit.collider.gameObject.GetComponentInParent<UnitComponent>() != null)
                {
                    UnitComponent _unitComp = _hit.collider.gameObject.GetComponentInParent<UnitComponent>();
                    if (_unitComp.GetComponentInParent<EnemyBehaviour>() != null)
                    {
                        StartCoroutine(changeTime(_unitComp.GetComponentInParent<EnemyBehaviour>(), m_speedUpTargetTime, m_speedUpDuration));
                    }
                }
            }
            else
            {
                //Miss 
                StartCoroutine(speedUpAttackEffect(0.15f, _controller, _controller.transform.position + _controller.transform.forward * 20f));
            }
        }

    }

    /// <summary>
    /// Creates the lightning effect seen during the regular attack.
    /// </summary>
    /// <param name="_playerScale"></param>
    /// <param name="_controller"></param>
    /// <param name="_hitPos"></param>
    /// <returns></returns>
    IEnumerator speedUpAttackEffect(float _playerScale, GameObject _controller, Vector3 _hitPos)
    {
        m_currentlyAttacking = true;
        //AudioManager.Instance.Play2DSound(SoundLists.zapSounds, false, 0, true, false, true);
        AudioManager.Instance.PlaySound("fastAttackLightning", AudioLists.Combat, AudioMixers.Effects, true, true, true, this.gameObject, 0.1f);

        //Create a seperate object to hold the zap effect.
        GameObject _goLine = new GameObject("regularAttackEffect");
        StartCoroutine(destroyEffect(_goLine, m_lightningTiming * m_lightningOccurance));
        _goLine.transform.parent = _controller.transform;

        List<LineRenderer> _lightningLines = new List<LineRenderer>();

        //Create the amount of line renderers required.
        for (int i = 0; i < m_lightningStrikes; i++)
        {
            GameObject _go = new GameObject("regularAttackEffect");
            _go.transform.parent = _goLine.transform;
            Destroy(_go, m_lightningTiming * m_lightningOccurance);
            _lightningLines.Add(new GameObject("regularAttackEffect").AddComponent<LineRenderer>());
            _lightningLines[i].material = m_lightningMaterial;
            _lightningLines[i].startWidth = 0.1f * _playerScale;
            _lightningLines[i].endWidth = 0.1f * _playerScale;
            _lightningLines[i].startColor = Color.magenta;
            _lightningLines[i].endColor = Color.magenta;
        }

        ///Break the line down into positions, equally sized.
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


        //Set the positions on the line to a random variation of those points. creates the lightning effect. 
        for (int i = 0; i < m_lightningOccurance; i++)
        {
            foreach (LineRenderer _line in _lightningLines)
            {
                _line.positionCount = _breakPositions.Count;
                Vector3[] _linePositions = new Vector3[_breakPositions.Count];
                _linePositions[0] = _controller.transform.position;
                for (int j = 1; j < _breakPositions.Count; j++)
                {
                    _linePositions[j] = _breakPositions[j] + Random.insideUnitSphere * m_lightningWidth * _playerScale;
                }
                _linePositions[_breakPositions.Count - 1] = _hitPos;
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

    #endregion




    /// <summary>
    /// Set the time perception of a enemy group to a certain value.
    /// </summary>
    /// <param name="_group"> The group to change</param>
    /// <param name="_targetTime">The time to set it to</param>
    /// <param name="_duration">The duration of the effect before changing back to 1.</param>
    /// <returns></returns>
    IEnumerator changeTime(EnemyBehaviour _group, float _targetTime, float _duration)
    {
        //Store original value.
        float _tempValue = _group.timePerception;
        _group.timePerception = _targetTime;
        yield return new WaitForSeconds(_duration);
        _group.timePerception = _tempValue;
    }





    /// <summary>
    /// Destroys the passed in object after a set time.
    /// </summary>
    /// <param name="_effect">Effect to destroy</param>
    /// <param name="_time">Time to destroy is after.</param>
    /// <returns></returns>
    IEnumerator destroyEffect(GameObject _effect,float _time)
    {
        yield return new WaitForSeconds(_time);
        Destroy(_effect);
    }
}
