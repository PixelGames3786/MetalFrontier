using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;


public class ShotGun01 : WeaponBase
{
    public GameObject BulletPrefab;

    public Transform ShotPosi;

    public Transform pivotObj;

    private bool canShot=true;

    public float maxAngle, bulletSpeed,shotInterval;

    public float shotBlur;
    //�V���b�g�K���Ȃ̂ŁA�����ɂǂꂭ�炢�̒e�����˂���邩
    public int shotBulletNum;

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

        //�X�e�[�^�X�̎ˌ����x���l�����������u�����v�Z
        float actualBlur = shotBlur / (1+legacyStatus.shotAccuracy);

        //�����ɕ������˂���
        for (int i=0;i<shotBulletNum;i++)
        {
            Transform bulletObj = Instantiate(BulletPrefab, ShotPosi.position, pivotObj.rotation).transform;

            Rigidbody bulletRb = bulletObj.GetChild(0).GetComponent<Rigidbody>();

            Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), transform.GetChild(0).GetComponent<Collider>(), true);

            //�u����K�p���� �����_�����g�ݍ��킹�Đ��K���z���ۂ�����
            Vector3 shotVector = pivotObj.forward;
            float verticalBlur = (Random.Range(-actualBlur, actualBlur) + Random.Range(-actualBlur, actualBlur)) / 2;
            float horizontalBlur= (Random.Range(-actualBlur, actualBlur) + Random.Range(-actualBlur, actualBlur)) / 2;

            Vector3 VerticalBlur = verticalBlur * pivotObj.right;
            Vector3 HorizontalBlur = horizontalBlur * pivotObj.up;

            shotVector = shotVector + VerticalBlur + HorizontalBlur;

            bulletRb.AddForce(shotVector * bulletSpeed, ForceMode.Impulse);
        }

        //�ˌ��\�Ԋu��҂�
        isIntervalWait = true;

        OnStartInterval?.Invoke(weaponPosition);
    }

}