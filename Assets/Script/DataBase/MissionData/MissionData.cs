using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "MissionDataBase", menuName = "DataBase/CreateMissionData")]
public class MissionData : ScriptableObject
{
    //通し番号
    public int missionNumber;

    public string missionName;

    [TextArea]
    public string description;

    [TextArea]
    public string target;

    //選択されたときに遷移するシーン名
    public string sceneName;

    //クリア時に入手できる金額
    public int clearGetCol;

    public List<ItemData> clearGetItems=new List<ItemData>();
}
