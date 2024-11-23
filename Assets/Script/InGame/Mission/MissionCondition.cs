using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MissionCondition : MonoBehaviour
{
    public MissionData missionData;

    //倒した敵の名前を保存するリスト
    protected List<string> defeatedEnemys=new List<string>();

    //死んだ味方の名前を保存するリスト
    protected List<string> defeatedAlly=new List<string>();

    protected List<string> defeatedLegacys=new List<string>();

    public Action onConditionChange;

    public bool isMissionClear { get; protected set; }
    public bool isMissionEnd { get; protected set; } //ミッション成功でも失敗でも、とにかく終わっているかどうか


    public abstract bool ClearConditionCheck();

    public abstract bool FailConditionCheck();

    public void MissionClear()
    {
        isMissionClear = true;
    }

    public void AddDefeatedLegacy(string name)
    {
        defeatedLegacys.Add(name);

        onConditionChange?.Invoke();
    }
}
