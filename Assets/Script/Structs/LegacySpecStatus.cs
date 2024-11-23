using System;
using UnityEngine;

[Serializable]
public class LegacySpecStatus //動かす際のステータス　
{
    public float maxHP;

    public float moveSpeed;
    public float boostSpeed;

    //ジャンプ一回目の力と継続的に上昇する際の力
    public float jumpForce;
    public float riseForce;

    //射撃精度
    public float shotAccuracy;

    //ブースト使用可能量
    public float boostAmount;

    //ブースト使用倍率
    public float boostUseRate;

    //ブースト回復倍率
    public float boostRecoverRate;

    //物理耐性・ビーム耐性
    public float physicalRes;
    public float beamRes;

    //最大速度　通常移動時とブースト時
    public Vector3 maxVel, boostMaxVel;

    //最大覚醒量
    public float quorraAmount;
}