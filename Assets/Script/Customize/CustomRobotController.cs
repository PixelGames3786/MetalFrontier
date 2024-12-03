using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 型エイリアスの定義
using WeaponSetDict = SerializableDictionary<LegacySettingData.WeaponSetPosi,UnityEngine.Transform>;

public class CustomRobotController : MonoBehaviour
{
    //メッシュとオブジェクト名を紐づける辞書
    public Dictionary<string,MeshFilter> AllChildMeshes = new Dictionary<string,MeshFilter>();

    //武器を配置する親オブジェクトとSetPosiを紐づける
    public WeaponSetDict WeaponSetDic = new();

    //実際にInstantiateしている武器のリスト
    private WeaponSetDict createdWeaponsObj = new WeaponSetDict() 
    {
        {LegacySettingData.WeaponSetPosi.LeftArm,null },
        {LegacySettingData.WeaponSetPosi.RightArm,null },
        {LegacySettingData.WeaponSetPosi.LeftShoulder,null },
        {LegacySettingData.WeaponSetPosi.RightShoulder,null },
    };

    // Start is called before the first frame update
    void Awake()
    {
        //自身の子供からMeshFilterコンポーネントを持つ子をすべて取得して辞書に登録
        MeshFilter[] meshFilters = transform.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.tag == "Weapon") continue;

            AllChildMeshes[meshFilter.name]= meshFilter;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //セーブデータのパーツ構成を本体に反映
    public void PartsSetReflect()
    {
        //データ取得
        LegacySettingData settingData = SaveDataManager.instance.saveData.settingData;

        //ボディパーツ反映
        //繰り返し処理用にリストを用意
        List<BodyPartsData> BodyParts = new List<BodyPartsData>();

        //全パーツのデータ取得
        foreach (KeyValuePair<BodyPartsData.PartsType,HavingItem> keyValue in settingData.PartsNumber)
        {
            BodyPartsData Data = (BodyPartsData)keyValue.Value.itemData;
            BodyParts.Add(Data);
        }

        foreach (BodyPartsData data in BodyParts)
        {
            foreach (KeyValuePair<string,Mesh> keyValue in data.ObjnameMeshPairs)
            {
                //子オブジェクトを検索
                //GameObject meshChangeObj = transform.FindFromAllChildren(keyValue.Key).gameObject;

                AllChildMeshes[keyValue.Key].GetComponent<MeshFilter>().mesh = keyValue.Value;
            }
        }

        //武器パーツ反映
        //パーツデータ取得
        foreach (KeyValuePair<LegacySettingData.WeaponSetPosi,HavingItem> keyValue in settingData.WeaponsNumber)
        {
            //もしも何も装備されていない部位だったら次に行く
            if (keyValue.Value == null)
            {
                //武器が既に生成されており、外すなどによって武器が外れた場合オブジェクトを消す処理
                if (createdWeaponsObj[keyValue.Key]!=null)
                {
                    GameObject destoryObj = createdWeaponsObj[keyValue.Key].gameObject;

                    createdWeaponsObj[keyValue.Key] = null;

                    Destroy(destoryObj);
                }
                continue;
            }

            WeaponPartsData data = (WeaponPartsData)keyValue.Value.itemData;
            GameObject weaponPrefab = data.setPosiWithPrefab[keyValue.Key];

            //同じ武器が既に配置されているなら行わない
            if (createdWeaponsObj[keyValue.Key]!=null && createdWeaponsObj[keyValue.Key].name==weaponPrefab.name)
            {
                continue;
            }


            //もしも装備が入れ替わっているならば　以前の武器を削除する
            if (createdWeaponsObj[keyValue.Key] != null && createdWeaponsObj[keyValue.Key].name != weaponPrefab.name)
            {
                GameObject destoryObj = createdWeaponsObj[keyValue.Key].gameObject;

                Destroy(destoryObj);
            }

            Transform weaponParent = WeaponSetDic[keyValue.Key];

            createdWeaponsObj[keyValue.Key] = Instantiate(weaponPrefab,weaponParent).transform;
            createdWeaponsObj[keyValue.Key].name= weaponPrefab.name;

            createdWeaponsObj[keyValue.Key].RecursiveSetLayer(LayerMask.NameToLayer("CustomizeRobot"));
        }

    }

    //全アウトラインを表示or非表示
    public void SetAllOutlines(bool active)
    {
        foreach (KeyValuePair<string,MeshFilter> keyValue in AllChildMeshes)
        {
            keyValue.Value.GetComponent<Outline>().enabled = active;
        }
    }

    //指定したメッシュのアウトラインを表示or非表示
    public void SetMeshOutline(string meshName,bool active)
    {
        AllChildMeshes[meshName].GetComponent<Outline>().enabled = active;
    }
}

