using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCondition : MissionCondition
{
    private NovelWindowManager novelWindow;

    [SerializeField]
    private List<string> targetNames= new List<string>();
    public string playerName;

    public string targetName;

    public float textWaitTime,textWaitElapsed;

    private bool isMission,isTextWaiting;

    public void Update()
    {
        if (isMission && isTextWaiting)
        {
            textWaitElapsed += Time.deltaTime;

            if (textWaitElapsed>=textWaitTime)
            {
                novelWindow.ReadmoreMessage();
                textWaitElapsed = 0;

                isTextWaiting = false;
            }
        }
    }

    public override void MissionStart()
    {
        base.MissionStart();

        novelWindow = FindObjectOfType<NovelWindowManager>();
        isMission = true;

        novelWindow.StartMessage(0);
        novelWindow.onTextShowEnd += StartTextWait;

        isMissionClear = true; //最初からクリアフラグを立てておく
    }

    private void StartTextWait()
    {
        isTextWaiting = true;
    }

    public override bool ClearConditionCheck()
    {
        bool isClear = true;

        foreach (var name in targetNames)
        {
            if (!defeatedUnits.Contains(name))
            {
                isClear= false;

                break;
            }
        }

        isMissionClear = true;
        isMissionEnd = true;

        return isClear;
    }

    public override bool FailConditionCheck()
    {
        if (defeatedUnits.Contains(playerName))
        {
            isMissionClear = false;
            isMissionEnd = true;

            return true;
        }
        else
        {
            return false;
        }
    }

}
