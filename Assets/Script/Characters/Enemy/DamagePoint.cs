using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamagePoint : MonoBehaviour,IDamageable,ITargetable
{
    // UnityEventを使用して、インスペクターから設定可能にする
    [System.Serializable]
    public class AttackEvent : UnityEvent<AttackData> { }

    //親にダメージを与えるためのデリゲート
    public AttackEvent parentDamageFunc;

    public bool canTarget = true;
    public bool canDamage=true;

    protected MeshRenderer[] allMeshes; //カメラ外に出ているか判定するため、全メッシュを保存しておく

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
    public virtual bool CanHit()
    {
        return canDamage;
    }

    public virtual bool CanTarget()
    {
        return canTarget;
    }

    public virtual void Damage(AttackData attack)
    {
        parentDamageFunc?.Invoke(attack);
    }

    public virtual bool IsVisible()
    {
        if (allMeshes == null) return false;

        foreach (MeshRenderer renderer in allMeshes)
        {
            if (renderer == null) continue;
            if (renderer.gameObject.layer == LayerMask.NameToLayer("MiniMap")) continue;

            if (renderer.isVisible)
            {
                return true;
            }
        }

        return false;
    }
}
