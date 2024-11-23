using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using DG.Tweening;
using static UnityEditor.Progress;

public class CustomizeUIController : MonoBehaviour
{
    //ステートで状態を管理する

    //シーンが読み込まれた際に、シーンのセットアップを行うステート
    public class SetUpState : UIControllerState
    {
        //コンストラクタ　初期化
        public SetUpState(CustomizeUIController controller)
        {
            State = CustomizeUIState.SetUp;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

        }

        //ステートに入った際に、セットアップを行う
        public override void OnEnter()
        {
            //もしもセーブデータのロードが済んでいるのなら
            if (SaveDataManager.instance.isLoadComplete)
            {
                SetUp();
            }
            else
            {
                //終わるまで待つ
                SaveDataManager.instance.OnLoadComplete += SetUp;
            }
        }

        private void SetUp()
        {
            //自レガシーオブジェクト生成
            GameObject legacyObj = Instantiate(uiController.legacyCustomPrefab);
            uiController.robotControl = legacyObj.GetComponent<CustomRobotController>();

            uiController.robotControl.PartsSetReflect();

            uiController.curtainCanvas.DOFade(0f, 0.5f).OnComplete(() => 
            { 
                uiController.curtainCanvas.gameObject.SetActive(false);
                uiController.StateTranstion(CustomizeUIState.WaitButtonSelect);
            });
        }

    }

    //待機ステート
    public class WaitSelectState : UIControllerState
    {
        //コンストラクタ　初期化
        public WaitSelectState(CustomizeUIController controller)
        {
            State = CustomizeUIState.WaitButtonSelect;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
                {"OpenMissionScroll",OpenMissionScroll }
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
                {"OpenBodySelectScroll",OpenBodySelectScroll },
                {"OpenWeaponSelectScroll",OpenWeaponSelectScroll },
            };

        }

        public void OpenMissionScroll()
        {
            //ボタンを非表示にしスクロールビューを表示　将来的にはDOTweenで動かしたい
            uiController.ButtonsParent.SetActive(false);

            uiController.missionScroll.InitializeUI(SaveDataManager.instance.saveData.missionDataList);

            uiController.missionCanvas.gameObject.SetActive(true);
            uiController.missionCanvas.DOFade(1f, 0.5f).OnComplete(() => 
            {
                //状態遷移
                uiController.StateTranstion(CustomizeUIState.MissionScrollView);
            });
        }

        public void OpenBodySelectScroll(object[] args)
        {
            //object型で渡された引数を変換する
            BodyPartsData.PartsType type = (BodyPartsData.PartsType)args[0];


            //ボタンを非表示にしスクロールビューを表示　将来的にはDOTweenで動かしたい
            uiController.ButtonsParent.SetActive(false);

            //所持パーツから指定した種類のパーツを抜き出しそれでスクロールビューを構成する
            SaveData saveData = SaveDataManager.instance.saveData;

            List<HavingItem> ItemList = saveData.PartsDataList.Where(parts => parts.itemData is BodyPartsData bodyPartData && bodyPartData.Type == type).ToList();
            List<BodyPartsData> TypeDataList = ItemList.Select(parts => (BodyPartsData)parts.itemData).ToList();

            uiController.partsScroll.InitializeUI(TypeDataList, ItemList);
            uiController.partsScroll.gameObject.SetActive(true);


            //選択中のメッシュのアウトラインを表示
            //現在装備している選択した種類のデータを取得
            uiController.robotControl.SetAllOutlines(false);

            BodyPartsData partsData = (BodyPartsData)saveData.settingData.PartsNumber[type].itemData;

            foreach (KeyValuePair<string, Mesh> keyValue in partsData.ObjnameMeshPairs)
            {
                uiController.robotControl.SetMeshOutline(keyValue.Key, true);
            }

            //状態遷移
            uiController.StateTranstion(CustomizeUIState.OpenedScrollView);
        }

