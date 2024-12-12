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

    //ƒAƒCƒeƒ€’Ê‚µ”Ô†
    public int ItemNumber;

    public ItemType itemType;

    public int price; //”Ì”„‰¿Ši

    public string itemName;

    [TextArea]
    public string Description;
}
