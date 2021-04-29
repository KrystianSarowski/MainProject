using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//@Author Krystian Sarowski

public class ShopItemButton : MonoBehaviour
{
    [SerializeField]
    Button m_button;

    [SerializeField]
    Image m_image;

    [SerializeField]
    TMP_Text m_costText;
    
    [SerializeField]
    TMP_Text m_nameText;

    ShopItem m_shopItem;

    [SerializeField]
    Image m_soldImage;

    public void Initialize(ShopItem t_shopItem)
    {
        m_shopItem = t_shopItem;

        m_costText.text = m_shopItem.m_cost.ToString();
        m_nameText.text = m_shopItem.m_itemName.ToString();

        m_image.sprite = Resources.Load<Sprite>("Sprites/" + m_shopItem.m_shopSpriteName);

        if(!m_shopItem.m_isAvailable || m_shopItem.m_isBought)
        {
            m_button.interactable = false;
        }

        if (m_shopItem.m_isBought)
        {
            m_soldImage.gameObject.SetActive(true);
        }

        m_button.onClick.AddListener(Purchase);
    }

    // Update is called once per frame
    void Update()
    {
        if(m_shopItem.m_isAvailable && !m_shopItem.m_isBought)
        {
            if(PlayerStats.s_gold < m_shopItem.m_cost)
            {
                m_button.interactable = false;
            }
            else
            {
                m_button.interactable = true;
            }
        }
    }

    public void Purchase()
    {
        m_shopItem.m_isBought = true;
        m_soldImage.gameObject.SetActive(true);
        m_button.interactable = false;

        PlayerStats.DecreaseGold(m_shopItem.m_cost);

        PlayerController playerController = FindObjectOfType<PlayerController>();

        if(playerController != null)
        {
            playerController.UsePurchasedItem(m_shopItem.m_itemType, m_shopItem.m_itemName);
        }
    }
}