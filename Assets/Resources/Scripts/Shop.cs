using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//@Author Krystian Sarowski

public class Shop : MonoBehaviour
{
    [SerializeField]
    Canvas m_shopCanvas;

    int m_activeIndex = 0;
    bool m_isActive = false;

    Vector3 m_itemsStartPos = new Vector3(120, 250, 0);
    Vector3 m_itemsPosOffset = new Vector3(140, 140, 0);

    [SerializeField]
    GameObject m_shopItemPrefab;

    List<GameObject> m_shopItems = new List<GameObject>();

    [SerializeField]
    TMP_Text m_goldText;

    void Awake()
    {
        m_shopCanvas.gameObject.SetActive(m_isActive);
    }

    void Update()
    {
        if(m_isActive)
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Interact();
            }

            m_goldText.text = PlayerStats.s_gold.ToString();
        }
    }

    public void Interact()
    {
        m_activeIndex = (m_activeIndex + 1) % 2;
        m_isActive = m_activeIndex != 0;

        m_shopCanvas.gameObject.SetActive(m_isActive);

        if(m_isActive)
        {
            GameplayManager.Pause();
        }
        else
        {
            GameplayManager.UnPause();
        }
    }

    public void SetItems(List<ShopItem> t_shopItems)
    {
        for(int i = m_shopItems.Count - 1; i >= 0; i--)
        {
            Destroy(m_shopItems[i]);
        }

        m_shopItems.Clear();

        foreach(ShopItem item in t_shopItems)
        {
            GameObject shopItemButton = Instantiate(m_shopItemPrefab, m_shopCanvas.transform);
            shopItemButton.GetComponent<ShopItemButton>().Initialize(item);

            Vector3 positon = m_itemsStartPos * m_shopCanvas.scaleFactor;
            positon.x += m_itemsPosOffset.x * (m_shopItems.Count % 5) * m_shopCanvas.scaleFactor;
            positon.y -= m_itemsPosOffset.y * (m_shopItems.Count / 5) * m_shopCanvas.scaleFactor;

            shopItemButton.transform.position = positon;

            m_shopItems.Add(shopItemButton);
        }
    }
}