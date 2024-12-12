using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class ShoulderMissile01 : WeaponBase
{
    public Transform[] bulletPoints;

    [SerializeField]
    private Transform[] bullets;

    public MissileBullet bulletPrefab;

    public Transform pivotObj;

    

    //�ˌ��\��
    private bool canShot=true;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isIntervalWait)
        {
            intervalElapseTime += Time.deltaTime;

            if (intervalElapseTime >= weaponData.useInterval)
            {
                canShot = true;
                isIntervalWait = false;

                intervalElapseTime = 0;
                OnEndInterval?.Invoke(weaponPosition);
            }
        }
    }

    public override async void Use(Transform target = null)
    {
        if (!canShot || !target || leftBulletNum==0) return;

        canShot = false;

        //�ˌ��\�Ԋu��҂�
        isIntervalWait = true;
        OnStartInterval?.Invoke(weaponPosition);

        foreach (Transform firePoint in bulletPoints)
        {
            if (leftBulletNum == 0) break;

            MissileBullet missile = Instantiate(bulletPrefab,firePoint);

            //���g�̃R���C�_�[�ƂԂ���Ȃ��悤�ɂ���
            //���g�ɓ�����Ȃ��悤��
            Physics.IgnoreCollision(missile.bulletCol, controller.GetComponent<Collider>(), true);

            missile.attackData.type = weaponData.attackType;
            missile.attackData.damage = weaponData.damage;

            missile.Shot(target, weaponData.bulletSpeed);

            leftBulletNum--;
            leftBulletNum = Mathf.Clamp(leftBulletNum, 0, maxBulletNum);

            onLeftBulletChange?.Invoke(weaponPosition);

            await UniTask.Delay(200);
        }

    }

}