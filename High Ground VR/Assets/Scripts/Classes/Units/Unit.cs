using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum unitTypes { player, enemy };

public class Unit
{
    public unitTypes unitType;
    public float health;
    public float damage;
    public float intelligence;
    public float timePerception = 1.0f;
    public GameObject helmet;
    public GameObject weapon;

    public Unit(unitTypes _unit, float _health, float _damage, float _intelligence, GameObject _helmet, GameObject _weapon)
    {
        unitType = _unit;
        health = _health;
        damage = _damage;
        intelligence = _intelligence;
        helmet = _helmet;
        weapon = _weapon;
    }
}
