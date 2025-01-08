using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine.UIElements;
using UnityEngine.InputSystem.XR;


public class MissileBullet : MonoBehaviour,IDamageable,ITargetable
{
    public bool CanDamage = false;
    public bool CanTarget = false;

    [SerializeField]
    private float lifeTime;

    private CancellationTokenSource cts;

    [SerializeField]
    public AttackData attackData;

    private Rigidbody rb;

    public Transform target;
    public float torqueRatio;
    public float speed,firstTime,rotateSpeed;

    public float HP;
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

            if (linePoints.Count >= 3)
            {
                linePoints.RemoveAt(0);
                linePoints.RemoveAt(1);
            }

            if (linePoints.Count<=1)
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
        Transform colTrans = collision.collider.transform;
        IDamageable damageAble = colTrans.GetComponent<IDamageable>();

        print(colTrans.name);

        if (damageAble == null)
        {
            cts.Cancel();
            cts.Dispose();

            Destroy(bulletObj);

            return;
        }

        print(damageAble.CanHit());


        if (damageAble.CanHit())
        {
            damageAble.Damage(attackData);

            //パーティクルを作成
            GameObject particle = Instantiate(particlePrefab);

            particle.transform.position = collision.contacts[0].point;

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

    public void Damage(AttackData attack)
    {
        //攻撃タイプと耐性を考慮してダメージを決定
        float damage = attack.damage;

        HP -= damage;

        //死亡処理
        if (HP <= 0)
        {
            CanTarget = false;
            CanDamage = false;

            //パーティクルを作成
            GameObject particle = Instantiate(particlePrefab);

            particle.transform.position = transform.position;

            cts.Cancel();
            cts.Dispose();

            Destroy(bulletObj);
        }
    }

    bool IDamageable.CanHit()
    {
        return CanDamage;
    }

    void IDamageable.Damage(AttackData attack)
    {
        //攻撃タイプと耐性を考慮してダメージを決定
        float damage = attack.damage;

        HP-=damage;

        //死亡処理
        if (HP <= 0)
        {
            CanTarget = false;
            CanDamage = false;

            //パーティクルを作成
            GameObject particle = Instantiate(particlePrefab);

            particle.transform.position = transform.position;

            cts.Cancel();
            cts.Dispose();

            Destroy(bulletObj);
        }
    }

    bool ITargetable.CanTarget()
    {
        return CanTarget;
    }

    bool ITargetable.IsVisible()
    {
        return bulletCol.GetComponent<MeshRenderer>().isVisible;
    }
}
