using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//@Author Krystian Sarowski

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    GameObject m_canvasPrefab;
    GameObject m_playerCanvas;

    RectTransform m_healthBar;

    TMP_Text m_ammoText;
    TMP_Text m_goldText;
    TMP_Text m_seedText;

    Vector2 m_healthBarMaxSize;

    // Start is called before the first frame update
    public void Initialize()
    {
        m_playerCanvas = GameObject.FindGameObjectWithTag("Canvas");

        if(m_playerCanvas == null)
        {
            m_playerCanvas = Instantiate(m_canvasPrefab);
        }

        foreach(Transform child in m_playerCanvas.transform)
        {
            if(child.name == "HealthBar")
            {
                m_healthBar = child.GetComponent<RectTransform>();
                m_healthBarMaxSize = m_healthBar.sizeDelta;
            }
            else if(child.name == "Ammo Text")
            {
                m_ammoText = child.GetComponent<TMP_Text>();
            }
            else if(child.name == "Gold Text")
            {
                m_goldText = child.GetComponent<TMP_Text>();
            }
            else if (child.name == "Seed Text")
            {
                m_seedText = child.GetComponent<TMP_Text>();
                m_seedText.text = "Seed\n" + FindObjectOfType<GameplayManager>().m_seed;
            }
        }
    }

    void Update()
    {
        m_goldText.text = PlayerStats.s_gold.ToString();     
    }

    public void EnableAmmoText(bool t_setActive)
    {
        m_ammoText.gameObject.SetActive(t_setActive);
    }

    public void UpdateHealthBar(int t_health)
    {
        float prectange = t_health / 100.0f;

        m_healthBar.sizeDelta = new Vector2(m_healthBarMaxSize.x * prectange, m_healthBarMaxSize.y);

        if (prectange > 0.6f)
        {
            m_healthBar.GetComponent<Image>().color = Color.green;
        }
        else if (prectange > 0.2f)
        {
            m_healthBar.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            m_healthBar.GetComponent<Image>().color = Color.red;
        }
    }

    public void UpdateAmmoText(int t_ammoCount, int t_maxAmmo)
    {
        m_ammoText.text = t_ammoCount + "/" + t_maxAmmo;
    }
}