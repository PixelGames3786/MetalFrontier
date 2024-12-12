using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[CreateAssetMenu(fileName = "ModuleDataBase", menuName = "DataBase/CreateModuleDataBase")]
public class ModuleDataBase : ScriptableObject
{
    public List<ModuleData> ModuleDataList = new List<ModuleData>();

    //�A�C�e���ԍ�����A�C�e�����擾
    public ModuleData GetModule(int moduleNum)
    {
        return ModuleDataList.FirstOrDefault(module => module.ItemNumber == moduleNum);
    }

    //�A�C�e���ԍ��̃��X�g����A�C�e���̃��X�g�ɕϊ�
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
