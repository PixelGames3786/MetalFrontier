using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol01 : WeaponBase
{
    public GameObject BulletPrefab;

    public Transform ShotPosi;

    public Transform pivotObj;

    private bool canShot=true;

    public float maxAngle, bulletSpeed,shotInterval;

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


        Vector3 shotPower = Vector3.zero;

        if (target)
        {
            shotPower = (target.position - ShotPosi.position).normalized * bulletSpeed;
        }
        else
        {
            shotPower = pivotObj.forward * bulletSpeed;
        }

        bulletRb.AddForce(shotPower, ForceMode.Impulse);


        //ŽËŒ‚‰Â”\ŠÔŠu‚ð‘Ò‚Â
        isIntervalWait = true;

        OnStartInterval?.Invoke(weaponPosition);
    }

}