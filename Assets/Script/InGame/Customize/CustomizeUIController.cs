using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using DG.Tweening;
using UnityEngine.InputSystem;
using static ShopControllerState;

public class CustomizeUIController : MonoBehaviour
{
    //ステートで状態を管理する

    //シーンが読み込まれた際に、シーンのセットアップを行うステート
    class SetUpState : CustomizeControlState
    {
        //コンストラクタ　初期化
        public SetUpState(CustomizeUIController controller)
        {
            state = CustomizeUIState.SetUp;

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
                SaveDataManager.instance.onLoadComplete += SetUp;
            }
        }

        private void SetUp()
        {
            //自レガシーオブジェクト生成
            GameObject legacyObj = Instantiate(uiController.legacyCustomPrefab);
            uiController.robotControl = legacyObj.GetComponent<CustomRobotController>();

            uiController.robotControl.PartsSetReflect();

            uiController.StateTranstion(CustomizeUIState.SelectMenu);
        }
    }

    class WaitState : CustomizeControlState
    {
        //コンストラクタ　初期化
        public WaitState(CustomizeUIController controller)
        {
            state = CustomizeUIState.Wait;

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

        }
    }

    class SelectMenuState : CustomizeControlState
    {
        enum MenuState
        {
            Body,
            Weapon,
        }

        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 1;

        //コンストラクタ　初期化
        public SelectMenuState(CustomizeUIController controller)
        {
            state = CustomizeUIState.SelectMenu;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

            arrowDefaultYPosi = uiController.selectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            uiController.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;

            uiController.upArrowAct.performed += UpArrowAction;
            uiController.downArrowAct.performed += DownArrowAction;
            uiController.confirmAct.performed += ConfirmAction;
            uiController.canselAct.performed += CanselAction;
        }

        public override void OnExit()
        {
            uiController.upArrowAct.performed -= UpArrowAction;
            uiController.downArrowAct.performed -= DownArrowAction;
            uiController.confirmAct.performed -= ConfirmAction;
            uiController.canselAct.performed-= CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            //現在選択している種類の商品のスクロールビューを表示する
            MenuState select = (MenuState)nowSelectNum;

            switch (select)
            {
                case MenuState.Body:

                    uiController.StateTranstion(CustomizeUIState.SelectBodyMenu);

                    break;

                case MenuState.Weapon:

                    uiController.StateTranstion(CustomizeUIState.SelectWeaponMenu);

                    break;
            }
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(CustomizeUIState.Wait);
            uiController.mainMenuControl.StateTranstion(MainMenuState.MainMenuStateEnum.SelectMenu);

            uiController.GetComponent<CanvasGroup>().DOFade(0f, 0.5f);
        }
    }

    class SelectBodyMenuState : CustomizeControlState
    {
        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 4;

        //コンストラクタ　初期化
        public SelectBodyMenuState(CustomizeUIController controller)
        {
            state = CustomizeUIState.SelectBodyMenu;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

            arrowDefaultYPosi = uiController.selectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition=arrowPosi;

            uiController.bodyMenuParent.gameObject.SetActive(true);
            uiController.selectMenuParent.gameObject.SetActive(false);

            uiController.upArrowAct.performed += UpArrowAction;
            uiController.downArrowAct.performed += DownArrowAction;
            uiController.confirmAct.performed += ConfirmAction;
            uiController.canselAct.performed += CanselAction;
        }

        public override void OnExit()
        {
            uiController.upArrowAct.performed -= UpArrowAction;
            uiController.downArrowAct.performed -= DownArrowAction;
            uiController.confirmAct.performed -= ConfirmAction;
            uiController.canselAct.performed -= CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            //現在選択している種類の商品のスクロールビューを表示する
            BodyPartsData.PartsType select = (BodyPartsData.PartsType)nowSelectNum;

            OpenBodySelectScroll(select);
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(CustomizeUIState.SelectMenu);

            uiController.bodyMenuParent.gameObject.SetActive(false);
            uiController.selectMenuParent.gameObject.SetActive(true);
        }

        private void OpenBodySelectScroll(BodyPartsData.PartsType type)
        {
            //ボタンを非表示にしスクロールビューを表示　将来的にはDOTweenで動かしたい
            uiController.ButtonsParent.SetActive(false);

            //所持パーツから指定した種類のパーツを抜き出しそれでスクロールビューを構成する
            SaveData saveData = SaveDataManager.instance.saveData;

            List<HavingItem> ItemList = saveData.PartsDataList.Where(parts => parts.itemData is BodyPartsData bodyPartData && bodyPartData.Type == type).ToList();
            List<BodyPartsData> TypeDataList = ItemList.Select(parts => (BodyPartsData)parts.itemData).ToList();

            uiController.partsScroll.OpenScrollView();
            uiController.partsScroll.InitializeUI(TypeDataList, ItemList);

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
    }

    class SelectWeaponMenuState : CustomizeControlState
    {
        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 3;

        //コンストラクタ　初期化
        public SelectWeaponMenuState(CustomizeUIController controller)
        {
            state = CustomizeUIState.SelectWeaponMenu;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

            arrowDefaultYPosi = uiController.selectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;

            uiController.weaponMenuParent.gameObject.SetActive(true);
            uiController.selectMenuParent.gameObject.SetActive(false);

            uiController.upArrowAct.performed += UpArrowAction;
            uiController.downArrowAct.performed += DownArrowAction;
            uiController.confirmAct.performed += ConfirmAction;
            uiController.canselAct.performed += CanselAction;
        }

        public override void OnExit()
        {
            uiController.upArrowAct.performed -= UpArrowAction;
            uiController.downArrowAct.performed -= DownArrowAction;
            uiController.confirmAct.performed -= ConfirmAction;
            uiController.canselAct.performed -= CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            //現在選択している種類の商品のスクロールビューを表示する
            WeaponPartsData.SetType type=WeaponPartsData.SetType.Arm;
            LegacySettingData.WeaponSetPosi selectPosi = (LegacySettingData.WeaponSetPosi)nowSelectNum;

            if (nowSelectNum > 1) type = WeaponPartsData.SetType.Shoulder;

            OpenWeaponSelectScroll(type,selectPosi);
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(CustomizeUIState.SelectMenu);

            uiController.weaponMenuParent.gameObject.SetActive(false);
            uiController.selectMenuParent.gameObject.SetActive(true);
        }

        private void OpenWeaponSelectScroll(WeaponPartsData.SetType type,LegacySettingData.WeaponSetPosi setPosi)
        {
            //ボタンを非表示にしスクロールビューを表示　将来的にはDOTweenで動かしたい
            uiController.ButtonsParent.SetActive(false);

            //所持パーツから指定した種類のパーツを抜き出しそれでスクロールビューを構成する
            SaveData saveData = SaveDataManager.instance.saveData;
            List<HavingItem> ItemList = saveData.WeaponsDataList.Where(parts => parts.itemData is WeaponPartsData weaponData && weaponData.setType == type).ToList();
            List<WeaponPartsData> TypeDataList = ItemList.Select(parts => (WeaponPartsData)parts.itemData).ToList();

            uiController.partsScroll.InitializeUI(TypeDataList, ItemList);
            uiController.partsScroll.OpenScrollView();

            uiController.partsScroll.setPosi = setPosi;

            //状態遷移
            uiController.StateTranstion(CustomizeUIState.OpenedScrollView);
        }
    }

    //待機ステート
    public class WaitSelectState : CustomizeControlState
    {
        //コンストラクタ　初期化
        public WaitSelectState(CustomizeUIController controller)
        {
            state = CustomizeUIState.WaitButtonSelect;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
                {"OpenBodySelectScroll",OpenBodySelectScroll },
                {"OpenWeaponSelectScroll",OpenWeaponSelectScroll },
            };

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
    public class OpenedScrollViewState : CustomizeControlState
    {
        //コンストラクタ　初期化
        public OpenedScrollViewState(CustomizeUIController controller)
        {
            state = CustomizeUIState.OpenedScrollView;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {{"CloseScrollView",CloseScrollView }, };
        }

        public override void OnEnter()
        {
            uiController.upArrowAct.performed += UpArrowAction;
            uiController.downArrowAct.performed += DownArrowAction;
            uiController.confirmAct.performed += ConfirmAction;
            uiController.canselAct.performed += CanselAction;
        }

        public override void OnExit()
        {
            uiController.upArrowAct.performed -= UpArrowAction;
            uiController.downArrowAct.performed -= DownArrowAction;
            uiController.confirmAct.performed -= ConfirmAction;
            uiController.canselAct.performed -= CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            uiController.partsScroll.ChangeForcus(-1);
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            uiController.partsScroll.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(uiController.beforeState.state);

            uiController.partsScroll.CloseScrollView();

            //アウトラインを非表示に
            uiController.robotControl.SetAllOutlines(false);
        }


        public void CloseScrollView()
        {
            //ボタンを表示しスクロールビューを非表示
            uiController.partsScroll.gameObject.SetActive(false);

            uiController.ButtonsParent.SetActive(true);

            //アウトラインを非表示に
            uiController.robotControl.SetAllOutlines(false);

            //オートセーブする
            SaveDataManager.instance.SaveFileWriteAsync();

            //状態遷移
            uiController.StateTranstion(CustomizeUIState.WaitButtonSelect);
        }
    }

    public CanvasGroup curtainCanvas;

    public CustomRobotController robotControl;

    private List<CustomizeControlState> States = new List<CustomizeControlState>();

    private CustomizeControlState nowState,beforeState;

    public RectTransform selectArrowRect;
    public RectTransform selectMenuParent, bodyMenuParent,weaponMenuParent;

    public GameObject legacyCustomPrefab;

    public GameObject ButtonsParent;
    public PartsScrollView partsScroll;

    public LegacyStatusUI statusUI;

    public MainMenuUIController mainMenuControl;

    [NonSerialized]
    public InputAction upArrowAct, downArrowAct, leftArrowAct, rightArrowAct, confirmAct, canselAct;

    // Start is called before the first frame update
    void Start()
    {
        //Actionのセットアップ
        InputControls testControl = new InputControls();

        upArrowAct = testControl.UI.UpArrow;
        downArrowAct = testControl.UI.DownArrow;
        leftArrowAct = testControl.UI.LeftArrow;
        rightArrowAct = testControl.UI.RightArrow;
        confirmAct = testControl.UI.Confirm;
        canselAct = testControl.UI.Cansel;

        upArrowAct.Enable();
        downArrowAct.Enable();
        leftArrowAct.Enable();
        rightArrowAct.Enable();
        confirmAct.Enable();
        canselAct.Enable();

        States.Add(new SetUpState(this));
        States.Add(new SelectMenuState(this));

        
        States.Add(new WaitState(this));
        States.Add(new WaitSelectState(this));
        States.Add(new SelectBodyMenuState(this));
        States.Add(new SelectWeaponMenuState(this));
        States.Add(new OpenedScrollViewState(this));

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
    public void StateTranstion(CustomizeControlState.CustomizeUIState transitState)
    {
        nowState.OnExit();

        CustomizeControlState newState = States.First(state => state.state == transitState);

        //ヌルチェ
        if (newState == null) throw new System.Exception("遷移するステートがないらしいよ");

        beforeState = nowState;
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
}

//基底ステートの定義
public abstract class CustomizeControlState : IState
{
    public enum CustomizeUIState
    {
        SetUp,
        Wait,
        SelectMenu,     //ボディパーツか武器パーツか選択するメニュー
        SelectBodyMenu, //頭や体、どのパーツを変更するか選択するメニュー
        SelectWeaponMenu, //左腕や右腕、どのパーツを変更するか選択するメニュー
        WaitButtonSelect,
        OpenedScrollView,
    }

    public CustomizeUIState state;

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
