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
    private float lifeTime,trackingTime; //生存時間　追尾時間

    private float trackingElapsedTime=0; //追尾経過時間

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

            //追尾時間中は追尾する
            if (trackingTime>=trackingElapsedTime)
            {
                // ゆるくターゲットの方向へ回転する
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);

                trackingElapsedTime += Time.deltaTime;
            }

            // 前方に進む
            rb.AddForce(transform.forward * bulletSpeed * Time.deltaTime);
        }
        else
        {
            // ターゲットがない場合、前方に進み続ける
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

    // オブジェクトの削除を待機する関数
    public async UniTaskVoid DestroyObjectAfterDelay(float delayInSeconds)
    {
        // CancellationTokenSourceを作成
        cts = new CancellationTokenSource();

        try
        {
            // 指定された秒数を待つ（途中でキャンセルされる可能性あり）
            await UniTask.Delay(TimeSpan.FromSeconds(delayInSeconds), cancellationToken: cts.Token);

            // オブジェクトがまだ存在する場合のみデストロイ
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合の処理
            Debug.Log("オブジェクト削除がキャンセルされました。");
        }
    }
}
