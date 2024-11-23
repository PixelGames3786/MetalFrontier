using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ShoulderRocket01Bullet : MonoBehaviour
{
    private Transform target;

    public int bulletPower;

    [SerializeField]
    private float bulletSpeed, rotateSpeed;

    [SerializeField]
    private float lifeTime,trackingTime; //�������ԁ@�ǔ�����

    private float trackingElapsedTime=0; //�ǔ��o�ߎ���

    private bool isMoving = false;

    private CancellationTokenSource cts;

    private Rigidbody rb;

    private Vector3 dir;

    [SerializeField]
    public AttackData attackData;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving) return;

        if (target != null)
        {
            dir = (target.position - transform.position).normalized;

            //�ǔ����Ԓ��͒ǔ�����
            if (trackingTime>=trackingElapsedTime)
            {
                // ��邭�^�[�Q�b�g�̕����։�]����
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);

                trackingElapsedTime += Time.deltaTime;
            }

            // �O���ɐi��
            rb.AddForce(transform.forward * bulletSpeed * Time.deltaTime);
        }
        else
        {
            // �^�[�Q�b�g���Ȃ��ꍇ�A�O���ɐi�ݑ�����
            rb.AddForce(transform.forward * bulletSpeed * Time.deltaTime);
        }
    }

    public void Shot(Transform tag,Vector3 shotPower)
    {
        rb = GetComponent<Rigidbody>();

        target = tag;

        GetComponent<Rigidbody>().isKinematic = false;

        GetComponent<BoxCollider>().enabled = true;

        rb.AddForce(shotPower, ForceMode.Impulse);

        transform.parent = null;

        isMoving = true;

        DestroyObjectAfterDelay(lifeTime);
    }

    public void OnCollisionEnter(Collision collision)
    {
        IDamageable damageAble = collision.transform.GetComponent<IDamageable>();

        if (damageAble != null)
        {
            damageAble.Damage(attackData);

            cts.Cancel();
            cts.Dispose();

            Destroy(transform.parent.gameObject);
        }
        else
        {
            cts.Cancel();
            cts.Dispose();

            Destroy(transform.parent.gameObject);
        }
    }

    // �I�u�W�F�N�g�̍폜��ҋ@����֐�
    public async UniTaskVoid DestroyObjectAfterDelay(float delayInSeconds)
    {
        // CancellationTokenSource���쐬
        cts = new CancellationTokenSource();

        try
        {
            // �w�肳�ꂽ�b����҂i�r���ŃL�����Z�������\������j
            await UniTask.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken: cts.Token);

            // �I�u�W�F�N�g���܂����݂���ꍇ�̂݃f�X�g���C
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        catch (OperationCanceledException)
        {
            // �L�����Z�����ꂽ�ꍇ�̏���
            Debug.Log("�I�u�W�F�N�g�폜���L�����Z������܂����B");
        }
    }
}
