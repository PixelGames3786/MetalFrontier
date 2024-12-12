using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using PartsType = BodyPartsData.PartsType;
using Random = UnityEngine.Random;

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

        missionNumList.Add(1);

        //TODO デバッグ用に最初にぱーつをいくつか手に入れる
        AddItemRange(new List<int> { 306, 307, 308, 309, 310, 311, 312, 313, 314, 315, 501,507, 502,506 });

        shopGoodsNumList.Add(501);
        shopGoodsNumList.Add(502);
        shopGoodsNumList.Add(502);
        shopGoodsNumList.Add(502);
        shopGoodsNumList.Add(502);
        shopGoodsNumList.Add(502);
        shopGoodsNumList.Add(502);
        shopGoodsNumList.Add(502);
        shopGoodsNumList.Add(502);
        shopGoodsNumList.Add(301);

        settingData.SetBodyParts(PartsType.Head, AddItem(301));
        settingData.SetBodyParts(PartsType.Body, AddItem(302));
        settingData.SetBodyParts(PartsType.LeftArm, AddItem(303));
        settingData.SetBodyParts(PartsType.RightArm, AddItem(304));
        settingData.SetBodyParts(PartsType.Leg, AddItem(305));

        settingData.SetWeaponParts(LegacySettingData.WeaponSetPosi.LeftArm, WeaponsDataList[0]);
        settingData.SetWeaponParts(LegacySettingData.WeaponSetPosi.RightArm, null);
        settingData.SetWeaponParts(LegacySettingData.WeaponSetPosi.LeftShoulder, WeaponsDataList[1]);
        settingData.SetWeaponParts(LegacySettingData.WeaponSetPosi.RightShoulder, null);
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
        //何も装備されていない部位に装備する場合
        if (settingData.WeaponsNumber[setPosi] == null)
        {
            settingData.SetWeaponParts(setPosi,having);

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

public class LegacySettingData
{
    public enum WeaponSetPosi
    {
        LeftArm,
        RightArm,
        LeftShoulder,
        RightShoulder,
    }

    //パーツの位置とHavingItemのUniqueIdの辞書
    //セーブロードによってHavingItemインスタンスが再生成され同じインスタンスとして扱われなくなるのを回避
    [JsonProperty]
    private Dictionary<PartsType, string> partsPairs = new Dictionary<PartsType, string>();

    //レガシーパーツ　部位別アイテム番号
    //public Dictionary<BodyPartsData.PartsType, HavingItem> PartsNumber = new Dictionary<BodyPartsData.PartsType, HavingItem>();
    [JsonIgnore]
    public Dictionary<PartsType, HavingItem> PartsNumber = new Dictionary<PartsType, HavingItem>();

    //武器の位置とHavingItemのuniqueIdの辞書　理由は上記
    [JsonProperty]
    private Dictionary<WeaponSetPosi, string> weaponsPairs = new Dictionary<WeaponSetPosi, string>();

    //武器パーツ　部位別アイテム番号
    [JsonIgnore]
    public Dictionary<WeaponSetPosi, HavingItem> WeaponsNumber = new Dictionary<WeaponSetPosi, HavingItem>();

    //パーツ設定類
    public void SetBodyParts(PartsType type, HavingItem having)
    {
        if (having == null)
        {
            partsPairs[type] = null;
        }
        else
        {
            partsPairs[type] = having.uniqueId;
        }

        RefleshPartsNumber();
    }

    public void SetWeaponParts(WeaponSetPosi posi, HavingItem having)
    {
        if (having==null)
        {
            weaponsPairs[posi] = null;
        }
        else
        {
            weaponsPairs[posi] = having.uniqueId;
        }

        RefleshWeaponsNumber();
    }

    //PartsNumber辞書をUniqueIdの辞書に合わせて設定する
    public void RefleshPartsNumber()
    {
        Dictionary<PartsType, HavingItem> dic = new Dictionary<PartsType, HavingItem>();

        foreach (KeyValuePair<PartsType, string> keyValue in partsPairs)
        {
            if (keyValue.Value == null) dic[keyValue.Key]=null; 
            else dic[keyValue.Key] = SaveDataManager.instance.saveData.GetItem(keyValue.Value);
        }

        PartsNumber = dic;
    }

    public void RefleshWeaponsNumber()
    {
        Dictionary<WeaponSetPosi, HavingItem> dic = new Dictionary<WeaponSetPosi, HavingItem>();

        foreach (KeyValuePair<WeaponSetPosi, string> keyValue in weaponsPairs)
        {
            if (keyValue.Value == null) dic[keyValue.Key] = null;
            else dic[keyValue.Key] = SaveDataManager.instance.saveData.GetItem(keyValue.Value);
        }

        WeaponsNumber = dic;
    }
}

public class HavingItem
{
    public string uniqueId;

    public int itemNumber;

    //装備済みを示す
    public bool equiped;

    public int moduleSlotNum=0; //モジュールが設定可能なスロット数

    [JsonIgnore]
    private ItemData _itemData;

    [JsonIgnore]
    public ItemData itemData
    {
        get
        {
            if (_itemData == null)
            {
                _itemData = GetItem();
            }
            return _itemData;
        }
    }

    public HavingItem(int num)
    {
        uniqueId = Guid.NewGuid().ToString();

        itemNumber = num;

        _itemData = GetItem();

        //アイテムの種類がBodyParts,またはWeaponだったらモジュールスロット数をランダムに出す
        switch (_itemData.itemType)
        {
            case ItemData.ItemType.BodyParts:

                BodyPartsData bodyData = itemData as BodyPartsData;

                moduleSlotNum = Random.Range(bodyData.minModuleSlot,bodyData.maxModuleSlot);

                break;

            case ItemData.ItemType.WeaponParts:

                WeaponPartsData weaponData = itemData as WeaponPartsData;

                moduleSlotNum = Random.Range(weaponData.minModuleSlot, weaponData.maxModuleSlot);

                break;
        }
    }

    [JsonConstructor]
    public HavingItem(string uniqueId, int itemNumber, bool equipped, int moduleSlotNum)
    {
        this.uniqueId = uniqueId;
        this.itemNumber = itemNumber;
        this.equiped = equipped;
        this.moduleSlotNum = moduleSlotNum;

        _itemData = GetItem();
    }

    public ItemData GetItem()
    {
        return DataBaseController.instance.itemDataBase.GetItem(itemNumber);
    }
}
