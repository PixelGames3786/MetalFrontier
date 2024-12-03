using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponSetDict = SerializableDictionary<LegacySettingData.WeaponSetPosi,UnityEngine.Transform>;

public class RobotSetUpController : MonoBehaviour
{
    //メッシュとオブジェクト名を紐づける辞書
    private Dictionary<string,MeshFilter> AllChildMeshes = new Dictionary<string,MeshFilter>();

    //武器を配置する親オブジェクトとSetPosiを紐づける
    public WeaponSetDict WeaponSetDic = new();

    //実際にInstantiateしている武器のリスト
    public Dictionary<LegacySettingData.WeaponSetPosi,Transform> createdWeaponsObj = new Dictionary<LegacySettingData.WeaponSetPosi, Transform>
    {
        {LegacySettingData.WeaponSetPosi.LeftArm,null },
        {LegacySettingData.WeaponSetPosi.RightArm,null },
        {LegacySettingData.WeaponSetPosi.LeftShoulder,null },
        {LegacySettingData.WeaponSetPosi.RightShoulder,null },
    };

    private RobotController controller;

    // Start is called before the first frame update
    void Start()
    {
        controller= GetComponent<RobotController>();

        //自身の子供からMeshFilterコンポーネントを持つ子をすべて取得して辞書に登録
        MeshFilter[] meshFilters = transform.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.tag == "Weapon") continue;

            AllChildMeshes[meshFilter.name] = meshFilter;
        }
    }

    public void LegacySetUp(List<BodyPartsData> bodyParts,Dictionary<LegacySettingData.WeaponSetPosi,WeaponPartsData> weaponParts)
    {
        //一致する名前のメッシュを登録
        foreach (BodyPartsData data in bodyParts)
        {
            foreach (KeyValuePair<string, Mesh> keyValue in data.ObjnameMeshPairs)
            {
                AllChildMeshes[keyValue.Key].GetComponent<MeshFilter>().mesh = keyValue.Value;
            }
        }

        //武器パーツ反映
        //パーツデータ取得
        foreach (KeyValuePair<LegacySettingData.WeaponSetPosi, WeaponPartsData> keyValue in weaponParts)
        {
            WeaponPartsData data = keyValue.Value;
            GameObject weaponPrefab = data.setPosiWithPrefab[keyValue.Key];

            Transform weaponParent = WeaponSetDic[keyValue.Key];

            //武器に装備位置を登録
            createdWeaponsObj[keyValue.Key] = Instantiate(weaponPrefab, weaponParent).transform;
            createdWeaponsObj[keyValue.Key].GetComponent<WeaponBase>().weaponPosition = keyValue.Key;
            createdWeaponsObj[keyValue.Key].GetComponent<WeaponBase>().legacyStatus = GetComponent<RobotStatusController>().nowStatus;

            createdWeaponsObj[keyValue.Key].GetComponent<WeaponBase>().controller = controller;
        }
    }

}
