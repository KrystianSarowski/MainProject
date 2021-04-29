using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

[System.Serializable]
public struct WeaponStats
{
    [HideInInspector]
    public float m_damageMultiplier;
    public float m_defaultDamageMultiplier;
    public float m_damageMultiplierIncrease;

    [HideInInspector]
    public float m_attackDelay;
    public float m_defaultAttackDelay;
    public float m_minAttackDelay;
    public float m_decreaseAttackDelay;
}

public class Weapon : MonoBehaviour
{
    [SerializeField]
    protected WeaponStats m_stats;

    public Vector3 m_spawnOffset;

    protected bool m_isAttacking = false;

    public virtual void Initialize()
    {
    }

    public void SetWeaponStats(WeaponStats t_weaponStats)
    {
        m_stats = t_weaponStats;
    }

    public WeaponStats GetWeaponStats()
    {
        return m_stats;
    }

    public virtual void Attack()
    {
        if(!m_isAttacking)
        {
            //Attack;
        }
    }

    public void DecreaseAttackDelay(int t_attackDelayLevel)
    {
        m_stats.m_attackDelay = m_stats.m_defaultAttackDelay - m_stats.m_decreaseAttackDelay * t_attackDelayLevel;

        if (m_stats.m_attackDelay < m_stats.m_minAttackDelay)
        {
            m_stats.m_attackDelay = m_stats.m_minAttackDelay;
        }
    }

    public void IncreaseDamageMultiplier(int t_damageMultiplierLevel)
    {
        m_stats.m_damageMultiplier = m_stats.m_defaultDamageMultiplier + m_stats.m_damageMultiplierIncrease * t_damageMultiplierLevel;
    }
}