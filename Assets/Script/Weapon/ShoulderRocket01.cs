using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class ShoulderRocket01 : WeaponBase
{
    public Transform[] bulletParent;

    [SerializeField]
    private Transform[] bullets;

    //残り弾数
    private int bulletLeft=4;

    //弾を一つ補充するための時間
    [SerializeField]
    private float bulletReloadTime;

    public GameObject BulletPrefab;

    public Transform ShotPosi;

    public Transform pivotObj;

    //射撃可能か
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
        if (!canShot || bulletLeft==0 || leftBulletNum==0) return;

        canShot = false;

        Transform bulletObj = bullets[bulletLeft-1];
        ShoulderRocket01Bullet bullet = bulletObj.GetComponent<ShoulderRocket01Bullet>();

        //自身のコライダーとぶつからないようにする
        //自身に当たらないように
        Physics.IgnoreCollision(bullet.bulletObj.GetComponent<Collider>(), controller.GetComponent<Collider>(), true);

        //ダメージ登録
        bullet.attackData.type = weaponData.attackType;
        bullet.attackData.damage = weaponData.damage;

        Vector3 shotPower=pivotObj.forward * weaponData.bulletSpeed;

        bullet.Shot(target,shotPower);

        bulletLeft--;

        BulletReload();

        //射撃可能間隔を待つ
        isIntervalWait = true;

        leftBulletNum--;
        leftBulletNum = Mathf.Clamp(leftBulletNum, 0, maxBulletNum);

        onLeftBulletChange?.Invoke(weaponPosition);
        OnStartInterval?.Invoke(weaponPosition);
    }

    public async void BulletReload()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(bulletReloadTime));

        bulletLeft++;

        bullets[bulletLeft-1] = Instantiate(BulletPrefab, bulletParent[bulletLeft-1]).transform;
    }

}