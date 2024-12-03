using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "WeaponPartsData", menuName = "PartsData/CreateWeaponParts")]
public class WeaponPartsData : ItemData
{
    public enum SetType
    {
        Shoulder,
        Arm
    }

    public enum WeaponType
    {
        Gun,
        Rocket,
    }

    //�U����ށ@�r�[��or����or�Œ�_���[�W
    public enum AttackType
    {
        Physicial,
        Beam,
        Constant,
    }

    public SetType setType;
    public WeaponType weaponType;
    public AttackType attackType;

    public SerializableDictionary<LegacySettingData.WeaponSetPosi, GameObject> setPosiWithPrefab;

    //1�̃p�[�c�ɕ����̃��b�V�����܂܂��ꍇ�����邽��
    //public SerializableDictionary<string, Mesh> ObjnameMeshPairs = new SerializableDictionary<string, Mesh>();

    //�p�����[�^

}
