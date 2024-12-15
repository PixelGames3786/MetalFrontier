using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.UIElements;


public class MissileBullet : MonoBehaviour
{
    [SerializeField]
    private float lifeTime;

    private CancellationTokenSource cts;

    [SerializeField]
    public AttackData attackData;

    private Rigidbody rb;

    public Transform target;
    public float torqueRatio;
    public float speed,firstTime,rotateSpeed;

    private float elapsedTime=0f;

    private bool isHoming=false;

    public GameObject bulletObj,particlePrefab;
    public Collider bulletCol;

    public LineRenderer lineRenderer;

    private List<Vector3> linePoints=new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime>=firstTime)
        {
            isHoming = true;
        }

        if (bulletObj)
        {
            linePoints.Add(transform.position);

            if (linePoints.Count > 30)  // �_�̍ő吔��50�ɐ���
            {
                linePoints.RemoveAt(0);
            }
        }
        else
        {
            linePoints.Add(transform.position);
            linePoints.RemoveAt(0);
            linePoints.RemoveAt(1);

            if (linePoints.Count==1)
            {
                Destroy(gameObject);
            }
        }

        if (lineRenderer) DrawLine();
    }


    void FixedUpdate()
    {
        if (target == null) return;

        if (isHoming)
        {
            //�������^�[�Q�b�g�̕��Ɍ�����
            var diff = (target.transform.position - transform.position).normalized;

            // ��邭�^�[�Q�b�g�̕����։�]����
            Quaternion lookRotation = Quaternion.LookRotation(diff,transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        }

        //�܂������i��
        rb.velocity = transform.forward*speed;
    }

    public void Shot(Transform tag, float bulletSpeed)
    {
        target = tag;
        speed = bulletSpeed;

        transform.parent = null;

        DestroyObjectAfterDelay(lifeTime);
    }

    private void DrawLine()
    {
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray()); // �I�u�W�F�N�g�̈ʒu�����Z�b�g
    }

    public void OnCollisionEnter(Collision collision)
    {
        IDamageable damageAble = collision.transform.GetComponent<IDamageable>();

        if (damageAble != null)
        {
            damageAble.Damage(attackData);

            //�p�[�e�B�N�����쐬
            GameObject particle = Instantiate(particlePrefab);

            particle.transform.position = collision.contacts[0].point;

            cts.Cancel();
            cts.Dispose();

            Destroy(bulletObj);
        }
        else
        {
            cts.Cancel();
            cts.Dispose();

            Destroy(bulletObj);
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
