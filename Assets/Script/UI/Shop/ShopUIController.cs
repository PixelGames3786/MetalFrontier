using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;
using static CustomizeControlState;

public class ShopUIController : MonoBehaviour
{
    class WaitState : ShopControllerState
    {
        //コンストラクタ　初期化
        public WaitState(ShopUIController controller)
        {
            state = ShopState.Wait;

            uiControl = controller;
        }
    }

    class SelectGenreState : ShopControllerState
    {
        private enum ShopGenre
        {
            Armor,
            Weapon,
        }

        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 1;

        //コンストラクタ　初期化
        public SelectGenreState(ShopUIController controller)
        {
            state = ShopState.SelectGenre;

            uiControl = controller;

            arrowDefaultYPosi = uiControl.selectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            uiControl.colText.text = SaveDataManager.instance.saveData.haveCol+" Col";

            //uiController.GetComponent<CanvasGroup>().DOFade(1f,0.4f);

            if (uiControl.beforeState.state == ShopState.Wait)
            {
                uiControl.transitionManager.onTransitionComplete += InputActionRegister;
            }
            else
            {
                InputActionRegister();
            }
        }

        public override void OnExit()
        {
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

            nowSelectNum = Mathf.Clamp(nowSelectNum,0,maxSelectNum);

            Vector2 arrowPosi = uiControl.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum*70);

            uiControl.selectArrowRect.localPosition = arrowPosi;
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiControl.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.selectArrowRect.localPosition = arrowPosi;
        }

        public void ConfirmAction(InputAction.CallbackContext context)
        {
            //現在選択している種類の商品のスクロールビューを表示する
            ShopGenre genre = (ShopGenre)nowSelectNum;

            List<ItemData> goods=new List<ItemData>();

            switch (genre)
            {
                case ShopGenre.Armor:

                    //セーブデータから解禁されているアイテムリストを取得（武器を除く）
                    goods = SaveDataManager.instance.saveData.shopGoodsList.Where(data => data is BodyPartsData).ToList();

                    break;

                case ShopGenre.Weapon:

                    goods = SaveDataManager.instance.saveData.shopGoodsList.Where(data => data is WeaponPartsData).ToList();

                    break;
            }

            uiControl.StateTranstion(ShopState.SelectBuyGoods);

            uiControl.goodsScrollView.InitializeScrollView(goods);
            uiControl.goodsScrollView.OpenScrollView();

        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            //uiController.GetComponent<CanvasGroup>().DOFade(0f, 0.4f);

            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.Terminal,DockImageTransition.MenuType.Shop);

            FindObjectOfType<MainMenuUIController>().StateTranstion(MainMenuState.MainMenuStateEnum.SelectMenu);
            uiControl.StateTranstion(ShopState.Wait);

