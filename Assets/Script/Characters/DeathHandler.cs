using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;

//ミッションマネージャーに死亡したことなどを伝達するためのクラス
public class DeathHandler : MonoBehaviour
{
    //デリゲート
    public Action<string> OnDeathWithName;

    public void DeathInvoke()
    {
        OnDeathWithName?.Invoke(gameObject.name);
    }
}
