using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GenerationType
{
    TopDown,
    BottomUp
}

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager s_instance;

    public GenerationType m_generationType;

    public string m_seed;

    [HideInInspector]
    public const int m_MAX_SEED_SIZE = 6;

    public static System.Random s_seedRandom;

    [HideInInspector]
    public static string s_seedString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    void Awake()
    {
        //Makes sure there is only one Gameplay manager
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            s_instance = this;
            DontDestroyOnLoad(gameObject);
            s_seedRandom = new System.Random(m_seed.GetHashCode());
            m_generationType = GenerationType.BottomUp;
        }
    }

   public void UseRandomSeed()
   {
        m_seed = "";

        int count = 0;

        while(count < m_MAX_SEED_SIZE)
        {
            char c = s_seedString[Random.Range(0, s_seedString.Length)];
            m_seed += c;

            count++;
        }

        s_seedRandom = new System.Random(m_seed.GetHashCode());
   }

    public void UseCustomSeed(string t_cutomSeed)
    {
        m_seed = t_cutomSeed;
        s_seedRandom = new System.Random(m_seed.GetHashCode());
    }

    public static void LoadScene(string t_string)
    {
        SceneManager.LoadScene(t_string);
    }
}
