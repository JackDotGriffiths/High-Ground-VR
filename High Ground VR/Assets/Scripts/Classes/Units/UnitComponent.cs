using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitComponent : MonoBehaviour
{
    #region Variables
    [Header("This component is used for both ordinary and large enemies.")]
    [SerializeField, Tooltip("Body object of the unit. Used to change material")] private MeshRenderer m_unitBody;
    [SerializeField, Tooltip("Canvas on the enemy. Used for health bars, and other status updates.")] private Canvas m_enemyDisplay;
    [SerializeField, Tooltip("The health bar Image for the unit. Only used for Tank enemies.")] private Image m_healthBar;


    [SerializeField,Space(10), Tooltip("The lowest possible health of the unit.")] private float m_lowestHealthPlayer;
    [SerializeField, Tooltip("The highest possible health of the unit.")] private float m_highestHealthPlayer;
    [SerializeField, Space(3), Tooltip("The lowest possible health of the unit.")] private float m_lowestHealthEnemy;
    [SerializeField, Tooltip("The highest possible health of the unit.")] private float m_highestHealthEnemy;



    [SerializeField,Space(10), Tooltip("The lowest possible damage of the unit.")] private float m_lowestDamagePlayer;
    [SerializeField, Tooltip("The highest possible damage of the unit.")] private float m_highestDamagePlayer;
    [SerializeField, Space(3), Tooltip("The lowest possible damage of the unit.")] private float m_lowestDamageEnemy;
    [SerializeField, Tooltip("The highest possible damage of the unit.")] private float m_highestDamageEnemy;


    [SerializeField,Space(10), Tooltip("All possible materials for the player units.")] private List<Material> m_playerMaterials;
    [SerializeField, Tooltip("All possible materials for the enemy units.")] private List<Material> m_enemyMaterials;


    [SerializeField,Space(10), Tooltip("All possible helmets for the units to spawn with.")] private List<GameObject> m_helmets;
    [SerializeField, Tooltip("All possible weapons for the units to spawn with.")] private List<GameObject> m_weapons;
    #endregion


    private float m_maxHealth = 0.0f;

    public Unit unit;



    void Update()
    {
        try
        {
            m_enemyDisplay.transform.LookAt(Camera.main.transform);
            if(InputManager.Instance.CurrentSize == InputManager.SizeOptions.large)
            {
                m_enemyDisplay.transform.eulerAngles = new Vector3(m_enemyDisplay.transform.eulerAngles.x, m_enemyDisplay.transform.eulerAngles.y, m_enemyDisplay.transform.eulerAngles.z);
            }
            else
            {
                m_enemyDisplay.transform.eulerAngles = new Vector3(0.0f, m_enemyDisplay.transform.eulerAngles.y, 0.0f);
            }
            m_healthBar.fillAmount = Mathf.Lerp(m_healthBar.fillAmount, unit.health / m_maxHealth, 1.5f * Time.deltaTime);
        }
        catch { }

    }

    public void playerUnitConstructor()
    {
        unitTypes _unitType = unitTypes.player;
        float _health = Random.Range(m_lowestHealthPlayer, m_highestHealthPlayer);
        float _damage = Random.Range(m_lowestDamagePlayer, m_highestDamagePlayer);
        GameObject _helmet = m_helmets[Random.Range(0, m_helmets.Count)];
        GameObject _weapon = m_weapons[Random.Range(0, m_weapons.Count)];
        m_unitBody.material = m_playerMaterials[Random.Range(0, m_playerMaterials.Count)];

        unit = new Unit(_unitType,this,_health,_damage, _helmet,_weapon);
    }
    public void enemyUnitConstructor()
    {
        unitTypes _unitType = unitTypes.player;
        float _health = Random.Range(m_lowestHealthEnemy, m_highestHealthEnemy);
        m_maxHealth = _health;
        float _damage = Random.Range(m_lowestDamageEnemy, m_highestDamageEnemy);
        GameObject _helmet = m_helmets[Random.Range(0, m_helmets.Count)];
        GameObject _weapon = m_weapons[Random.Range(0, m_weapons.Count)];
        m_unitBody.material = m_enemyMaterials[Random.Range(0, m_enemyMaterials.Count)];

        unit = new Unit(_unitType,this, _health, _damage, _helmet, _weapon);
    }


    /// <summary>
    /// Kills the unit
    /// </summary>
    public void Die()
    {
        //Instantiate a duplicate of the object
        GameObject _deadUnit = Instantiate(this.gameObject, this.gameObject.transform.position, this.gameObject.transform.rotation, null);
        Rigidbody _rigid = _deadUnit.AddComponent<Rigidbody>();
        _rigid.interpolation = RigidbodyInterpolation.Extrapolate;
        _rigid.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        Destroy(_deadUnit.GetComponent<UnitComponent>());
        //Destroy the original. This ensures it's appropriately removed from lists.
        Destroy(this.gameObject);
        Destroy(_deadUnit, 5f);
    }
}
