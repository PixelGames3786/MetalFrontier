using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public class ShoulderCanon01 : WeaponBase
{
    //íeÇàÍÇ¬ï‚è[Ç∑ÇÈÇΩÇﬂÇÃéûä‘
    [SerializeField]
    private float bulletReloadTime;

    [SerializeField]
    private Collider mainUnitCollider;

    public GameObject BulletPrefab;

    public Transform ShotPosi;

    public Transform pivotObj;

    //éÀåÇâ¬î\Ç©
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
        if (!canShot || leftBulletNum == 0) return;

        canShot = false;

        Transform bulletObj = Instantiate(BulletPrefab, ShotPosi.position, pivotObj.rotation).transform;

        Rigidbody bulletRb = bulletObj.GetChild(0).GetComponent<Rigidbody>();

        //é©êgÇ…ìñÇΩÇÁÇ»Ç¢ÇÊÇ§Ç…
        Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), transform.GetChild(0).GetComponent<Collider>(), true);
        Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), controller.GetComponent<Collider>(), true);

        //É_ÉÅÅ[ÉWìoò^
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

        bulletRb.AddForce(shotVector*weaponData.bulletSpeed, ForceMode.Impulse);

        //éÀåÇâ¬î\ä‘äuÇë“Ç¬
        isIntervalWait = true;

        leftBulletNum--;
        leftBulletNum = Mathf.Clamp(leftBulletNum, 0, maxBulletNum);

        onLeftBulletChange?.Invoke(weaponPosition);

        OnStartInterval?.Invoke(weaponPosition);
    }
}