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

            if (linePoints.Count > 30)  // 点の最大数を50に制限
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
            //向きをターゲットの方に向ける
            var diff = (target.transform.position - transform.position).normalized;

            // ゆるくターゲットの方向へ回転する
            Quaternion lookRotation = Quaternion.LookRotation(diff,transform.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);
        }

        //まっすぐ進む
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
        lineRenderer.SetPositions(linePoints.ToArray()); // オブジェクトの位置情報をセット
    }

    public void OnCollisionEnter(Collision collision)
    {
        IDamageable damageAble = collision.transform.GetComponent<IDamageable>();

        if (damageAble != null)
        {
            damageAble.Damage(attackData);

            //パーティクルを作成
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
