using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantMissionCondition : MissionCondition
{
    [SerializeField]
    private List<string> targetNames= new List<string>();
    public string playerName;

    public string targetName;

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
