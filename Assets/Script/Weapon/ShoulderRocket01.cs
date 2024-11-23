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

    public float bulletSpeed,shotInterval;

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
            intervalWaitTime += Time.deltaTime;

            if (intervalWaitTime >= useInterval)
            {
                canShot = true;
                isIntervalWait = false;

                intervalWaitTime = 0;
                OnEndInterval?.Invoke(weaponPosition);
            }
        }
    }

    public override async void Use(Transform target = null)
    {
        if (!canShot || bulletLeft==0) return;

        canShot = false;

        Transform bulletObj = bullets[bulletLeft-1];

        //自身のコライダーとぶつからないようにする
        Physics.IgnoreCollision(bulletObj.gameObject.GetComponent<Collider>(), FindObjectOfType<RobotPlayerInput>().GetComponent<Collider>(), true);

        Vector3 shotPower=pivotObj.forward * bulletSpeed;

        bulletObj.GetComponent<ShoulderRocket01Bullet>().Shot(target,shotPower);

        bulletLeft--;

        BulletReload();

        //射撃可能間隔を待つ
        isIntervalWait = true;
        OnStartInterval?.Invoke(weaponPosition);
    }

    public async void BulletReload()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(bulletReloadTime));

        bulletLeft++;

        bullets[bulletLeft-1] = Instantiate(BulletPrefab, bulletParent[bulletLeft-1]).transform;
    }

}