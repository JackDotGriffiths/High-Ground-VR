using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class BuildingHealth : MonoBehaviour
{
    [SerializeField,Tooltip("Health of the building.")]private float m_maxHealth = 100;
    [SerializeField, Tooltip("Time after taking damage in which the building can regen health.")] private float m_cooldownTime = 4.0f;
    [SerializeField, Tooltip("Amount of health to regen. Usually a very small amount")] private float m_healthRegenAmount = 0.04f;
    [SerializeField, Tooltip("Health bar image. Used to display the current health of the building."),Space (10)] private Image m_healthBar;
    [SerializeField] private Color m_idleColour = new Color(54, 164, 50, 0); //Transparent Green
    [SerializeField] private Color m_highHealthColour = new Color(54, 164, 50, 180); //Visible Green
    [SerializeField] private Color m_mediumHealthColour = new Color(255, 185, 0, 180); //Visible Yellow
    [SerializeField] private Color m_lowHealthColour = new Color(255, 0, 0, 255); //Visible Red
    [SerializeField, Tooltip("Whether or not this component is attatched to the gem. Acts differently when it's destroyed"), Space(10)] private bool m_isGem = false;


    public float currentHealth; //Current Health of the building.
    private bool m_canRegenHealth; //Tracks whether the building can regenerate health.
    private Color m_targetColour;

    private void Start()
    {
        currentHealth = m_maxHealth;
        m_healthBar.GetComponentInParent<RectTransform>().eulerAngles = new Vector3(-90, 0, 0);
    }

    void FixedUpdate()
    {
        if (m_canRegenHealth && currentHealth < m_maxHealth)
        {
            currentHealth = Mathf.Clamp(currentHealth + m_healthRegenAmount, 0, m_maxHealth);
        }
    }


    private void Update()
    {
        if(m_healthBar != null)
        {
            //Colour the bar appropriately.
            float _healthPercentage = currentHealth / m_maxHealth;


            if (_healthPercentage == 1)
            {
                m_targetColour = m_idleColour;
            }
            if (_healthPercentage < 0.9f && _healthPercentage > 0.5f)
            {
                m_targetColour = m_highHealthColour;
            }
            if (_healthPercentage < 0.5f && _healthPercentage > 0.25)
            {
                m_targetColour = m_mediumHealthColour;
            }
            if (_healthPercentage < 0.25f)
            {
                m_targetColour = m_lowHealthColour;
            }

            //Lerp the colour so it fades between them
            m_healthBar.color = Color.Lerp(m_healthBar.color, m_targetColour, 0.03f);

            //Make sure the fill amount is representitive of the health of the building.
            m_healthBar.fillAmount = Mathf.Lerp(m_healthBar.fillAmount, _healthPercentage, 0.5f);
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

        if (currentHealth < 0)
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
        this.GetComponentInParent<NodeComponent>().node.navigability = nodeTypes.navigable;
        AudioManager.Instance.PlaySound("destroyBuilding", AudioLists.Building, AudioMixers.Effects,true, true, false, this.gameObject, 0.1f);
        Instantiate(GameManager.Instance.buildingDestructionEffect, transform.position,Quaternion.Euler(-90,0,0));
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Destroys the gem
    /// </summary>
    void GemDie()
    {
        this.GetComponentInParent<NodeComponent>().node.navigability = nodeTypes.navigable;
        GameManager.Instance.GameOver = true;
        AudioManager.Instance.PlaySound("destroyBuilding", AudioLists.Building, AudioMixers.Effects, true, true, false, this.gameObject, 0.1f);
        AudioManager.Instance.PlaySound("gameStarted/Over", AudioLists.Combat, AudioMixers.Music, false, true, true, this.gameObject, 0.1f);
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

#if UNITY_EDITOR
    /// <summary>
    /// Draws the health value above the building for debug purposes.
    /// </summary>
    void OnDrawGizmos()
    {
        Vector3 _position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        Handles.Label(_position, "Health : " + currentHealth);
    }
#endif
}
