using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;

public class ShopUIController : MonoBehaviour
{
    class WaitState : ShopControllerState
    {
        //コンストラクタ　初期化
        public WaitState(ShopUIController controller)
        {
            State = ShopState.Wait;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

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
            State = ShopState.SelectGenre;

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
            uiController.upArrowAct.performed += UpArrowAction;
            uiController.downArrowAct.performed += DownArrowAction;
            uiController.confirmAct.performed += ConfirmAction;
            uiController.canselAct.performed += CanselAction;

            uiController.colText.text = SaveDataManager.instance.saveData.haveCol+" Col";

            uiController.gameObject.SetActive(true);
            uiController.GetComponent<CanvasGroup>().DOFade(1f,0.5f);
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

            nowSelectNum = Mathf.Clamp(nowSelectNum,0,maxSelectNum);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum*70);

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

            uiController.goodsScrollView.InitializeScrollView(goods);
            uiController.goodsScrollView.OpenScrollView();

            uiController.StateTranstion(ShopState.SelectBuyGoods);
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(() => 
            {
                uiController.gameObject.SetActive(false);
                uiController.StateTranstion(ShopState.Wait);
            });
        }

    }

    class SelectGoodsState : ShopControllerState
    {
        //コンストラクタ　初期化
        public SelectGoodsState(ShopUIController controller)
        {
            State = ShopState.SelectBuyGoods;

            uiController = controller;

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
            uiController.canselAct.performed-=CanselAction;
        }

        private void InfoOnClick(object[] args)
        {
            ItemData goods = (ItemData)args[0];

            OpenGoodsStatus(goods);
        }

        private void OpenGoodsStatus(ItemData selectGoods)
        {
            uiController.goodsStatus.InitializeUI(selectGoods);
            uiController.goodsStatus.OpenWindow();
        }

        private void CloseGoodsStatus()
        {
            uiController.goodsStatus.CloseWindow();
        }

        private void UpArrowAction(InputAction.CallbackContext context)
        {
            uiController.goodsScrollView.ChangeForcus(-1);
        }

        private void DownArrowAction(InputAction.CallbackContext context)
        {
            uiController.goodsScrollView.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(ShopState.PurchaseCheck);
        }

        private void CanselAction(InputAction.CallbackContext context)
        {
            uiController.goodsScrollView.CloseScrollView();
            uiController.goodsStatus.CloseWindow();

            uiController.StateTranstion(ShopState.SelectGenre);
        }
    }

    class PurchaseCheckState : ShopControllerState
    {
        private ItemData purchaseGoods;

        //コンストラクタ　初期化
        public PurchaseCheckState(ShopUIController controller)
        {
            State = ShopState.PurchaseCheck;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };
        }

        public override void OnEnter()
        {
            //お金が足りているかどうかチェック
            purchaseGoods = uiController.goodsScrollView.forcusInfo.goodsData;

            //お金が足りていたら
            if (SaveDataManager.instance.saveData.haveCol>=purchaseGoods.price)
            {
                uiController.purchaseCheck.InitializeUI(uiController.goodsScrollView.forcusInfo.goodsData);
                uiController.purchaseCheck.OpenWindow();
            }
            else
            {
                uiController.purchaseError.OpenWindow();
            }

            uiController.leftArrowAct.performed += LeftArrowAction;
            uiController.rightArrowAct.performed += RightArrowAction;
            uiController.confirmAct.performed += ConfirmAction;
        }

        public override void OnExit()
        {
            uiController.leftArrowAct.performed -= LeftArrowAction;
            uiController.rightArrowAct.performed -= RightArrowAction;
            uiController.confirmAct.performed -= ConfirmAction;
        }

        private void LeftArrowAction(InputAction.CallbackContext context)
        {
            uiController.purchaseCheck.ChangeForcus(-1);
        }

        private void RightArrowAction(InputAction.CallbackContext context)
        {
            uiController.purchaseCheck.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context)
        {
            //お金が足りていたら
            if (SaveDataManager.instance.saveData.haveCol >= purchaseGoods.price)
            {
                //Yesを選択していたらお金を払いアイテムを手に入れる
                if (uiController.purchaseCheck.isPurchase)
                {
                    SaveDataManager.instance.saveData.ColChange(-purchaseGoods.price);

                    SaveDataManager.instance.saveData.AddItem(purchaseGoods.ItemNumber);

                    uiController.colText.text = SaveDataManager.instance.saveData.haveCol + " Col";
                }

                //Noを選択していたら何もせず戻る
                uiController.purchaseCheck.CloseWindow();
            }
            else
            {
                uiController.purchaseError.CloseWindow();
            }

            uiController.StateTranstion(ShopState.SelectBuyGoods);
        }
    }

    private List<ShopControllerState> States = new List<ShopControllerState>();

    private ShopControllerState nowState;

    public RectTransform selectArrowRect;

    public ShopGoodsScrollView goodsScrollView;
    public ShopGoodsStatusUI goodsStatus;
    public PurchaseCheckUI purchaseCheck;
    public PurchaseErrorUI purchaseError;

    public TextMeshProUGUI colText;

    public InputAction upArrowAct, downArrowAct,leftArrowAct,rightArrowAct, confirmAct,canselAct;

    // Start is called before the first frame update
    void Start()
    {
        //Actionのセットアップ
        TestControls testControl = new TestControls();

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

        States.Add(new WaitState(this));
        States.Add(new SelectGenreState(this));
        States.Add(new SelectGoodsState(this));
        States.Add(new PurchaseCheckState(this));

        nowState = States[0];

        nowState.OnEnter();

        gameObject.SetActive(false);
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

        ShopControllerState newState = States.First(state => state.State == transitState);

        //ヌルチェ
        if (newState == null) throw new System.Exception("遷移するステートがないらしいよ");

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

    public ShopState State;

    protected ShopUIController uiController;

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
