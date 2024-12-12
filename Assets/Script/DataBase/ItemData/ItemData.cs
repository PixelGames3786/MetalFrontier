using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemData : ScriptableObject
{
    public enum ItemType
    {
        BodyParts,
        WeaponParts,
        Module,
    }

    //�A�C�e���ʂ��ԍ�
    public int ItemNumber;

    public ItemType itemType;

    public int price; //�̔����i

    public string itemName;

    [TextArea]
    public string Description;
}
