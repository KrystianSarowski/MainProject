using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    public static int s_health = 100;
    public static int s_maxHealth = 100;
    public static int s_gold = 1000;

    static int s_baseMaxHealth = 100;
    static int s_maxHealthIncrease = 10;

    public static void DealDamage(int t_damage)
    {
        s_health -= t_damage;

        if(s_health < 0)
        {
            s_health = 0;
        }
        else if(s_health > s_maxHealth)
        {
            s_health = s_maxHealth;
        }
    }

    public static void IncreaseMaxHealth(int t_maxHealthLevel)
    {
        int previousValue = s_maxHealth;

        s_maxHealth = s_baseMaxHealth + s_maxHealthIncrease * t_maxHealthLevel;

        s_health = (int)(s_health * (s_maxHealth / (float)previousValue));

        if(s_health > s_maxHealth)
        {
            s_health = s_maxHealth;
        }
    }

    public static void IncreaseGold(int t_goldAmount)
    {
        s_gold += t_goldAmount;
    }

    public static void DecreaseGold(int t_goldAmount)
    {
        s_gold -= t_goldAmount;
    }
}