using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

public class ItemDataImporter : EditorWindow
{
    private TextAsset csvFile,statusCsvFile; // CSVファイルを指定するフィールド
    private TextAsset weaponCsvFile;

    private Vector2 scrollPosition; // スクロールの位置を管理する変数

    private List<BodyPartsData> itemList = new List<BodyPartsData>();
    private List<WeaponPartsData> weaponList = new List<WeaponPartsData>();

    [MenuItem("Tools/BodyPartsDataImporter")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataImporter>("ItemDataImporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Data from CSV", EditorStyles.boldLabel);

        // アセットのCSVファイルを指定
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        statusCsvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", statusCsvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import BodyParts CSV"))
        {
            if (csvFile != null)
            {
                ImportBodyPartsCSV(csvFile);
            }
            else
            {
                Debug.LogError("Please specify a CSV file.");
            }
        }
        if (GUILayout.Button("Import BodyParts Status CSV"))
        {
            if (statusCsvFile != null)
            {
                ImportBodyStatusCSV(statusCsvFile);
            }
            else
            {
                Debug.LogError("Please specify a CSV file.");
            }
        }

        // 作成されたアイテムのリストを表示
        GUILayout.Label("Imported Items", EditorStyles.boldLabel);

        // スクロールビューの開始
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));

        // スクロールビュー内に表示する内容
        foreach (var item in itemList)
        {
            GUILayout.Label(item.itemName + " (ID: " + item.ItemNumber + ")");
        }

        // スクロールビューの終了
        EditorGUILayout.EndScrollView();


        //アセットのCSVファイルを指定 武器
        weaponCsvFile = (TextAsset)EditorGUILayout.ObjectField("WeaponParts CSV File", weaponCsvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import WeaponParts CSV"))
        {
            if (weaponCsvFile != null)
            {
                ImportWeaponCSV(weaponCsvFile);
            }
            else
            {
                Debug.LogError("Please specify a CSV file.");
            }
        }

        // 作成されたアイテムのリストを表示
        GUILayout.Label("Imported Items", EditorStyles.boldLabel);

        foreach (var item in itemList)
        {
            GUILayout.Label(item.itemName + " (ID: " + item.ItemNumber + ")");
        }
    }

    // CSVのインポート処理
    private void ImportBodyPartsCSV(TextAsset csvFile)
    {
        itemList.Clear();
        string[] lines = csvFile.text.Split('\n'); // CSVの内容を読み込む

        // CSVの最初の行はヘッダーなのでスキップ
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            if (values.Length < 4) continue; // 不足しているデータは無視

            BodyPartsData newItem = CreateBodyData(values);
            itemList.Add(newItem);
        }
    }

    //ステータスCSVのインポート処理
    private void ImportBodyStatusCSV(TextAsset csvFile)
    {
        itemList.Clear();
        string[] lines = csvFile.text.Split('\n'); // CSVの内容を読み込む

        // CSVの最初の行はヘッダーなのでスキップ
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            UpdateBodyData(values);
        }
    }


    // CSVのインポート処理
    private void ImportWeaponCSV(TextAsset csvFile)
    {
        itemList.Clear();
        string[] lines = csvFile.text.Split('\n'); // CSVの内容を読み込む

        // CSVの最初の行はヘッダーなのでスキップ
        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');

            WeaponPartsData newItem = CreateWeaponData(values);
            weaponList.Add(newItem);
        }
    }

    //BodyPartsDataの作成
    private BodyPartsData CreateBodyData(string[] values)
    {
        int itemNum = int.Parse(values[0]);
        string fileName = values[1];
        string itemName = values[2];
        string itemDescription = values[3];
        BodyPartsData.PartsType type= (BodyPartsData.PartsType)Enum.Parse(typeof(BodyPartsData.PartsType), values[4]);
        int itemPrice = int.Parse(values[5]);
        string modelName= values[6].Trim();

        BodyPartsData item = ScriptableObject.CreateInstance<BodyPartsData>();


        item.itemType = ItemData.ItemType.BodyParts;
        item.ItemNumber = itemNum;
        item.itemName = itemName;
        item.Description = itemDescription;
        item.Type = type;
        item.price = itemPrice;
        item.ObjnameMeshPairs = new SerializableDictionary<string, Mesh>();
        item.itemType = ItemData.ItemType.BodyParts;

        Mesh targetMesh = null;
        string findMeshName = "";

        //種類ごとにメッシュなどを登録
        switch (type)
        {
            case BodyPartsData.PartsType.Head:

                targetMesh = FindAssetByName<Mesh>("Assets/Models", fileName);
                item.ObjnameMeshPairs.Add("Head_Mesh",targetMesh);

                break;

            case BodyPartsData.PartsType.Body:

                findMeshName = modelName + "_Body";

                if ("NR-1" == modelName.Trim()) Debug.Log("同一の文字列");

                targetMesh = FindAssetByName<Mesh>("Assets/Models", findMeshName);
                item.ObjnameMeshPairs.Add("Body_Mesh", targetMesh);

                break;

            case BodyPartsData.PartsType.LeftArm:

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName+"_UpperArm_L");
                item.ObjnameMeshPairs.Add("UpperArm_L_Mesh", targetMesh);

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName + "_LowerArm_L");
                item.ObjnameMeshPairs.Add("LowerArm_L_Mesh", targetMesh);

                break;

            case BodyPartsData.PartsType.RightArm:

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName + "_UpperArm_R");
                item.ObjnameMeshPairs.Add("UpperArm_R_Mesh", targetMesh);

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName + "_LowerArm_R");
                item.ObjnameMeshPairs.Add("LowerArm_R_Mesh", targetMesh);

                break;

            case BodyPartsData.PartsType.Leg:

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName + "_Hips");
                item.ObjnameMeshPairs.Add("Hips_Mesh", targetMesh);

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName+"_UpperLeg_L");
                item.ObjnameMeshPairs.Add("UpperLeg_L_Mesh", targetMesh);

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName + "_LowerLeg_L");
                item.ObjnameMeshPairs.Add("LowerLeg_L_Mesh", targetMesh);

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName + "_UpperLeg_R");
                item.ObjnameMeshPairs.Add("UpperLeg_R_Mesh", targetMesh);

                targetMesh = FindAssetByName<Mesh>("Assets/Models", modelName + "_LowerLeg_R");
                item.ObjnameMeshPairs.Add("LowerLeg_R_Mesh", targetMesh);

                break;
        }

        // Assetsフォルダに保存
        string path = "Assets/DataBase/BodyPartsData/" + fileName + ".asset";

        // 同名のアセットが存在する場合は削除
        if (AssetDatabase.LoadAssetAtPath<ItemData>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(item, path);
        AssetDatabase.SaveAssets();

        return item;
    }

    private void UpdateBodyData(string[] values)
    {
        int itemNum = int.Parse(values[0]);
        string fileName = values[1];
        string itemName = values[2];
        float hp = float.Parse(values[3]);
        float normalSpeed = float.Parse(values[4]);
        float boostSpeed = float.Parse(values[5]);
        float jumpForce = float.Parse(values[6]);
        float riseForce = float.Parse(values[7]);
        float shotAccuracy = float.Parse(values[8]);
        float boostAmount = float.Parse(values[9]);
        float boostUseRate = float.Parse(values[10]);
        float boostRecoverRate = float.Parse(values[11]);
        float physicRes = float.Parse(values[12]);
        float beamRes = float.Parse(values[13]);
        float normalMaxVel = float.Parse(values[14]);
        float boostMaxVel = float.Parse(values[15]);
        

        string assetPath= "Assets/DataBase/BodyPartsData/" + fileName + ".asset";

        // 既存アセットをロード
        BodyPartsData item = AssetDatabase.LoadAssetAtPath<BodyPartsData>(assetPath);

        if (item == null)
        {
            Debug.LogError($"No ItemData asset found at path: {assetPath}");
            return;
        }

        // プロパティを更新
        item.hpFactor = hp;
        item.moveSpeed = normalSpeed;
        item.boostSpeed = boostSpeed;
        item.jumpForce = jumpForce;
        item.riseForce = riseForce;
        item.shotAccuracy = shotAccuracy;
        item.boostAmout = boostAmount;
        item.boostUseRate = boostUseRate;
        item.boostRecoverRate = boostRecoverRate;
        item.physicalRes = physicRes;
        item.beamRes = beamRes;
        item.maxVel = new Vector3(normalMaxVel,normalMaxVel,normalMaxVel);
        item.boostMaxVel = new Vector3(boostMaxVel,boostMaxVel,boostMaxVel);
        item.itemType = ItemData.ItemType.WeaponParts;

        // アセットを変更としてマークして保存
        EditorUtility.SetDirty(item);
        AssetDatabase.SaveAssets();
    }

    //WeaponPartsData作成
    private WeaponPartsData CreateWeaponData(string[] values)
    {
        int itemNum = int.Parse(values[0]);
        string fileName = values[1];
        string itemName = values[2];
        string modelNumber= values[3];
        string itemDescription = values[4];
        WeaponPartsData.WeaponType weaponType = (WeaponPartsData.WeaponType)Enum.Parse(typeof(WeaponPartsData.WeaponType), values[5]);
        WeaponPartsData.SetType setType = (WeaponPartsData.SetType)Enum.Parse(typeof(WeaponPartsData.SetType), values[6]);
        WeaponPartsData.AttackType attackType = (WeaponPartsData.AttackType)Enum.Parse(typeof(WeaponPartsData.AttackType), values[7]);
        int itemPrice = int.Parse(values[8]);
        float bulletSpeed = float.Parse(values[9]);
        float damage = float.Parse(values[10]);
        float shotBlur= float.Parse(values[11]);
        float useInterval = float.Parse(values[12]);
        int defaultBullet = int.Parse(values[13]);


        WeaponPartsData item = ScriptableObject.CreateInstance<WeaponPartsData>();

        item.ItemNumber = itemNum;
        item.itemName = itemName;
        item.modelName= modelNumber;
        item.Description = itemDescription;
        item.price = itemPrice;
        item.weaponType = weaponType;
        item.setType = setType;
        item.attackType= attackType;
        item.bulletSpeed = bulletSpeed;
        item.damage= damage;
        item.shotBlur = shotBlur;
        item.useInterval = useInterval;
        item.defaultBullet = defaultBullet;
        item.itemType = ItemData.ItemType.WeaponParts;

        item.setPosiWithPrefab =new SerializableDictionary<LegacySettingData.WeaponSetPosi, GameObject>();

        //セット箇所とプレハブを取得しセット　1箇所目
        string[] setPosiPrefab = values[14].Split('/');

        GameObject targetObj = null;
        LegacySettingData.WeaponSetPosi weaponSetPosi= (LegacySettingData.WeaponSetPosi)Enum.Parse(typeof(LegacySettingData.WeaponSetPosi), setPosiPrefab[0]);

        targetObj = FindAssetByName<GameObject>("Assets/Prefab/Weapon", setPosiPrefab[1].Trim());
        item.setPosiWithPrefab.Add(weaponSetPosi,targetObj);

        //セット箇所とプレハブを取得しセット　2箇所目
        setPosiPrefab = values[15].Split('/');

        weaponSetPosi = (LegacySettingData.WeaponSetPosi)Enum.Parse(typeof(LegacySettingData.WeaponSetPosi), setPosiPrefab[0]);

        targetObj = FindAssetByName<GameObject>("Assets/Prefab/Weapon", setPosiPrefab[1].Trim());
        item.setPosiWithPrefab.Add(weaponSetPosi, targetObj);

        // Assetsフォルダに保存
        string path = "Assets/DataBase/WeaponPartsData/" + fileName + ".asset";

        // 同名のアセットが存在する場合は削除
        if (AssetDatabase.LoadAssetAtPath<ItemData>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(item, path);
        AssetDatabase.SaveAssets();

        return item;
    }



    private T FindAssetByName<T>(string rootFolderPath, string assetName) where T : UnityEngine.Object
    {
        // 指定フォルダとその子フォルダを含むすべてのアセットを検索
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { rootFolderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            // 名前が一致するか確認
            if (asset != null && asset.name == assetName)
            {
                return asset;
            }
        }

        Debug.LogWarning($"Asset of type {typeof(T).Name} with name '{assetName}' not found in folder '{rootFolderPath}' or its subfolders.");
        return null; // 該当するアセットが見つからない場合
    }
}
