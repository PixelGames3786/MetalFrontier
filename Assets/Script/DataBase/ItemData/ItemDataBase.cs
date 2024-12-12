using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[CreateAssetMenu(fileName = "ItemDataBase", menuName = "DataBase/CreateItemDataBase")]
public class ItemDataBase : ScriptableObject
{
    public List<ItemData> itemDataList = new List<ItemData>();

    //アイテム番号からアイテムを取得
    public ItemData GetItem(int itemNum)
    {
        ItemData item = itemDataList.FirstOrDefault(item => item.ItemNumber == itemNum);

        if (item==null)
        {
            Debug.Log($"{itemNum}番のアイテムが見つからなかった……");
        }

        return item;
    }

    //アイテム番号のリストからアイテムのリストに変換
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
