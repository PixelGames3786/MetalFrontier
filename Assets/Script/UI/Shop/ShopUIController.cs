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
        //�R���X�g���N�^�@������
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

        //�R���X�g���N�^�@������
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
            //���ݑI�����Ă����ނ̏��i�̃X�N���[���r���[��\������
            ShopGenre genre = (ShopGenre)nowSelectNum;

            List<ItemData> goods=new List<ItemData>();

            switch (genre)
            {
                case ShopGenre.Armor:

                    //�Z�[�u�f�[�^������ւ���Ă���A�C�e�����X�g���擾�i����������j
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

            //�I�[�g�Z�[�u
            SaveDataManager.instance.SaveFileWriteAsync();
        }

    }

    class SelectGoodsState : ShopControllerState
    {
        //�R���X�g���N�^�@������
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

        //�R���X�g���N�^�@������
        public PurchaseCheckState(ShopUIController controller)
        {
            state = ShopState.PurchaseCheck;

            uiControl = controller;
        }

        public override void OnEnter()
        {
            //����������Ă��邩�ǂ����`�F�b�N
            purchaseGoods = uiControl.goodsScrollView.forcusInfo.goodsData;

            //����������Ă�����
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
            //����������Ă�����
            if (SaveDataManager.instance.saveData.haveCol >= purchaseGoods.price)
            {
                //Yes��I�����Ă����炨���𕥂��A�C�e������ɓ����
                if (uiControl.purchaseCheck.isConfirm)
                {
                    SaveDataManager.instance.saveData.ColChange(-purchaseGoods.price);

                    SaveDataManager.instance.saveData.AddItem(purchaseGoods.ItemNumber);

                    uiControl.colText.text = SaveDataManager.instance.saveData.haveCol + " Col";
                }

                //No��I�����Ă����牽�������߂�
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
        //Action�̃Z�b�g�A�b�v
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


    //�X�e�[�g�̐؂�ւ�
    public void StateTranstion(ShopControllerState.ShopState transitState)
    {
        nowState.OnExit();

        ShopControllerState newState = States.First(state => state.state == transitState);

        //�k���`�F
        if (newState == null) throw new System.Exception("�J�ڂ���X�e�[�g���Ȃ��炵����");

        beforeState = nowState;
        nowState = newState;

        nowState.OnEnter();
    }

    //�X�e�[�g�̊֐����Ăяo���i�����Ȃ��j
    public void CallStateFunc(string funcName)
    {
        nowState.CallFunc(funcName);
    }

    //�X�e�[�g�̊֐����Ăяo���i��������j
    public void CallStateFuncArg(string funcName, object[] args)
    {
        nowState.CallFuncArg(funcName, args);
    }

}


//���X�e�[�g�̒�`
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

    //�Ăׂ�֐����܂Ƃ߂�Dictionary
    protected Dictionary<string, Action> actionDic;

    //��������̊֐����܂Ƃ߂��
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
            throw new System.Exception("�ĂԊ֐����Ȃ����I");
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
            throw new System.Exception("�ĂԊ֐����Ȃ����I");
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
