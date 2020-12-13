using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public TMP_Text m_seedText;

    string m_seedString;

    bool m_inputingCustomSeed = false;

    // Update is called once per frame
    void Update()
    {
        if(m_inputingCustomSeed)
        {
            UpdateCusomSeed();
        }
    }

    public void SetIsInputingCustomSeed(bool t_inputingCustomSeed)
    {
        m_inputingCustomSeed = t_inputingCustomSeed;

        if(t_inputingCustomSeed)
        {
            m_seedString = "";
        }
    }

    public void UpdateCusomSeed()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (m_seedString.Length != 0)
            {
                m_seedString = m_seedString.Substring(0, m_seedString.Length - 1);
            }
        }

        if (Input.inputString != "")
        {
            string c = "" + Input.inputString[0];

            if (GameplayManager.s_seedString.Contains(c))
            {
                if (GameplayManager.m_MAX_SEED_SIZE > m_seedString.Length)
                {
                    m_seedString += c;
                }
            }
        }

        m_seedText.text = m_seedString;
    }

    public void ChangeScene(string t_sceneName)
    {
        if(m_inputingCustomSeed)
        {
            FindObjectOfType<GameplayManager>().UseCustomSeed(m_seedString);
        }
        else
        {
            FindObjectOfType<GameplayManager>().UseRandomSeed();
        }

        GameplayManager.LoadScene(t_sceneName);
    }
}
