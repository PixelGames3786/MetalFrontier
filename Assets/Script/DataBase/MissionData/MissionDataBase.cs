using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

[CreateAssetMenu(fileName = "MissionDataBase", menuName = "DataBase/CreateMissionDataBase")]
public class MissionDataBase : ScriptableObject
{
    public List<MissionData> missionDataList = new List<MissionData>();

    //�A�C�e���ԍ�����A�C�e�����擾
    public MissionData GetMission(int missionNum)
    {
        return missionDataList.FirstOrDefault(mission => mission.missionNumber == missionNum);
    }

    //�A�C�e���ԍ��̃��X�g����A�C�e���̃��X�g�ɕϊ�
    public List<MissionData> GetMissionList(List<int> missionNumList)
    {
        List<MissionData> returnList = new List<MissionData>();

        foreach (int missionNum in missionNumList)
        {
            returnList.Add(GetMission(missionNum));
        }

        return returnList;
    }
}
