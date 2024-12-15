using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using DG.Tweening;
using UnityEngine.InputSystem;
using static ShopControllerState;
using TMPro;

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

            uiControl = controller;
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
            GameObject legacyObj = Instantiate(uiControl.legacyCustomPrefab);
            uiControl.robotControl = legacyObj.GetComponent<CustomRobotController>();

            uiControl.robotControl.PartsSetReflect();

            uiControl.StateTranstion(CustomizeUIState.Wait);
        }
    }

    class WaitState : CustomizeControlState
    {
        //コンストラクタ　初期化
        public WaitState(CustomizeUIController controller)
        {
            state = CustomizeUIState.Wait;

            uiControl = controller;
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

            uiControl = controller;

            arrowDefaultYPosi = uiControl.genreSelectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            //UIが滑らかにフェードインする
            Vector2 arrowPosi = uiControl.genreSelectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.genreSelectArrowRect.localPosition = arrowPosi;

            /*

            uiControl.genreSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(1f,0.3f);

            RectTransform[] menuChildRect= uiControl.selectMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-700f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-813f, 0.3f).SetDelay(i * 0.1f); //時間差で左に移動するように
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f).SetDelay(i * 0.1f); //時間差でフェード
            }*/

            uiControl.transitionManager.onTransitionComplete += InputActionRegister;
        }

        public override void OnExit()
        {
            //UIが滑らかにフェードアウトする
            /*
            uiControl.genreSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f);

            RectTransform[] menuChildRect = uiControl.selectMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-813f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-900f, 0.3f).SetDelay(i * 0.1f); //時間差で左に移動するように
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f).SetDelay(i * 0.1f); //時間差でフェード
            }*/

            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed-= CanselAction;
        }

        private void InputActionRegister()
        {
            uiControl.upArrowAct.performed += UpArrowAction;
            uiControl.downArrowAct.performed += DownArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
            uiControl.cancelAct.performed += CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiControl.genreSelectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.genreSelectArrowRect.localPosition = arrowPosi;
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiControl.genreSelectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.genreSelectArrowRect.localPosition = arrowPosi;
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            //現在選択している種類の商品のスクロールビューを表示する
            MenuState select = (MenuState)nowSelectNum;

            switch (select)
            {
                case MenuState.Body:

                    uiControl.transitionManager.TransitionToRight(DockImageTransition.MenuType.CustomGenre,DockImageTransition.MenuType.CustomPartsType);
                    uiControl.StateTranstion(CustomizeUIState.SelectBodyMenu);

                    break;

                case MenuState.Weapon:

                    uiControl.transitionManager.TransitionToRight(DockImageTransition.MenuType.CustomGenre, DockImageTransition.MenuType.CustomPartsType);
                    uiControl.StateTranstion(CustomizeUIState.SelectWeaponMenu);

                    break;
            }
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.Terminal,DockImageTransition.MenuType.CustomGenre);
            uiControl.StateTranstion(CustomizeUIState.Wait);
            uiControl.mainMenuControl.StateTranstion(MainMenuState.MainMenuStateEnum.SelectMenu);
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

            uiControl = controller;

            arrowDefaultYPosi = uiControl.typeSelectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            //UIが滑らかにフェードインする
            Vector2 arrowPosi = uiControl.typeSelectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.typeSelectArrowRect.localPosition = arrowPosi;

            //uiController.typeSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f);

            uiControl.bodyMenuParent.gameObject.SetActive(true);
            uiControl.weaponMenuParent.gameObject.SetActive(false);

            /*
            RectTransform[] menuChildRect = uiControl.bodyMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-700f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-813f, 0.3f).SetDelay(i * 0.1f); //時間差で左に移動するように
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f).SetDelay(i * 0.1f); //時間差でフェード
            }*/

            uiControl.transitionManager.onTransitionComplete += InputActionRegister;
        }

        private void InputActionRegister()
        {
            uiControl.upArrowAct.performed += UpArrowAction;
            uiControl.downArrowAct.performed += DownArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
            uiControl.cancelAct.performed += CanselAction;
        }

        public override void OnExit()
        {
            //UIが滑らかにフェードアウトする
            //uiController.typeSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f);

            /*
            RectTransform[] menuChildRect = uiControl.bodyMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-813f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-900f, 0.3f).SetDelay(i * 0.1f); //時間差で左に移動するように
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f).SetDelay(i * 0.1f); //時間差でフェード
            }*/

            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed -= CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            ChangeArrowPosition();
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            ChangeArrowPosition();
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            //現在選択している種類の商品のスクロールビューを表示する
            BodyPartsData.PartsType select = (BodyPartsData.PartsType)nowSelectNum;

            OpenBodySelectScroll(select);
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.CustomGenre, DockImageTransition.MenuType.CustomPartsType);
            uiControl.StateTranstion(CustomizeUIState.SelectMenu);
        }

        private void ChangeArrowPosition()
        {
            Vector2 arrowPosi = uiControl.typeSelectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.typeSelectArrowRect.localPosition = arrowPosi;
        }

        private void OpenBodySelectScroll(BodyPartsData.PartsType type)
        {
            //所持パーツから指定した種類のパーツを抜き出しそれでスクロールビューを構成する
            SaveData saveData = SaveDataManager.instance.saveData;

            List<HavingItem> ItemList = saveData.PartsDataList.Where(parts => parts.itemData is BodyPartsData bodyPartData && bodyPartData.Type == type).ToList();
            List<BodyPartsData> TypeDataList = ItemList.Select(parts => (BodyPartsData)parts.itemData).ToList();

            uiControl.partsScroll.OpenScrollView();
            uiControl.partsScroll.InitializeUI(TypeDataList, ItemList);

            uiControl.partsScroll.setPartsType = type;


            //選択中のメッシュのアウトラインを表示
            //現在装備している選択した種類のデータを取得
            uiControl.robotControl.SetAllOutlines(false);

            BodyPartsData partsData = (BodyPartsData)saveData.settingData.PartsNumber[type].itemData;

            foreach (KeyValuePair<string, Mesh> keyValue in partsData.ObjnameMeshPairs)
            {
                uiControl.robotControl.SetMeshOutline(keyValue.Key, true);
            }

            uiControl.statusUI.ChangeCustomize();

            //状態遷移
            uiControl.transitionManager.TransitionToRight(DockImageTransition.MenuType.CustomPartsType, DockImageTransition.MenuType.CustomSelectParts);
            uiControl.StateTranstion(CustomizeUIState.OpenedScrollView);
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

            uiControl = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

            arrowDefaultYPosi = uiControl.typeSelectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            //UIが滑らかにフェードインする
            Vector2 arrowPosi = uiControl.typeSelectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.typeSelectArrowRect.localPosition = arrowPosi;

            //uiController.typeSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f);

            uiControl.bodyMenuParent.gameObject.SetActive(false);
            uiControl.weaponMenuParent.gameObject.SetActive(true);

            /*

            RectTransform[] menuChildRect = uiControl.weaponMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-700f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-813f, 0.3f).SetDelay(i * 0.1f); //時間差で左に移動するように
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f).SetDelay(i * 0.1f); //時間差でフェード
            }*/

            uiControl.transitionManager.onTransitionComplete += InputActionRegister;

        }

        public override void OnExit()
        {
            //UIが滑らかにフェードアウトする
            //uiController.typeSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f);

            /*
            RectTransform[] menuChildRect = uiControl.weaponMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-813f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-900f, 0.3f).SetDelay(i * 0.1f); //時間差で左に移動するように
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f).SetDelay(i * 0.1f); //時間差でフェード
            }*/

            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed -= CanselAction;
        }

        private void InputActionRegister()
        {
            uiControl.upArrowAct.performed += UpArrowAction;
            uiControl.downArrowAct.performed += DownArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
            uiControl.cancelAct.performed += CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            ArrowPositionChange();
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            ArrowPositionChange();
        }

        private void ArrowPositionChange()
        {
            Vector2 arrowPosi = uiControl.typeSelectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.typeSelectArrowRect.localPosition = arrowPosi;
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
            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.CustomGenre, DockImageTransition.MenuType.CustomPartsType);
            uiControl.StateTranstion(CustomizeUIState.SelectMenu);
        }

        private void OpenWeaponSelectScroll(WeaponPartsData.SetType type,LegacySettingData.WeaponSetPosi setPosi)
        {
            //所持パーツから指定した種類のパーツを抜き出しそれでスクロールビューを構成する
            SaveData saveData = SaveDataManager.instance.saveData;
            List<HavingItem> ItemList = saveData.WeaponsDataList.Where(parts => parts.itemData is WeaponPartsData weaponData && weaponData.setType == type).ToList();
            List<WeaponPartsData> TypeDataList = ItemList.Select(parts => (WeaponPartsData)parts.itemData).ToList();

            uiControl.partsScroll.InitializeUI(TypeDataList, ItemList);
            uiControl.partsScroll.OpenScrollView();

            uiControl.partsScroll.setPosi = setPosi;

            //状態遷移
            uiControl.transitionManager.TransitionToRight(DockImageTransition.MenuType.CustomPartsType, DockImageTransition.MenuType.CustomSelectParts);
            uiControl.StateTranstion(CustomizeUIState.OpenedScrollView);
        }
    }

    //スクロールビューでの選択待ちステート
    public class OpenedScrollViewState : CustomizeControlState
    {
        //コンストラクタ　初期化
        public OpenedScrollViewState(CustomizeUIController controller)
        {
            state = CustomizeUIState.OpenedScrollView;

            uiControl = controller;
        }

        public override void OnEnter()
        {
            uiControl.transitionManager.onTransitionComplete += InputActionRegister;
        }

        public override void OnExit()
        {
            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed -= CanselAction;
        }

        private void InputActionRegister()
        {
            uiControl.upArrowAct.performed += UpArrowAction;
            uiControl.downArrowAct.performed += DownArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
            uiControl.cancelAct.performed += CanselAction;

            uiControl.openStatusAct.performed += StatusOpenClose;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            uiControl.partsScroll.ChangeForcus(-1);
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            uiControl.partsScroll.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            uiControl.partsScroll.SelectConfirm();
        }

        //ステータス表示
        public void StatusOpenClose(InputAction.CallbackContext context)
        {
            uiControl.statusUI.ChangeNextState();
            uiControl.StatusUIDataReflesh();
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            //オートセーブする
            SaveDataManager.instance.SaveFileWriteAsync();

            //アウトラインを非表示に
            uiControl.robotControl.SetAllOutlines(false);

            if (uiControl.beforeState.state == CustomizeUIState.SelectBodyMenu)
            {
                uiControl.statusUI.ChangeCustomize();
                uiControl.StatusUIDataReflesh();
            }

            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.CustomPartsType, DockImageTransition.MenuType.CustomSelectParts);
            uiControl.StateTranstion(uiControl.beforeState.state);
        }
    }

    public CanvasGroup curtainCanvas;

    public CustomRobotController robotControl;

    private List<CustomizeControlState> States = new List<CustomizeControlState>();

    private CustomizeControlState nowState,beforeState;

    public RectTransform genreSelectArrowRect,typeSelectArrowRect;
    public RectTransform selectMenuParent, bodyMenuParent,weaponMenuParent;

    public GameObject legacyCustomPrefab;

    public GameObject ButtonsParent;
    public PartsScrollView partsScroll;

    public LegacyStatusUI statusUI;

    public MainMenuUIController mainMenuControl;

    public DockImageTransition transitionManager;

    [NonSerialized]
    public InputAction upArrowAct, downArrowAct, leftArrowAct, rightArrowAct, confirmAct, cancelAct;

    private InputAction openStatusAct;

    private void Awake()
    {
        //Actionのセットアップ
        InputControls testControl = new InputControls();

        upArrowAct = testControl.UI.UpArrow;
        downArrowAct = testControl.UI.DownArrow;
        leftArrowAct = testControl.UI.LeftArrow;
        rightArrowAct = testControl.UI.RightArrow;
        confirmAct = testControl.UI.Confirm;
        cancelAct = testControl.UI.Cancel;
        openStatusAct = testControl.UI.OpenStatus;
    }

    // Start is called before the first frame update
    void Start()
    {
        //openStatusAct.performed += StatusOpenClose;

        States.Add(new SetUpState(this));
        States.Add(new WaitState(this));
        States.Add(new SelectMenuState(this));
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

    //ステータス表示
    public void StatusOpenClose(InputAction.CallbackContext context)
    {
        statusUI.ChangeNextState();
        StatusUIDataReflesh();
    }

    //ステータスUIデータ更新
    public void StatusUIDataReflesh()
    {
        statusUI.NowStatusInitialize();
        statusUI.UIInitialize();
    }

    private void OnDisable()
    {
        upArrowAct.Disable();
        downArrowAct.Disable();
        leftArrowAct.Disable();
        rightArrowAct.Disable();
        confirmAct.Disable();
        cancelAct.Disable();
        openStatusAct.Disable();
    }

    private void OnEnable()
    {
        upArrowAct.Enable();
        downArrowAct.Enable();
        leftArrowAct.Enable();
        rightArrowAct.Enable();
        confirmAct.Enable();
        cancelAct.Enable();
        openStatusAct.Enable();
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
        OpenedScrollView,
    }

    public CustomizeUIState state;

    protected CustomizeUIController uiControl;

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
