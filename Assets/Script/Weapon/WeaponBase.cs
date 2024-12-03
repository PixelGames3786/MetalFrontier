using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponPartsData weaponData;

    public bool isIntervalWait { get; protected set; }

    public float useInterval,intervalWaitTime;

    public LegacySpecStatus legacyStatus;

    public LegacySettingData.WeaponSetPosi weaponPosition;

    public Action<LegacySettingData.WeaponSetPosi> OnStartInterval,OnEndInterval;

    public RobotController controller;

    public abstract void Use(Transform target=null);
}
