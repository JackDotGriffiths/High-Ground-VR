using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitComponent : MonoBehaviour
{
    [SerializeField, Tooltip("Body object of the unit. Used to change material")] private MeshRenderer m_unitBody;


    [SerializeField,Space(20), Tooltip("The lowest possible health of the unit.")] private float m_lowestHealth;
    [SerializeField, Tooltip("The highest possible health of the unit.")] private float m_highestHealth;
    [SerializeField,Space(7), Tooltip("The lowest possible damage of the unit.")] private float m_lowestDamage;
    [SerializeField, Tooltip("The highest possible damage of the unit.")] private float m_highestDamage;
    [SerializeField, Space(7), Tooltip("The lowest possible intelligence of the unit.")] private float m_lowestIntelligence;
    [SerializeField, Tooltip("The highest possible intelligence of the unit.")] private float m_highestIntelligence;
    [SerializeField,Space(10), Tooltip("All possible materials for the player units.")] private List<Material> m_playerMaterials;
    [SerializeField, Tooltip("All possible materials for the enemy units.")] private List<Material> m_enemyMaterials;
    [SerializeField,Space(10), Tooltip("All possible helmets for the units to spawn with.")] private List<GameObject> m_helmets;
    [SerializeField, Tooltip("All possible weapons for the units to spawn with.")] private List<GameObject> m_weapons;

    public Unit unit;
    // Start is called before the first frame update
    public void playerUnitConstructor()
    {
        unitTypes _unitType = unitTypes.player;
        int _health = (int)Random.Range(m_lowestHealth, m_highestHealth);
        int _damage = (int)Random.Range(m_lowestDamage, m_highestDamage);
        float _intelligence = m_highestIntelligence; //Player Units are always 100% intelligent
        GameObject _helmet = m_helmets[Random.Range(0, m_helmets.Count)];
        GameObject _weapon = m_weapons[Random.Range(0, m_weapons.Count)];
        m_unitBody.material = m_playerMaterials[Random.Range(0, m_playerMaterials.Count)];

        unit = new Unit(_unitType,_health,_damage,_intelligence,_helmet,_weapon);
    }

    public void enemyUnitConstructor()
    {
        unitTypes _unitType = unitTypes.player;
        int _health = (int)Random.Range(m_lowestHealth, m_highestHealth);
        int _damage = (int)Random.Range(m_lowestDamage, m_highestDamage);
        float _intelligence = m_highestIntelligence; //Player Units are always 100% intelligent
        GameObject _helmet = m_helmets[Random.Range(0, m_helmets.Count)];
        GameObject _weapon = m_weapons[Random.Range(0, m_weapons.Count)];
        m_unitBody.material = m_enemyMaterials[Random.Range(0, m_enemyMaterials.Count)];

        unit = new Unit(_unitType, _health, _damage, _intelligence, _helmet, _weapon);
    }
}
