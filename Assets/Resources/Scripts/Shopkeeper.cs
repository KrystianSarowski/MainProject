using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski
public enum ShopItemType
{ 
    Heal,
    Poweup,
    Weapon
}

[System.Serializable]
public class ShopItem
{
    public string m_shopSpriteName;
    public string m_itemName;

    public ShopItemType m_itemType;

    public bool m_isAvailable;
    public bool m_isBought;

    public int m_cost;
}

public class Shopkeeper : MonoBehaviour
{
    [SerializeField]
    GameObject m_shopPrefab;

    Shop m_shop;

    List<ShopItem> m_shopItems = new List<ShopItem>();

    [SerializeField]
    int m_shopSize = 2;

    void Awake()
    {
        GameObject shopObj = GameObject.FindGameObjectWithTag("Shop");

        if(shopObj != null)
        {
            m_shop = shopObj.GetComponent<Shop>();
        }
        else
        {
            shopObj = Instantiate(m_shopPrefab, m_shopPrefab.transform.position, m_shopPrefab.transform.rotation);
            m_shop = shopObj.GetComponent<Shop>();
        }

        SetItemPool();
    }

    void SetItemPool()
    {
        List<ShopItem> shopItems = DataLoad.LoadShopItems("ShopItems");

        int count = 0;

        while (count < m_shopSize)
        {
            ShopItem item = shopItems[Random.Range(0, shopItems.Count)];

            if (!m_shopItems.Contains(item))
            {
                m_shopItems.Add(item);
                count++;
            }
        }
    }

    public void Interact()
    {
        foreach(ShopItem shopItem in m_shopItems)
        {
            shopItem.m_isAvailable = true;
        }

        foreach (ShopItem shopItem in m_shopItems)
        {
            if (shopItem.m_itemType == ShopItemType.Weapon)
            {
                if (!shopItem.m_isBought)
                {
                    if (shopItem.m_itemName == PlayerStats.GetWeaponName())
                    {
                        shopItem.m_isAvailable = false;
                        break;
                    }
                }
            }
        }

        m_shop.Interact();
        m_shop.SetItems(m_shopItems);
    }
}