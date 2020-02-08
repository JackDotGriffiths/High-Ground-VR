using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum unitTypes { player, enemy };

public class Unit
{
    public unitTypes unitType;
    public int health;
    public int damage;
    public float intelligence;
    public float timePerception = 1.0f; //Timeperception is a value that is changed to impact either slowness or speedyness on a player or enemy unit.
    public GameObject helmet;
    public GameObject weapon;

    /// <summary>
    /// Unit Constructor Class
    /// </summary>
    /// <param name="_unit">Unit type, player or enemy.</param>
    /// <param name="_health">Health of the Unit</param>
    /// <param name="_damage">Damage of the Unit</param>
    /// <param name="_intelligence">Intelligence of the unit. Used for deciding if they fall into traps. 0 : none 1: full intelligence</param>
    /// <param name="_helmet"> Helmet Gameobject to spawn on the unit</param>
    /// <param name="_weapon">Weapon Gameobject to spawn on the unit</param>
    public Unit(unitTypes _unit, int _health, int _damage, float _intelligence, GameObject _helmet, GameObject _weapon)
    {
        unitType = _unit;
        health = _health;
        damage = _damage;
        intelligence = _intelligence;
        helmet = _helmet;
        weapon = _weapon;
    }
}
