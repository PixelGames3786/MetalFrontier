using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemData : ScriptableObject
{
    //�A�C�e���ʂ��ԍ�
    public int ItemNumber;

    public int price; //�̔����i

    public string itemName;

    [TextArea]
    public string Description;
}
