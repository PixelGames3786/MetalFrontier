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
        //�R���X�g���N�^�@������
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

        //�R���X�g���N�^�@������
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
        //�R���X�g���N�^�@������
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

        //�R���X�g���N�^�@������
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
            //����������Ă��邩�ǂ����`�F�b�N
            purchaseGoods = uiController.goodsScrollView.forcusInfo.goodsData;

            //����������Ă�����
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
            //����������Ă�����
            if (SaveDataManager.instance.saveData.haveCol >= purchaseGoods.price)
            {
                //Yes��I�����Ă����炨���𕥂��A�C�e������ɓ����
                if (uiController.purchaseCheck.isPurchase)
                {
                    SaveDataManager.instance.saveData.ColChange(-purchaseGoods.price);

                    SaveDataManager.instance.saveData.AddItem(purchaseGoods.ItemNumber);

                    uiController.colText.text = SaveDataManager.instance.saveData.haveCol + " Col";
                }

                //No��I�����Ă����牽�������߂�
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
        //Action�̃Z�b�g�A�b�v
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


    //�X�e�[�g�̐؂�ւ�
    public void StateTranstion(ShopControllerState.ShopState transitState)
    {
        nowState.OnExit();

        ShopControllerState newState = States.First(state => state.State == transitState);

        //�k���`�F
        if (newState == null) throw new System.Exception("�J�ڂ���X�e�[�g���Ȃ��炵����");

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

    public ShopState State;

    protected ShopUIController uiController;

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
