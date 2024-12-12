using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponPartsData weaponData;

    public bool isIntervalWait { get; protected set; } //待機時間中か
    public bool isUseBullet { get; protected set; } //使用回数が限られているかどうか

    public float useInterval,intervalElapseTime;
    public int maxBulletNum, leftBulletNum; //最大使用回数 残り使用回数

    public LegacySpecStatus legacyStatus;

    public LegacySettingData.WeaponSetPosi weaponPosition;

    public Action<LegacySettingData.WeaponSetPosi> OnStartInterval,OnEndInterval; //使用間隔関係のデリゲート
    public Action<LegacySettingData.WeaponSetPosi> onLeftBulletChange; //残弾数が変更された際のデリゲート

    public RobotController controller;

    public void DataInitialize(WeaponPartsData data)
    {
        weaponData = data;

        useInterval = data.useInterval;

        if (data.defaultBullet==0) //デフォルトの最大使用回数が0ならば使用回数が限られていない
        {
            isUseBullet = false;
        }
        else
        {
            isUseBullet=true;

            maxBulletNum = data.defaultBullet;
            leftBulletNum=data.defaultBullet;
        }
    }

    public abstract void Use(Transform target=null);
}
