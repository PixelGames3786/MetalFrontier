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

    //攻撃種類　ビームor物理or固定ダメージ
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

    //1つのパーツに複数のメッシュが含まれる場合があるため
    //public SerializableDictionary<string, Mesh> ObjnameMeshPairs = new SerializableDictionary<string, Mesh>();

    //パラメータ

}
