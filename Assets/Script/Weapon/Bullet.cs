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

            //パーティクルを作成
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
            if (transform.parent.gameObject != null)
            {
                Destroy(transform.parent.gameObject);
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルされた場合の処理
            Debug.Log("オブジェクト削除がキャンセルされました。");
        }
    }
}
