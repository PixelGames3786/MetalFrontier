using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveDataManager>();
            }

            return _instance;
        }
    }

    public static SaveDataManager _instance;

    public SaveData saveData;

    // データのロードが完了したときに発生するイベント
    public event Action OnLoadComplete;

    private bool NewGameFlag = true;
    public bool isLoadComplete = false;

    //疑似シングルトン
    public void Awake()
    {
        if (instance!=this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        //セーブデータが存在しているかどうかチェック
        if (File.Exists(Application.persistentDataPath + "/SaveData/SaveData.sav"))
        {
            NewGameFlag = false;
        }
    }

    // Start is called before the first frame update
    async void Start()
    {
        await AsyncLoadSaveData();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async UniTask AsyncLoadSaveData()
    {
        await UniTask.Delay(1000);

        // データのロード処理（実際にはファイルを読み込む処理をここに記述）
        if (NewGameFlag)
        {
            saveData = new SaveData();
            saveData.DataInitialize();
        }
        else
        {
            saveData = SaveFileRead();
            saveData.settingData.RefleshPartsNumber();
            saveData.settingData.RefleshWeaponsNumber();
        }

        // ロード完了フラグを設定
        isLoadComplete = true;

        // データのロードが完了したことを通知
        OnLoadComplete?.Invoke();
    }

    public void SaveFileWrite()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/SaveData"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/SaveData");
        }

        string NormalPath = Application.persistentDataPath + "/SaveData/";

        string Path = NormalPath + "/SaveData.sav";

        //true 追記 false 上書き
        bool isAppend = false;
        string jsonData = "";

        //json.netを使用したセーブ方式
        print("セーブしまーす");

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        try
        {
            jsonData = JsonConvert.SerializeObject(saveData, settings);
            Debug.Log(jsonData);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Serialization failed: {ex.Message}");
        }

        //書き込み
        using (var fs = new StreamWriter(Path, isAppend, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            fs.Write(jsonData);
        }
    }

    public SaveData SaveFileRead()
    {
        SaveData data = null;

        string NormalPath = Application.persistentDataPath + "/SaveData/";

        string Path = NormalPath + "/SaveData.sav";

        //Json.netを使用したロード方式
        //ファイルから読み込み
        using (var fs = new StreamReader(Path, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            string LoadResult = fs.ReadToEnd();

            data = JsonConvert.DeserializeObject<SaveData>(LoadResult);
        }

        return data;
    }
}
