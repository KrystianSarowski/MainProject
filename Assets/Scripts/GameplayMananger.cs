using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayMananger : MonoBehaviour
{
    public static GameplayMananger s_instance;

    public string m_seed;

    public static System.Random s_seedRandom;

    void Awake()
    {
        //Makes sure there is only one Gameplay manager
        if (s_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            s_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        s_seedRandom = new System.Random(m_seed.GetHashCode());
    }
}
