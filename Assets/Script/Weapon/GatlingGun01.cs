using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GatlingGun01 : WeaponBase
{
    public GameObject BulletPrefab;

    public Transform ShotPosi;

    public Transform pivotObj;

    private bool canShot=true;

    //撃った際のブレ
    public float shotBlur;
    public float bulletSpeed,shotInterval;

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
        if (!canShot) return;

        canShot = false;

        Debug.DrawRay(pivotObj.position, pivotObj.forward, Color.red);

        Transform bulletObj = Instantiate(BulletPrefab, ShotPosi.position, pivotObj.rotation).transform;

        Rigidbody bulletRb = bulletObj.GetChild(0).GetComponent<Rigidbody>();

        Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), transform.GetChild(0).GetComponent<Collider>(), true);

        //ブレを適用する
        Vector3 shotVector = pivotObj.forward;
        Vector3 VerticalBlur = Random.Range(-shotBlur,shotBlur) * pivotObj.right;
        Vector3 HorizontalBlur = Random.Range(-shotBlur,shotBlur)*pivotObj.up;

        shotVector = shotVector+VerticalBlur+HorizontalBlur;

        bulletRb.AddForce(shotVector * bulletSpeed, ForceMode.Impulse);


        //射撃可能間隔を待つ
        isIntervalWait = true;

        OnStartInterval?.Invoke(weaponPosition);
    }

}