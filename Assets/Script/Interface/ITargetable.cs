using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
    //ターゲッティング可能な状態か（死んでたらターゲッティング出来ない、とか）
    public bool CanTarget();

    //カメラ外に出ているなら
    public bool IsVisible();
}
