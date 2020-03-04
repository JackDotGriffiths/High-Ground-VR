using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitComponent : MonoBehaviour
{
    [SerializeField, Tooltip("Body object of the unit. Used to change material")] private MeshRenderer m_unitBody;


    [SerializeField, Space(10), Tooltip("The lowest possible health of the unit.")] private float m_lowestHealthPlayer;
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

    public Unit unit;



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
        float _damage = Random.Range(m_lowestDamageEnemy, m_highestDamageEnemy);
        GameObject _helmet = m_helmets[Random.Range(0, m_helmets.Count)];
        GameObject _weapon = m_weapons[Random.Range(0, m_weapons.Count)];
        m_unitBody.material = m_enemyMaterials[Random.Range(0, m_enemyMaterials.Count)];

        unit = new Unit(_unitType,this, _health, _damage, _helmet, _weapon);
    }


    //Kills the unit
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
