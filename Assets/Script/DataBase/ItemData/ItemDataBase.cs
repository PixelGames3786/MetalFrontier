using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[CreateAssetMenu(fileName = "ItemDataBase", menuName = "DataBase/CreateItemDataBase")]
public class ItemDataBase : ScriptableObject
{
    public List<ItemData> itemDataList = new List<ItemData>();

    //�A�C�e���ԍ�����A�C�e�����擾
    public ItemData GetItem(int itemNum)
    {
        return itemDataList.FirstOrDefault(item => item.ItemNumber == itemNum);
    }

    //�A�C�e���ԍ��̃��X�g����A�C�e���̃��X�g�ɕϊ�
    public List<ItemData> GetItemList(List<int> itemNumList)
    {
        List<ItemData> returnList = new List<ItemData>();

        foreach (int itemNum in itemNumList)
        {
            returnList.Add(GetItem(itemNum));
        }

        return returnList;
    }
}
