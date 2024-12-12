using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "ModuleData", menuName = "PartsData/CreateModule")]
public class ModuleData : ItemData
{
    public enum ModuleSetType
    {
        Body,
        Weapon,
    }

    public enum ModuleEffectType
    {
        AttackUp,
    }

    public ModuleSetType setType;

    public int useSlotNum; //�X���b�g�����g�g�p����̂�

    public List<ModuleEffectType> effectTypes = new List<ModuleEffectType>();
    public List<float> effectPowers= new List<float>();
}
