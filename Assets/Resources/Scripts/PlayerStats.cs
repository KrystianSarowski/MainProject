using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats
{
    public static int s_health = 100;
    public static int s_maxHealth = 100;

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

    public static void IncreaseMaxHealth(int t_increase)
    {
        s_maxHealth += t_increase;
    }
}
