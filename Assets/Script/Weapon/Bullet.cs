using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float lifeTime;

    private CancellationTokenSource cts;

    [SerializeField]
    public AttackData attackData;

    [SerializeField]
    private GameObject particlePrefab;

    // Start is called before the first frame update
    void Start()
    {
        DestroyObjectAfterDelay(lifeTime);
    }

    public void OnCollisionEnter(Collision collision)
    {
        IDamageable damageAble = collision.transform.GetComponent<IDamageable>();

        if (damageAble!=null)
        {
            //damageAble.Damage(bulletPower);
            damageAble.Damage(attackData);

            //�p�[�e�B�N�����쐬
            GameObject particle = Instantiate(particlePrefab);

            particle.transform.position = collision.contacts[0].point;

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
            if (transform.parent.gameObject != null)
            {
                Destroy(transform.parent.gameObject);
            }
        }
        catch (OperationCanceledException)
        {
            // �L�����Z�����ꂽ�ꍇ�̏���
            Debug.Log("�I�u�W�F�N�g�폜���L�����Z������܂����B");
        }
    }
}
