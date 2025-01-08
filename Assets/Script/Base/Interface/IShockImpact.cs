using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//外部から衝撃を加えることが出来るインターフェース（ダメージを受けたときに吹っ飛ぶなど）
public interface IShockImpact
{
    public void AddImpact(Vector3 impactVel);
}
