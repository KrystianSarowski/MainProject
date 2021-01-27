using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    GameObject m_canvasPrefab;
    GameObject m_playerCanvas;

    RectTransform m_healthBar;

    Vector2 m_healthBarMaxSize;

    // Start is called before the first frame update
    void Start()
    {
        m_playerCanvas = GameObject.FindGameObjectWithTag("Canvas");

        if(m_playerCanvas == null)
        {
            m_playerCanvas = Instantiate(m_canvasPrefab);
        }
        else
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
        }
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
}
