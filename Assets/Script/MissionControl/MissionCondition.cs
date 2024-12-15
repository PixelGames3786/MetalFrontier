using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MissionCondition : MonoBehaviour
{
    public MissionData missionData;

    //�|�����G�̖��O��ۑ����郊�X�g
    protected List<string> defeatedEnemys=new List<string>();

    //���񂾖����̖��O��ۑ����郊�X�g
    protected List<string> defeatedAlly=new List<string>();

    protected List<string> defeatedLegacys=new List<string>();

    public Action onConditionChange;

    public bool isMissionClear { get; protected set; }
    public bool isMissionEnd { get; protected set; } //�~�b�V���������ł����s�ł��A�Ƃɂ����I����Ă��邩�ǂ���


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
