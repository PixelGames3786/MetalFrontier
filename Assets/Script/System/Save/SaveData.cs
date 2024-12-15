using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PartsType = BodyPartsData.PartsType;

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

        settingData.RefleshWeaponsNumber();
        settingData.RefleshPartsNumber();

        missionNumList.Add(1);

        //TODO �f�o�b�O�p�ɍŏ��ɂρ[����������ɓ����
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


    //���K�V�[�̃J�X�^�}�C�Y�ύX
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
            Debug.Log("�������ł��邽�߃A�C�e�����O��");

            WeaponPartsRemove(setPosi, having);
        }
        else
        {
            Debug.Log("�������łȂ����߃A�C�e��������");

            //������������Ă��Ȃ����ʂɑ�������ꍇ
            if (settingData.WeaponsNumber[setPosi] == null)
            {
                settingData.SetWeaponParts(setPosi, having);

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
