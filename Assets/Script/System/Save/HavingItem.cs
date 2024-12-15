using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class HavingItem
{
    public string uniqueId;

    public int itemNumber;

    //�����ς݂�����
    public bool equiped;

    public int moduleSlotNum = 0; //���W���[�����ݒ�\�ȃX���b�g��

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

        Debug.Log(itemData.name);

        //�A�C�e���̎�ނ�BodyParts,�܂���Weapon�������烂�W���[���X���b�g���������_���ɏo��
        switch (_itemData.itemType)
        {
            case ItemData.ItemType.BodyParts:

                BodyPartsData bodyData = itemData as BodyPartsData;

                moduleSlotNum = Random.Range(bodyData.minModuleSlot, bodyData.maxModuleSlot);

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