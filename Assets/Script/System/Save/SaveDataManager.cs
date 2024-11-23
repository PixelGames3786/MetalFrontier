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

    // �f�[�^�̃��[�h�����������Ƃ��ɔ�������C�x���g
    public event Action OnLoadComplete;

    private bool NewGameFlag = true;
    public bool isLoadComplete = false;

    //�^���V���O���g��
    public void Awake()
    {
        if (instance!=this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        //�Z�[�u�f�[�^�����݂��Ă��邩�ǂ����`�F�b�N
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

        // �f�[�^�̃��[�h�����i���ۂɂ̓t�@�C����ǂݍ��ޏ����������ɋL�q�j
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

        // ���[�h�����t���O��ݒ�
        isLoadComplete = true;

        // �f�[�^�̃��[�h�������������Ƃ�ʒm
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

        //true �ǋL false �㏑��
        bool isAppend = false;
        string jsonData = "";

        //json.net���g�p�����Z�[�u����
        print("�Z�[�u���܁[��");

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

        //��������
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

        //Json.net���g�p�������[�h����
        //�t�@�C������ǂݍ���
        using (var fs = new StreamReader(Path, System.Text.Encoding.GetEncoding("UTF-8")))
        {
            string LoadResult = fs.ReadToEnd();

            data = JsonConvert.DeserializeObject<SaveData>(LoadResult);
        }

        return data;
    }
}
