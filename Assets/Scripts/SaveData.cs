using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
class SaveData
{
    //プレイヤー関連
    public int Level;
    public int MaxLife;
    public int Life;
    public int Food;
    public int Attack;
    public int MaxExp;
    public int Exp;
    public int FloorNumber;
    public string WeaponName;
    public int WeaponAttack;
    public List<string> MapData;

    //エネミー関連
    /// <summary>
    /// 未記入
    /// </summary>
    
    public void Save()
    {
        var json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("save",json);
    }

    public static SaveData Recover()
    {
        if (PlayerPrefs.HasKey("save"))
        {
            var json = PlayerPrefs.GetString("save");
            return JsonUtility.FromJson<SaveData>(json);
        }
        else 
        { 
            return null; 
        }
    }

    public static void Destoy()
    {
        PlayerPrefs.DeleteAll();
    }
}
