using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//@Author Krystian Sarowski


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

    public static void SaveGenerationData(CombinedData t_generationData)
    {
        string timeString = System.DateTime.Now.TimeOfDay.ToString();
        timeString = timeString.Replace(":", "");
        timeString = timeString.Replace(".", "");

        string jsonData = JsonUtility.ToJson(t_generationData, true);
        string fullPath = s_jsonPath + "Generation" + timeString + ".json";

        System.IO.File.WriteAllText(fullPath, jsonData);
        AssetDatabase.Refresh();
    }
}