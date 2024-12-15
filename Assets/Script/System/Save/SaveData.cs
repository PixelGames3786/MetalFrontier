using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PartsType = BodyPartsData.PartsType;

public class SaveData
{
    //レガシーのパーツ設定
    public LegacySettingData settingData;

    //所持金
    public int haveCol {get; private set;}

    //現在ショップで購入可能なアイテムの番号を保持
    public List<int> shopGoodsNumList= new List<int>();

    //現在解放されているミッションの番号を保持
    public List<int> missionNumList = new List<int>();

    //現在所持しているアイテムの番号をリストで保持する
    public List<int> itemNumList = new List<int>();

    //現在所持しているアイテムを保持
    public List<HavingItem> havingItemList = new List<HavingItem>();

    //現在所持しているアイテムのリスト
    [JsonIgnore]
    public List<ItemData> itemDataList
    {
        get { return DataBaseController.instance.itemDataBase.GetItemList(havingItemList.Select(item => item.itemNumber).ToList()); }
    }

    //現在購入可能なアイテムのリスト
    [JsonIgnore]
    public List<ItemData> shopGoodsList
    {
        get { return DataBaseController.instance.itemDataBase.GetItemList(shopGoodsNumList); }
    }

    [JsonIgnore]
    public List<MissionData> missionDataList
    {
        get { return DataBaseController.instance.missionDataBase.GetMissionList(missionNumList); }
    }

    //現在所持しているレガシーパーツのリスト
    [JsonIgnore]
    public List<HavingItem> PartsDataList
    {
        get { return havingItemList.Where(item => item.itemData is BodyPartsData).ToList(); }
    }

    //現在所持している武器パーツのリスト
    [JsonIgnore]
    public List<HavingItem> WeaponsDataList
    {
        get { return havingItemList.Where(item => item.itemData is WeaponPartsData).ToList(); }
    }


    //セーブデータ初期化処理
    public void DataInitialize()
    {
        settingData = new LegacySettingData();

        settingData.RefleshWeaponsNumber();
        settingData.RefleshPartsNumber();

        missionNumList.Add(1);

        //TODO デバッグ用に最初にぱーつをいくつか手に入れる
        AddItemRange(new List<int> { 306, 307, 308, 309, 310, 311, 312, 313, 314, 315, 601,607, 602,606 });

        shopGoodsNumList.Add(601);
        shopGoodsNumList.Add(602);
        shopGoodsNumList.Add(301);

        LegacyPartsChange(PartsType.Head, AddItem(301));
        LegacyPartsChange(PartsType.Body, AddItem(302));
        LegacyPartsChange(PartsType.LeftArm, AddItem(303));
        LegacyPartsChange(PartsType.RightArm, AddItem(304));
        LegacyPartsChange(PartsType.Leg, AddItem(305));

        WeaponPartsChange(LegacySettingData.WeaponSetPosi.LeftArm, WeaponsDataList[0]);
        WeaponPartsChange(LegacySettingData.WeaponSetPosi.LeftShoulder, WeaponsDataList[1]);
    }


    //レガシーのカスタマイズ変更
    public void LegacyPartsChange(PartsType type, HavingItem having)
    {
        if (settingData.PartsNumber[type] != null) settingData.PartsNumber[type].equiped = false;

        settingData.SetBodyParts(type,having);

        having.equiped = true;
    }

    public void WeaponPartsChange(LegacySettingData.WeaponSetPosi setPosi, HavingItem having)
    {
        if (having.equiped)
        {
            Debug.Log("装備中であるためアイテムを外す");

            WeaponPartsRemove(setPosi, having);
        }
        else
        {
            Debug.Log("装備中でないためアイテムを交換");

            //何も装備されていない部位に装備する場合
            if (settingData.WeaponsNumber[setPosi] == null)
            {
                settingData.SetWeaponParts(setPosi, having);

                having.equiped = true;

                return;
            }
            else //既になんらかのパーツが装備されている部位に装備する場合、前のパーツの装備状況を外す
            {
                settingData.WeaponsNumber[setPosi].equiped = false;

                settingData.SetWeaponParts(setPosi, having);

                settingData.WeaponsNumber[setPosi].equiped = true;
            }
        }
    }

    //部位につけられている武器を外す
    public void WeaponPartsRemove(LegacySettingData.WeaponSetPosi setPosi, HavingItem having)
    {
        //もしも既に装備されているアイテムと同じならば外す
        if (having == settingData.WeaponsNumber[setPosi])
        {
            settingData.SetWeaponParts(setPosi, null);

            having.equiped = false;

            Debug.Log("外した！");
        }
        else
        {
            Debug.Log("外せないなぁ！");
        }
    }

    //アイテムを追加する処理類
    public HavingItem AddItem(int itemNum)
    {
        HavingItem item = new HavingItem(itemNum);

        havingItemList.Add(item);

        return item;
    }

    public void AddItem(HavingItem item) 
    {
        havingItemList.Add(item);
    }

    public void AddItemRange(int[] itemNumArray)
    {
        foreach (int itemNum in itemNumArray)
        {
            havingItemList.Add(new HavingItem(itemNum));
        }
    }

    public void AddItemRange(HavingItem[] itemArray)
    {
        foreach (var item in itemArray) Debug.Log(item.itemData.itemName);

        havingItemList.AddRange(itemArray);

    }

    public void AddItemRange(List<int> itemNumList)
    {
        foreach (int itemNum in itemNumList)
        {
            havingItemList.Add(new HavingItem(itemNum));
        }
    }

    //アイテムを取得する処理類
    public HavingItem GetItem(string id)
    {
        HavingItem item = havingItemList.Find(item => item.uniqueId == id);

        if (item==null)
        {
            Debug.LogError("アイテムが見つからんかった！");
        }

        return item;
    }

    //Col(所持金)を増やす/減らす
    public void ColChange(int changeVal)
    {
        haveCol += changeVal;
    }
}
