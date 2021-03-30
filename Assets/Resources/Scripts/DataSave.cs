using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class DataSave 
{
    static string s_jsonPath = Application.dataPath + "/Resources/SaveData/";

    public static void SaveWeaponData(WeaponStats t_weaponStats, string t_fileName)
    {
        string jsonData = JsonUtility.ToJson(t_weaponStats, true);
        string fullPath = s_jsonPath + t_fileName + ".json";

        System.IO.File.WriteAllText(fullPath, jsonData);
        AssetDatabase.Refresh();
    }
}