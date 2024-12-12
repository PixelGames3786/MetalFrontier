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
    //���K�V�[�̃p�[�c�ݒ�
    public LegacySettingData settingData;

    //������
    public int haveCol {get; private set;}

    //���݃V���b�v�ōw���\�ȃA�C�e���̔ԍ���ێ�
    public List<int> shopGoodsNumList= new List<int>();

    //���݉������Ă���~�b�V�����̔ԍ���ێ�
    public List<int> missionNumList = new List<int>();

    //���ݏ������Ă���A�C�e���̔ԍ������X�g�ŕێ�����
    public List<int> itemNumList = new List<int>();

    //���ݏ������Ă���A�C�e����ێ�
    public List<HavingItem> havingItemList = new List<HavingItem>();

    //���ݏ������Ă���A�C�e���̃��X�g
    [JsonIgnore]
    public List<ItemData> itemDataList
    {
        get { return DataBaseController.instance.itemDataBase.GetItemList(havingItemList.Select(item => item.itemNumber).ToList()); }
    }

    //���ݍw���\�ȃA�C�e���̃��X�g
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

    //���ݏ������Ă��郌�K�V�[�p�[�c�̃��X�g
    [JsonIgnore]
    public List<HavingItem> PartsDataList
    {
        get { return havingItemList.Where(item => item.itemData is BodyPartsData).ToList(); }
    }

    //���ݏ������Ă��镐��p�[�c�̃��X�g
    [JsonIgnore]
    public List<HavingItem> WeaponsDataList
    {
        get { return havingItemList.Where(item => item.itemData is WeaponPartsData).ToList(); }
    }


    //�Z�[�u�f�[�^����������
    public void DataInitialize()
    {
        settingData = new LegacySettingData();

        missionNumList.Add(1);

        //TODO �f�o�b�O�p�ɍŏ��ɂρ[����������ɓ����
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


    //���K�V�[�̃J�X�^�}�C�Y�ύX
    public void LegacyPartsChange(PartsType type, HavingItem having)
    {
        if (settingData.PartsNumber[type] != null) settingData.PartsNumber[type].equiped = false;

        settingData.SetBodyParts(type,having);

        having.equiped = true;
    }

    public void WeaponPartsChange(LegacySettingData.WeaponSetPosi setPosi, HavingItem having)
    {
        //������������Ă��Ȃ����ʂɑ�������ꍇ
        if (settingData.WeaponsNumber[setPosi] == null)
        {
            settingData.SetWeaponParts(setPosi,having);

            having.equiped = true;

            return;
        }
        else //���ɂȂ�炩�̃p�[�c����������Ă��镔�ʂɑ�������ꍇ�A�O�̃p�[�c�̑����󋵂��O��
        {
            settingData.WeaponsNumber[setPosi].equiped = false;

            settingData.SetWeaponParts(setPosi, having);

            settingData.WeaponsNumber[setPosi].equiped = true;
        }
    }

    //���ʂɂ����Ă��镐����O��
    public void WeaponPartsRemove(LegacySettingData.WeaponSetPosi setPosi, HavingItem having)
    {
        //���������ɑ�������Ă���A�C�e���Ɠ����Ȃ�ΊO��
        if (having == settingData.WeaponsNumber[setPosi])
        {
            settingData.SetWeaponParts(setPosi, null);

            having.equiped = false;

            Debug.Log("�O�����I");
        }
        else
        {
            Debug.Log("�O���Ȃ��Ȃ��I");

        }
    }

    //�A�C�e����ǉ����鏈����
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

    //�A�C�e�����擾���鏈����
    public HavingItem GetItem(string id)
    {
        HavingItem item = havingItemList.Find(item => item.uniqueId == id);

        if (item==null)
        {
            Debug.LogError("�A�C�e����������񂩂����I");
        }

        return item;
    }

    //Col(������)�𑝂₷/���炷
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

    //�p�[�c�̈ʒu��HavingItem��UniqueId�̎���
    //�Z�[�u���[�h�ɂ����HavingItem�C���X�^���X���Đ������ꓯ���C���X�^���X�Ƃ��Ĉ����Ȃ��Ȃ�̂����
    [JsonProperty]
    private Dictionary<PartsType, string> partsPairs = new Dictionary<PartsType, string>();

    //���K�V�[�p�[�c�@���ʕʃA�C�e���ԍ�
    //public Dictionary<BodyPartsData.PartsType, HavingItem> PartsNumber = new Dictionary<BodyPartsData.PartsType, HavingItem>();
    [JsonIgnore]
    public Dictionary<PartsType, HavingItem> PartsNumber = new Dictionary<PartsType, HavingItem>();

    //����̈ʒu��HavingItem��uniqueId�̎����@���R�͏�L
    [JsonProperty]
    private Dictionary<WeaponSetPosi, string> weaponsPairs = new Dictionary<WeaponSetPosi, string>();

    //����p�[�c�@���ʕʃA�C�e���ԍ�
    [JsonIgnore]
    public Dictionary<WeaponSetPosi, HavingItem> WeaponsNumber = new Dictionary<WeaponSetPosi, HavingItem>();

    //�p�[�c�ݒ��
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

    //PartsNumber������UniqueId�̎����ɍ��킹�Đݒ肷��
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

    //�����ς݂�����
    public bool equiped;

    public int moduleSlotNum=0; //���W���[�����ݒ�\�ȃX���b�g��

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

        //�A�C�e���̎�ނ�BodyParts,�܂���Weapon�������烂�W���[���X���b�g���������_���ɏo��
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
