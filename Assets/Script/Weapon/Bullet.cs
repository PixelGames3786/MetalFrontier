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

    public Rigidbody rb;

    public Collider bulletCollider;

    public GameObject destroyObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        Transform colTrans = collision.collider.transform;
        IDamageable damageAble = colTrans.GetComponent<IDamageable>();

        if (damageAble == null)
        {
            cts.Cancel();
            cts.Dispose();

            Destroy(destroyObj);

            return;
        }

        if (damageAble.CanHit())
        {
            damageAble.Damage(attackData);

            //パーティクルを作成
            GameObject particle = Instantiate(particlePrefab);

            particle.transform.position = collision.contacts[0].point;

            cts.Cancel();
            cts.Dispose();

            Destroy(destroyObj);
        }
    }

    public void Shot(Vector3 shotPower)
    {
        rb.AddForce(shotPower, ForceMode.Impulse);

        DestroyObjectAfterDelay(lifeTime);
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
            if (destroyObj != null)
            {
                Destroy(destroyObj);
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合の処理
            //Debug.Log("オブジェクト削除がキャンセルされました。");
        }
    }
}
