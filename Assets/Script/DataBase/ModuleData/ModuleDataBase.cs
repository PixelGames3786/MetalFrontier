using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[CreateAssetMenu(fileName = "ModuleDataBase", menuName = "DataBase/CreateModuleDataBase")]
public class ModuleDataBase : ScriptableObject
{
    public List<ModuleData> ModuleDataList = new List<ModuleData>();

    //アイテム番号からアイテムを取得
    public ModuleData GetModule(int moduleNum)
    {
        return ModuleDataList.FirstOrDefault(module => module.ItemNumber == moduleNum);
    }

    //アイテム番号のリストからアイテムのリストに変換
    public List<ModuleData> GetModuleList(List<int> moduleNumList)
    {
        List<ModuleData> returnList = new List<ModuleData>();

        foreach (int moduleNum in moduleNumList)
        {
            returnList.Add(GetModule(moduleNum));
        }

        return returnList;
    }
}
