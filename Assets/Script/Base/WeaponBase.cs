using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponPartsData weaponData;

    public bool isIntervalWait { get; protected set; } //�ҋ@���Ԓ���
    public bool isUseBullet { get; protected set; } //�g�p�񐔂������Ă��邩�ǂ���

    public float useInterval,intervalElapseTime;
    public int maxBulletNum, leftBulletNum; //�ő�g�p�� �c��g�p��

    public LegacySpecStatus legacyStatus;

    public LegacySettingData.WeaponSetPosi weaponPosition;

    public Action<LegacySettingData.WeaponSetPosi> OnStartInterval,OnEndInterval; //�g�p�Ԋu�֌W�̃f���Q�[�g
    public Action<LegacySettingData.WeaponSetPosi> onLeftBulletChange; //�c�e�����ύX���ꂽ�ۂ̃f���Q�[�g

    public RobotController controller;

    public void DataInitialize(WeaponPartsData data)
    {
        weaponData = data;

        useInterval = data.useInterval;

        if (data.defaultBullet==0) //�f�t�H���g�̍ő�g�p�񐔂�0�Ȃ�Ύg�p�񐔂������Ă��Ȃ�
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