            //オートセーブ
            SaveDataManager.instance.SaveFileWriteAsync();
        }

    }

    class SelectGoodsState : ShopControllerState
    {
        //コンストラクタ　初期化
        public SelectGoodsState(ShopUIController controller)
        {
            state = ShopState.SelectBuyGoods;

            uiControl = controller;

            actionDic = new Dictionary<string, Action>()
            {
                {"CloseGoodsStatus",CloseGoodsStatus }
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
                {"InfoOnClick", InfoOnClick}
            };
        }

        public override void OnEnter()
        {
            uiControl.upArrowAct.performed += UpArrowAction;
            uiControl.downArrowAct.performed += DownArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
            uiControl.cancelAct.performed += CanselAction;
        }

        public override void OnExit()
        {
            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed-=CanselAction;
        }

        private void InfoOnClick(object[] args)
        {
            ItemData goods = (ItemData)args[0];

            OpenGoodsStatus(goods);
        }

        private void OpenGoodsStatus(ItemData selectGoods)
        {
            uiControl.goodsStatus.InitializeUI(selectGoods);
            uiControl.goodsStatus.OpenWindow();
        }

        private void CloseGoodsStatus()
        {
            uiControl.goodsStatus.CloseWindow();
        }

        private void UpArrowAction(InputAction.CallbackContext context)
        {
            uiControl.goodsScrollView.ChangeForcus(-1);
        }

        private void DownArrowAction(InputAction.CallbackContext context)
        {
            uiControl.goodsScrollView.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context)
        {
            uiControl.StateTranstion(ShopState.PurchaseCheck);
        }

        private void CanselAction(InputAction.CallbackContext context)
        {
            uiControl.goodsScrollView.CloseScrollView();
            uiControl.goodsStatus.CloseWindow();

            uiControl.StateTranstion(ShopState.SelectGenre);
        }
    }

    class PurchaseCheckState : ShopControllerState
    {
        private ItemData purchaseGoods;

        //コンストラクタ　初期化
        public PurchaseCheckState(ShopUIController controller)
        {
            state = ShopState.PurchaseCheck;

            uiControl = controller;
        }

        public override void OnEnter()
        {
            //お金が足りているかどうかチェック
            purchaseGoods = uiControl.goodsScrollView.forcusInfo.goodsData;

            //お金が足りていたら
            if (SaveDataManager.instance.saveData.haveCol>=purchaseGoods.price)
            {
                uiControl.purchaseCheck.InitializeShopUI(uiControl.goodsScrollView.forcusInfo.goodsData);
                uiControl.purchaseCheck.OpenWindow();
            }
            else
            {
                uiControl.purchaseError.OpenWindow();
            }

            uiControl.leftArrowAct.performed += LeftArrowAction;
            uiControl.rightArrowAct.performed += RightArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
        }

        public override void OnExit()
        {
            uiControl.leftArrowAct.performed -= LeftArrowAction;
            uiControl.rightArrowAct.performed -= RightArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
        }

        private void LeftArrowAction(InputAction.CallbackContext context)
        {
            uiControl.purchaseCheck.ChangeForcus(-1);
        }

        private void RightArrowAction(InputAction.CallbackContext context)
        {
            uiControl.purchaseCheck.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context)
        {
            //お金が足りていたら
            if (SaveDataManager.instance.saveData.haveCol >= purchaseGoods.price)
            {
                //Yesを選択していたらお金を払いアイテムを手に入れる
                if (uiControl.purchaseCheck.isConfirm)
                {
                    SaveDataManager.instance.saveData.ColChange(-purchaseGoods.price);

                    SaveDataManager.instance.saveData.AddItem(purchaseGoods.ItemNumber);

                    uiControl.colText.text = SaveDataManager.instance.saveData.haveCol + " Col";
                }

                //Noを選択していたら何もせず戻る
                uiControl.purchaseCheck.CloseWindow();
            }
            else
            {
                uiControl.purchaseError.CloseWindow();
            }

            uiControl.StateTranstion(ShopState.SelectBuyGoods);
        }
    }

    private List<ShopControllerState> States = new List<ShopControllerState>();

    private ShopControllerState nowState,beforeState;

    public RectTransform selectArrowRect;
    public CanvasGroup thisGroup;

    public ShopGoodsScrollView goodsScrollView;
    public ShopGoodsStatusUI goodsStatus;
    public ConfirmCheckUI purchaseCheck;
    public PurchaseErrorUI purchaseError;

    public TextMeshProUGUI colText;

    public DockImageTransition transitionManager;

    public InputAction upArrowAct, downArrowAct,leftArrowAct,rightArrowAct, confirmAct,cancelAct;

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
        cancelAct = testControl.UI.Cancel;

        upArrowAct.Enable();
        downArrowAct.Enable();
        leftArrowAct.Enable();
        rightArrowAct.Enable(); 
        confirmAct.Enable();
        cancelAct.Enable();

        States.Add(new WaitState(this));
        States.Add(new SelectGenreState(this));
        States.Add(new SelectGoodsState(this));
        States.Add(new PurchaseCheckState(this));

        nowState = States[0];

        nowState.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        nowState.OnUpdate();
    }


    //ステートの切り替え
    public void StateTranstion(ShopControllerState.ShopState transitState)
    {
        nowState.OnExit();

        ShopControllerState newState = States.First(state => state.state == transitState);

        //ヌルチェ
        if (newState == null) throw new System.Exception("遷移するステートがないらしいよ");

        beforeState = nowState;
        nowState = newState;

        nowState.OnEnter();
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

}


//基底ステートの定義
public abstract class ShopControllerState : IState
{
    public enum ShopState
    {
        Wait,
        SelectGenre,
        SelectBuyGoods,
        PurchaseCheck,
    }

    public ShopState state;

    protected ShopUIController uiControl;

    //呼べる関数をまとめるDictionary
    protected Dictionary<string, Action> actionDic;

    //引数ありの関数をまとめるよ
    protected Dictionary<string, Action<object[]>> actionDicWithArg;

    public void CallFunc(string FuncName)
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

    public void CallFuncArg(string FuncName, object[] args)
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
