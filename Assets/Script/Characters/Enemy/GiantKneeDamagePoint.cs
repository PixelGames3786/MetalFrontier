using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GiantKneeDamagePoint : DamagePoint
{
    // UnityEventを使用して、インスペクターから設定可能にする
    [System.Serializable]
    public class StunEvent : UnityEvent { }

    //親にダメージを与えるためのデリゲート
    public StunEvent parentStunFunc;

    public bool isStuned; //スタンしたかどうか
    public float stunHP; //0になったら片膝をつくHP

    public float physicalRes, beamRes;

    // Start is called before the first frame update
    void Start()
    {
        //全メッシュを取得
        allMeshes = GetComponentsInChildren<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Damage(AttackData attack)
    {
        //攻撃タイプと耐性を考慮してダメージを決定
        float damage = attack.damage;

        switch (attack.type)
        {
            case WeaponPartsData.AttackType.Physicial:

                damage = damage / (1 + physicalRes);

                break;

            case WeaponPartsData.AttackType.Beam:

                damage = damage / (1 + beamRes);

                break;
        }

        stunHP -= damage;

        //スタン処理
        if (stunHP <= 0 && !isStuned)
        {
            isStuned = true; //一度スタンしたなら二度はしない
            parentStunFunc?.Invoke();
        }

        parentDamageFunc?.Invoke(attack);
    }
}
