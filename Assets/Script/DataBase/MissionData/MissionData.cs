using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "MissionDataBase", menuName = "DataBase/CreateMissionData")]
public class MissionData : ScriptableObject
{
    //�ʂ��ԍ�
    public int missionNumber;

    public string missionName;

    [TextArea]
    public string description;

    [TextArea]
    public string target;

    //�I�����ꂽ�Ƃ��ɑJ�ڂ���V�[����
    public string sceneName;

    //�N���A���ɓ���ł�����z
    public int clearGetCol;

    public List<ItemData> clearGetItems=new List<ItemData>();
}
