using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//@Author Krystian Sarowski

[System.Serializable]
class ShopItemsData
{
    public List<ShopItem> m_shopItems = new List<ShopItem>();
}

public class DataLoad
{
    public static UpgradeLevels LoadUpgradeData(string t_fileName)
    {
        string fullPath = "SaveData/" + t_fileName;

        TextAsset loadFile = Resources.Load<TextAsset>(fullPath);

        if (loadFile != null)
        {
            UpgradeLevels upgradeLevels = JsonUtility.FromJson<UpgradeLevels>(loadFile.text);

            return upgradeLevels;
        }

        return new UpgradeLevels();
    }

    public static List<ShopItem> LoadShopItems(string t_fileName)
    {
        string fullPath = "SaveData/" + t_fileName;

        TextAsset loadFile = Resources.Load<TextAsset>(fullPath);

        List<ShopItem> shopItems = new List<ShopItem>();

        if (loadFile != null)
        {
            shopItems = JsonUtility.FromJson<ShopItemsData>(loadFile.text).m_shopItems;
        }

        return shopItems;
    }
}
