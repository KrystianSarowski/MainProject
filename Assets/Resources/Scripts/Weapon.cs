using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeaponStats
{
    public float m_damageMultiplier;
    public float m_damageMultiplierIncrease;

    public float m_attackDelay;
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
        if(FindObjectOfType<GameplayManager>().GetCurrentLevel() != 1)
        {
            m_stats = DataLoad.LoadWeaponData("Weapon");
        }
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

    public void DecreaseAttackDelay()
    {
        m_stats.m_attackDelay -= m_stats.m_decreaseAttackDelay;

        if (m_stats.m_attackDelay < m_stats.m_minAttackDelay)
        {
            m_stats.m_attackDelay = m_stats.m_minAttackDelay;
        }
    }

    public void IncreaseDamageMultiplier()
    {
        m_stats.m_damageMultiplier += m_stats.m_damageMultiplierIncrease;
    }

    public void SaveWeaponStats()
    {
        DataSave.SaveWeaponData(m_stats, "Weapon");
    }
}
