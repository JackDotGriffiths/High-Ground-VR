using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitComponent : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_unitBody;
    [SerializeField] private Material m_unitMaterial;


    [SerializeField,Space(20)] private float m_lowestHealth;
    [SerializeField] private float m_highestHealth;
    [SerializeField,Space(7)] private float m_lowestDamage;
    [SerializeField] private float m_highestDamage;
    [SerializeField, Space(7)] private float m_lowestIntelligence;
    [SerializeField] private float m_highestIntelligence;
    [SerializeField,Space(10)] private List<Material> m_playerMaterials;
    [SerializeField] private List<Material> m_enemyMaterials;
    [SerializeField,Space(10)] private List<GameObject> m_helmets;
    [SerializeField] private List<GameObject> m_weapons;

    public Unit unit;
    // Start is called before the first frame update
    public void playerUnitConstructor()
    {
        unitTypes _unitType = unitTypes.player;
        float _health = Random.Range(m_lowestHealth, m_highestHealth);
        float _damage = Random.Range(m_lowestDamage, m_highestDamage);
        float _intelligence = m_highestIntelligence; //Player Units are always 100% intelligent
        GameObject _helmet = m_helmets[Random.Range(0, m_helmets.Count)];
        GameObject _weapon = m_weapons[Random.Range(0, m_weapons.Count)];
        m_unitBody.material = m_playerMaterials[Random.Range(0, m_playerMaterials.Count)];

        unit = new Unit(_unitType,_health,_damage,_intelligence,_helmet,_weapon);
    }
}
