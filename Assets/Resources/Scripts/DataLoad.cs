using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class ShopItemsData
{
    public List<ShopItem> m_shopItems = new List<ShopItem>();
}

public class DataLoad
{
    public static WeaponStats LoadWeaponData(string t_fileName)
    {
        string fullPath = "SaveData/" + t_fileName;

        TextAsset loadFile = Resources.Load<TextAsset>(fullPath);

        if (loadFile != null)
        {
            WeaponStats loadedLevel = JsonUtility.FromJson<WeaponStats>(loadFile.text);

            return loadedLevel;
        }

        return new WeaponStats();
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
