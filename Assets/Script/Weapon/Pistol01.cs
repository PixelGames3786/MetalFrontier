using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random= UnityEngine.Random;

public class Pistol01 : WeaponBase
{
    public GameObject BulletPrefab;

    public Transform ShotPosi;

    public Transform pivotObj;

    private bool canShot=true;

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
        if (!canShot || leftBulletNum==0) return;

        //ステータスの射撃精度を考慮した実効ブレを計算
        float actualBlur = weaponData.shotBlur / (1 + legacyStatus.shotAccuracy);

        canShot = false;

        Transform bulletObj = Instantiate(BulletPrefab, ShotPosi.position, pivotObj.rotation).transform;
        Rigidbody bulletRb = bulletObj.GetChild(0).GetComponent<Rigidbody>();

        //自分自身とぶつからないようにする
        Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), transform.GetChild(0).GetComponent<Collider>(), true);
        Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), controller.GetComponent<Collider>(), true);

        //ダメージ登録
        Bullet bullet = bulletObj.GetComponentInChildren<Bullet>();

        bullet.attackData.type = weaponData.attackType;
        bullet.attackData.damage = weaponData.damage;

        Vector3 shotVector = Vector3.zero;

        if (target)
        {
            shotVector = (target.position - ShotPosi.position).normalized;
        }
        else
        {
            shotVector = pivotObj.forward;
        }

        //ブレを適用する ランダムを二つ組み合わせて正規分布っぽくする
        float verticalBlur = (Random.Range(-actualBlur, actualBlur) + Random.Range(-actualBlur, actualBlur)) / 2;
        float horizontalBlur = (Random.Range(-actualBlur, actualBlur) + Random.Range(-actualBlur, actualBlur)) / 2;

        Vector3 VerticalBlur = verticalBlur * pivotObj.right;
        Vector3 HorizontalBlur = horizontalBlur * pivotObj.up;

        shotVector = shotVector + VerticalBlur + HorizontalBlur;

        bulletRb.AddForce(shotVector*weaponData.bulletSpeed, ForceMode.Impulse);

        //射撃可能間隔を待つ
        isIntervalWait = true;

        leftBulletNum--;
        leftBulletNum = Mathf.Clamp(leftBulletNum,0,maxBulletNum);

        onLeftBulletChange?.Invoke(weaponPosition);

        OnStartInterval?.Invoke(weaponPosition);
    }

}