using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "BodyPartsData", menuName = "PartsData/CreateBodyParts")]
public class BodyPartsData : ItemData
{
    public enum PartsType
    {
        Head,
        Body,
        LeftArm,
        RightArm,
        Leg,
    }

    public PartsType Type;

    public int maxModuleSlot;
    public int minModuleSlot;


    //1つのパーツに複数のメッシュが含まれる場合があるため
    public SerializableDictionary<string,Mesh> ObjnameMeshPairs=new SerializableDictionary<string, Mesh>();

    //パラメータ
    public float hpFactor;

    public float moveSpeed;
    public float boostSpeed;

    //ジャンプ一回目の力と継続的に上昇する際の力
    public float jumpForce;
    public float riseForce;

    //射撃精度
    public float shotAccuracy;

    //ブースト使用可能量
    public float boostAmout;


    public float boostUseRate; //ブースト使用倍率
    //ブースト回復倍率
    public float boostRecoverRate;

    //物理耐性・ビーム耐性
    public float physicalRes;
    public float beamRes;

    //最大速度　通常移動時とブースト時
    public Vector3 maxVel, boostMaxVel;

}
