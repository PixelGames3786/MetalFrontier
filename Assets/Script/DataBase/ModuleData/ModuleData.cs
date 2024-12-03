using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "ModuleData", menuName = "Data/CreateModule")]
public class ModuleData : ScriptableObject
{
    //�ʂ��ԍ�
    public int moduleNumber;

    public int price; //�̔����i

    public string moduleName;

    [TextArea]
    public string description;
}
