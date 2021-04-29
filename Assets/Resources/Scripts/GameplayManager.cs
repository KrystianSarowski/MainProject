using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//@Author Krystian Sarowski

public enum GenerationType
{
    TopDown,
    BottomUp
}

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager s_instance;

    [HideInInspector]
    public static string s_seedString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    [HideInInspector]
    public static System.Random s_seedRandom;

    public GenerationType m_generationType;

    public string m_seed;

    public static bool s_isPaused = false;

    [HideInInspector]
    public const int m_MAX_SEED_SIZE = 6;

    int m_currentLevel = 1;

    void Awake()
    {
        //Makes sure there is only one Gameplay manager
        if (s_instance != null)
        {
            DestroyImmediate(gameObject);
            return;
        }
        else
        {
            s_instance = this;
            DontDestroyOnLoad(gameObject);
            s_seedRandom = new System.Random(m_seed.GetHashCode());
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

    public void LoadFirstLevel()
    {
        m_currentLevel = 1;

        PlayerStats.s_maxHealth = 100;
        PlayerStats.s_health = PlayerStats.s_maxHealth;

        LoadScene("Level" + m_currentLevel);
    }

    public void LoadNextLevel()
    {
        m_currentLevel++;

        if (m_currentLevel > 3)
        {
            LoadScene("MenuScene");
        }

        else
        {
            LoadScene("Level" + m_currentLevel);
        }
    }

    public static void Pause()
    {
        Time.timeScale = 0;
        s_isPaused = true;
    }

    public static void UnPause()
    {
        Time.timeScale = 1;
        s_isPaused = false;
    }

    public static void LoadScene(string t_string)
    {
        SceneManager.LoadScene(t_string);
    }

    public int GetCurrentLevel()
    {
        return m_currentLevel;
    }
}
