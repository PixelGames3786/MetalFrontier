using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PartsType = BodyPartsData.PartsType;

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
    private Dictionary<PartsType, string> partsPairs = new Dictionary<PartsType, string>()
    {
        { PartsType.Head,null},
        { PartsType.Body,null},
        { PartsType.LeftArm,null},
        { PartsType.RightArm,null},
        { PartsType.Leg,null},
    };

    //���K�V�[�p�[�c�@���ʕʃA�C�e���ԍ�
    //public Dictionary<BodyPartsData.PartsType, HavingItem> PartsNumber = new Dictionary<BodyPartsData.PartsType, HavingItem>();
    [JsonIgnore]
    public Dictionary<PartsType, HavingItem> PartsNumber = new Dictionary<PartsType, HavingItem>();

    //����̈ʒu��HavingItem��uniqueId�̎����@���R�͏�L
    [JsonProperty]
    private Dictionary<WeaponSetPosi, string> weaponsPairs = new Dictionary<WeaponSetPosi, string>()
    {
        { WeaponSetPosi.LeftArm,null },
        { WeaponSetPosi.RightArm,null },
        { WeaponSetPosi.LeftShoulder,null },
        { WeaponSetPosi.RightShoulder,null },
    };

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
        if (having == null)
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
            if (keyValue.Value == null) dic[keyValue.Key] = null;
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