        public void OpenWeaponSelectScroll(object[] args)
        {
            //object型で渡された引数を変換する
            WeaponPartsData.SetType type = (WeaponPartsData.SetType)args[0];
            LegacySettingData.WeaponSetPosi setPosi = (LegacySettingData.WeaponSetPosi)args[1];


            //ボタンを非表示にしスクロールビューを表示　将来的にはDOTweenで動かしたい
            uiController.ButtonsParent.SetActive(false);

            //所持パーツから指定した種類のパーツを抜き出しそれでスクロールビューを構成する
            SaveData saveData = SaveDataManager.instance.saveData;
            List<HavingItem> ItemList = saveData.WeaponsDataList.Where(parts => parts.itemData is WeaponPartsData weaponData && weaponData.setType == type).ToList();
            List<WeaponPartsData> TypeDataList = ItemList.Select(parts => (WeaponPartsData)parts.itemData).ToList();

            uiController.partsScroll.InitializeUI(TypeDataList, ItemList);
            uiController.partsScroll.gameObject.SetActive(true);

            uiController.partsScroll.setPosi = setPosi;


            //選択中のメッシュのアウトラインを表示
            //現在装備している選択した種類のデータを取得
            /*
            uiController.robotControl.SetAllOutlines(false);

            BodyPartsData partsData = (BodyPartsData)ItemDataBaseController.instance.itemDataBase.GetItem(saveData.settingData.PartsNumber[type]);

            foreach (KeyValuePair<string, Mesh> keyValue in partsData.ObjnameMeshPairs)
            {
                uiController.robotControl.SetMeshOutline(keyValue.Key, true);
            }

            */

            //状態遷移
            uiController.StateTranstion(CustomizeUIState.OpenedScrollView);
        }

    }

    //スクロールビューでの選択待ちステート
    public class OpenedScrollViewState : UIControllerState
    {
        //コンストラクタ　初期化
        public OpenedScrollViewState(CustomizeUIController controller)
        {
            State = CustomizeUIState.OpenedScrollView;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {{"CloseScrollView",CloseScrollView }, };
        }

        public void CloseScrollView()
        {
            //ボタンを表示しスクロールビューを非表示
            uiController.partsScroll.gameObject.SetActive(false);

            uiController.ButtonsParent.SetActive(true);

            //アウトラインを非表示に
            uiController.robotControl.SetAllOutlines(false);

            //状態遷移
            uiController.StateTranstion(CustomizeUIState.WaitButtonSelect);
        }
    }

    //ミッションスクロールビューでの選択待ちステート
    public class MissionScrollViewState : UIControllerState
    {
        //コンストラクタ　初期化
        public MissionScrollViewState(CustomizeUIController controller)
        {
            State = CustomizeUIState.MissionScrollView;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
                {"BackButton",BackButton }
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
                { "SelectMission",SelectMission},
            };
        }

        //ミッションが選択されたとき
        public void SelectMission(object[] args)
        {
            string sceneName = (string)args[0];

            uiController.curtainCanvas.gameObject.SetActive(true);
            uiController.curtainCanvas.DOFade(1f, 1f).OnComplete(() => 
            {
                SceneChangeManager.instance.StartCoroutine("SceneTransition", sceneName);
            });
        }

        //パーツ選択に戻る
        public void BackButton()
        {
            //ボタンを非表示にしスクロールビューを表示　将来的にはDOTweenで動かしたい
            uiController.ButtonsParent.SetActive(true);

            uiController.missionCanvas.DOFade(0f, 0.5f).OnComplete(() =>
            {
                uiController.missionCanvas.gameObject.SetActive(false);

                //状態遷移
                uiController.StateTranstion(CustomizeUIState.WaitButtonSelect);
            });
        }
    }

    public CanvasGroup curtainCanvas;

    public CustomRobotController robotControl;

    private List<UIControllerState> States = new List<UIControllerState>();

    private UIControllerState nowState;

    public GameObject legacyCustomPrefab;

    public GameObject ButtonsParent;
    public PartsScrollView partsScroll;
    public MissionScrollView missionScroll;
    public CanvasGroup missionCanvas;

    public LegacyStatusUI statusUI;

    public ShopUIController shopUIController;

    // Start is called before the first frame update
    void Start()
    {
        States.Add(new SetUpState(this));
        States.Add(new WaitSelectState(this));
        States.Add(new OpenedScrollViewState(this));
        States.Add(new MissionScrollViewState(this));

        nowState = States[0];

        nowState.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        nowState.OnUpdate();
    }

    //ステートの関数を呼び出す（引数なし）
    public void CallStateFunc(string funcName)
    {
        nowState.CallFunc(funcName);
    }

    //ステートの関数を呼び出す（引数あり）
    public void CallStateFuncArg(string funcName, object[] args)
    {
        nowState.CallFuncArg(funcName, args);
    }

    //レガシーパーツの変更
    public void LegacyPartsChange(BodyPartsData.PartsType type, int partsNum, HavingItem having)
    {
        SaveDataManager.instance.saveData.LegacyPartsChange(type, having);

        robotControl.PartsSetReflect();

        //ステータスに反映
        StatusUIDataReflesh();
    }

    //武器パーツの変更
    public void WeaponPartsChange(LegacySettingData.WeaponSetPosi posi, int partsNum, HavingItem having)
    {
        SaveDataManager.instance.saveData.WeaponPartsChange(posi, having);

        robotControl.PartsSetReflect();
    }

    //武器パーツの取り外し
    public void WeaponPartsRemove(LegacySettingData.WeaponSetPosi posi, int partsNum, HavingItem having)
    {
        SaveDataManager.instance.saveData.WeaponPartsRemove(posi, having);

        robotControl.PartsSetReflect();
    }

    //ステートの切り替え
    public void StateTranstion(UIControllerState.CustomizeUIState transitState)
    {
        nowState.OnExit();

        UIControllerState newState = States.First(state => state.State == transitState);

        //ヌルチェ
        if (newState == null) throw new System.Exception("遷移するステートがないらしいよ");

        nowState = newState;

        nowState.OnEnter();
    }

    //保存
    public void SaveButtonOnClick()
    {
        SaveDataManager.instance.SaveFileWrite();
    }

    //ステータス表示
    public void StatusButtonOnClick()
    {
        if (statusUI.gameObject.activeSelf)
        {
            statusUI.gameObject.SetActive(false);
            
        }
        else
        {
            StatusUIDataReflesh();

            statusUI.gameObject.SetActive(true);
        }
    }

    //ステータスUIデータ更新
    public void StatusUIDataReflesh()
    {
        //データ取得
        LegacySettingData settingData = SaveDataManager.instance.saveData.settingData;

        //ボディパーツ反映
        List<BodyPartsData> bodyParts = new List<BodyPartsData>();
        Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData> weaponParts = new Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData>();

        //全パーツのデータ取得
        foreach (KeyValuePair<BodyPartsData.PartsType, HavingItem> keyValue in settingData.PartsNumber)
        {
            BodyPartsData Data = (BodyPartsData)keyValue.Value.itemData;
            bodyParts.Add(Data);
        }

        statusUI.StatusInitialize(bodyParts);
        statusUI.UIInitialize();
    }

    //ショップボタン
    public void ShopButtonOnClick()
    {
        shopUIController.StateTranstion(ShopControllerState.ShopState.SelectGenre);
    }
}

//基底ステートの定義
public abstract class UIControllerState : IState
{
    public enum CustomizeUIState
    {
        SetUp,
        WaitButtonSelect,
        OpenedScrollView,
        MissionScrollView,
    }

    public CustomizeUIState State;

    protected CustomizeUIController uiController;

    //呼べる関数をまとめるDictionary
    protected Dictionary<string, Action> actionDic;

    //引数ありの関数をまとめるよ
    protected Dictionary<string, Action<object[]>> actionDicWithArg;

    public virtual void CallFunc(string FuncName)
    {
        Action action = actionDic[FuncName];

        if (action != null)
        {
            action.Invoke();
        }
        else
        {
            throw new System.Exception("呼ぶ関数がないぜ！");
        }
    }

    public virtual void CallFuncArg(string FuncName, object[] args)
    {
        Action<object[]> action = actionDicWithArg[FuncName];

        if (action != null)
        {
            action.Invoke(args);
        }
        else
        {
            throw new System.Exception("呼ぶ関数がないぜ！");
        }
    }


    public virtual void OnEnter()
    {

    }

    public virtual void OnExit()
    {

    }

    public virtual void OnUpdate()
    {

    }
}
