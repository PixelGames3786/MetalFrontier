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

    protected List<string> defeatedUnits=new List<string>();

    public Action onConditionChange;

    public AudioData bgm;

    public bool isMissionClear { get; protected set; }
    public bool isMissionEnd { get; protected set; } //ミッション成功でも失敗でも、とにかく終わっているかどうか

    //開始前にセットアップを行う処理
    public virtual void MissionSetUp()
    {

    }

    //ミッション開始時に行う処理(黒幕がフェードアウトしたあとの処理)
    public virtual void MissionStart()
    {
        AudioManager.instance.PlayBGM(bgm);
    }

    //ミッション終了時に行う処理（フェードアウト開始前に行う処理）
    public virtual void MissionEnd()
    {
        AudioManager.instance.StopBGM(bgm);
    }

    public abstract bool ClearConditionCheck();

    public abstract bool FailConditionCheck();

    public void MissionClear()
    {
        isMissionClear = true;
    }

    public void AddDefeatedUnit(string name)
    {
        Debug.Log(name+"を倒した！");

        defeatedUnits.Add(name);

        onConditionChange?.Invoke();
    }
}
