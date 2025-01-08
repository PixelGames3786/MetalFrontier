using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    [SerializeField]
    public AttackData attackData;

    public bool isImpact; //当たった時に衝撃を与えるか

    public float impactPower;

    public List<string> targetTag; //ダメージを与える相手のtag

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!targetTag.Contains(other.tag)) return;

        Transform colTrans = other.transform;
        IDamageable damageAble = colTrans.GetComponent<IDamageable>();

        if (damageAble == null) return;
        if (!damageAble.CanHit()) return;

        damageAble.Damage(attackData);


        IShockImpact shockImpact = colTrans.GetComponent<IShockImpact>();

        if (shockImpact == null || !isImpact) return;//衝撃を与えられるなら与える

        // 衝突位置を取得
        Vector3 hitPosition = other.ClosestPoint(transform.position);
        // 衝撃の方向を計算
        Vector3 forceDirection = (hitPosition - transform.position).normalized;

        // 衝突した方向に力を加える
        shockImpact.AddImpact(forceDirection * impactPower);
    }
}
