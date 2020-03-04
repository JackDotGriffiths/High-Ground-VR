using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BuildingHealth : MonoBehaviour
{
    [SerializeField,Tooltip("Health of the building.")]private float m_health = 100;
    [SerializeField, Tooltip("Time after taking damage in which the building can regen health.")] private float m_cooldownTime = 4.0f;
    [SerializeField, Tooltip("Amount of health to regen. Usually a very small amount")] private float m_healthRegenAmount = 0.04f;
    [SerializeField, Tooltip("Whether or not this component is attatched to the gem. Acts differently when it's destroyed")] private bool m_isGem = false;
    public float currentHealth; //Current Health of the building.
    private bool m_canRegenHealth; //Tracks whether the building can regenerate health.

    private void Start()
    {
        currentHealth = m_health;
    }


    void FixedUpdate()
    {
        if (m_canRegenHealth && currentHealth < m_health)
        {
            currentHealth = Mathf.Clamp(currentHealth + m_healthRegenAmount, 0, m_health);
        }
    }

    /// <summary>
    /// Takes damage away from the building.
    /// </summary>
    /// <param name="damage">Damage to take away from the building.</param>
    public void takeDamage(float damage)
    {
        currentHealth -= damage;

        m_canRegenHealth = false;
        StopAllCoroutines();
        StartCoroutine("healthCooldown");

        if (currentHealth < 0 && m_isGem == false)
        {
            if (m_isGem)
            {
                GemDie();
            }
            else
            {
                Die();
            }
        }
    }

    /// <summary>
    /// Destroys the building
    /// </summary>
    void Die()
    {
        this.GetComponentInParent<NodeComponent>().node.navigability = navigabilityStates.navigable;
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Destroys the gem
    /// </summary>
    void GemDie()
    {
        this.GetComponentInParent<NodeComponent>().node.navigability = navigabilityStates.navigable;
        GameManager.Instance.GameOver = true;
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Runs a cooldown to prevent health regenerating immediately.
    /// </summary>
    /// <returns></returns>
    IEnumerator healthCooldown()
    {
        yield return new WaitForSeconds(m_cooldownTime);
        m_canRegenHealth = true;
    }


     void OnDrawGizmos()
    {
        Vector3 _position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        Handles.Label(_position, "Health : " + currentHealth);
    }
}
