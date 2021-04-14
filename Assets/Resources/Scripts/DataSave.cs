using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



public class DataSave 
{
    static string s_jsonPath = Application.dataPath + "/Resources/SaveData/";

    public static void SaveUpgradeData(UpgradeLevels t_upgradeLevels, string t_fileName)
    {
        string jsonData = JsonUtility.ToJson(t_upgradeLevels, true);
        string fullPath = s_jsonPath + t_fileName + ".json";

        System.IO.File.WriteAllText(fullPath, jsonData);
        AssetDatabase.Refresh();
    }
}