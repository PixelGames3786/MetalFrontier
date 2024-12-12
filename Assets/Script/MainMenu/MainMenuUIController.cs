using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEditor;
using static MissionManager;
using MenuType = DockImageTransition.MenuType;

public class MainMenuUIController : MonoBehaviour
{
    class WaitLoadState : MainMenuState
    {
        //�R���X�g���N�^�@������
        public WaitLoadState(MainMenuUIController controller) : base(controller)
        {
            state = MainMenuStateEnum.WaitLoad;
        }

        public override void OnEnter()
        {
            //�������Z�[�u�f�[�^�̃��[�h���ς�ł���̂Ȃ�
            if (SaveDataManager.instance.isLoadComplete)
            {
                CurtainShowUp();
            }
            else
            {
                //�I���܂ő҂�
                SaveDataManager.instance.onLoadComplete += CurtainShowUp;
            }
        }

        public override void OnExit()
        {
            SaveDataManager.instance.onLoadComplete -= CurtainShowUp;
        }

        //�Z�[�u�f�[�^�̃��[�h�����������ۂɁA��ʂ��B���Ă��開���グ��
        private void CurtainShowUp()
        {
            uiController.curtainCanvas.DOFade(0f, 0.5f).OnComplete(() =>
            {
                uiController.curtainCanvas.gameObject.SetActive(false);
                uiController.StateTranstion(MainMenuStateEnum.SelectMenu);
            });
        }
    }

    class WaitState : MainMenuState
    {
        //�R���X�g���N�^�@������
        public WaitState(MainMenuUIController controller) : base(controller)
        {
            state = MainMenuStateEnum.Wait;
        }

        public override void OnEnter()
        {
            
        }

        public override void OnExit()
        {
            uiController.thisCanvas.DOFade(1f, 0.3f);

            for (int i = 0; i < uiController.selectTextRects.Count; i++)
            {
                uiController.selectTextRects[i].localPosition = new Vector2(-700f, uiController.selectTextRects[i].localPosition.y);
                uiController.selectTextRects[i].DOLocalMoveX(-813f, 0.3f).SetDelay(i * 0.1f); //���ԍ��ō��Ɉړ�����悤��
                uiController.selectTextRects[i].GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f).SetDelay(i * 0.1f); //���ԍ��Ńt�F�[�h
            }
        }

    }

    class SelectMenuState : MainMenuState
    {
        enum MenuState
        {
            Mission,
            Customize,
            Shop,
        }

        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 2;

        private MenuState transitState;

        //�R���X�g���N�^�@������
        public SelectMenuState(MainMenuUIController controller) : base(controller)
        {
            state = MainMenuStateEnum.SelectMenu;

            uiController = controller;

            arrowDefaultYPosi = uiController.selectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            uiController.upArrowAct.performed += UpArrowAction;
            uiController.downArrowAct.performed += DownArrowAction;
            uiController.confirmAct.performed += ConfirmAction;
        }

        public override void OnExit()
        {
            uiController.upArrowAct.performed -= UpArrowAction;
            uiController.downArrowAct.performed -= DownArrowAction;
            uiController.confirmAct.performed -= ConfirmAction;

            switch (transitState)
            {
                case MenuState.Mission:

                    uiController.transitionManager.TransitionToRight(MenuType.Terminal, MenuType.Mission);
                    uiController.missionUIControl.StateTranstion(MissionSelectState.MissionSelectStateEnum.SelectMission);

                    break;

                case MenuState.Customize:

                    uiController.transitionManager.TransitionToRight(MenuType.Terminal, MenuType.CustomGenre);
                    uiController.customizeUIControl.StateTranstion(CustomizeControlState.CustomizeUIState.SelectMenu);

                    break;

                case MenuState.Shop:

                    uiController.transitionManager.TransitionToRight(MenuType.Terminal, MenuType.Shop);
                    uiController.shopUIControl.StateTranstion(ShopControllerState.ShopState.SelectGenre);

                    break;
            }

            /*
            uiController.thisCanvas.DOFade(0f, 0.3f).OnComplete(() => 
            {
                
            });

            /*

            for (int i = 0; i < uiController.selectTextRects.Count; i++)
            {
                uiController.selectTextRects[i].localPosition = new Vector2(-813f, uiController.selectTextRects[i].localPosition.y);
                uiController.selectTextRects[i].DOLocalMoveX(-900f, 0.3f).SetDelay(i * 0.1f); //���ԍ��ō��Ɉړ�����悤��
                uiController.selectTextRects[i].GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f).SetDelay(i * 0.1f); //���ԍ��Ńt�F�[�h
            }
            */
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

        private void ConfirmAction(InputAction.CallbackContext context) //�m��
        {
            transitState = (MenuState)nowSelectNum;

            uiController.StateTranstion(MainMenuStateEnum.Wait);
        }
    }

    private List<MainMenuState> States = new List<MainMenuState>();

    private MainMenuState nowState;

    public RectTransform selectArrowRect;
    public CanvasGroup curtainCanvas,thisCanvas;

    public List<RectTransform> selectTextRects = new List<RectTransform>();

    public CustomizeUIController customizeUIControl;
    public ShopUIController shopUIControl;
    public MissionSelectUIController missionUIControl;

    public DockImageTransition transitionManager;

    [NonSerialized]
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

        States.Add(new WaitLoadState(this));
        States.Add(new SelectMenuState(this));
        States.Add(new WaitState(this));

        nowState = States[0];

        nowState.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        nowState.OnUpdate();
    }


    //�X�e�[�g�̐؂�ւ�
    public void StateTranstion(MainMenuState.MainMenuStateEnum transitState)
    {
        nowState.OnExit();

        MainMenuState newState = States.First(state => state.state == transitState);

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
public abstract class MainMenuState : IState
{
    public enum MainMenuStateEnum
    {
        WaitLoad, //������ԁ@�Z�[�u�}�l�[�W���[�̃��[�h��҂�
        Wait,
        SelectMenu,
    }

    public MainMenuStateEnum state;

    protected MainMenuUIController uiController;

    //�Ăׂ�֐����܂Ƃ߂�Dictionary
    protected Dictionary<string, Action> actionDic;

    //��������̊֐����܂Ƃ߂��
    protected Dictionary<string, Action<object[]>> actionDicWithArg;

    //�R���X�g���N�^�@������
    public MainMenuState(MainMenuUIController controller)
    {
        uiController = controller;
    }

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


    public virtual void OnEnter() { }

    public virtual void OnExit(){ }

    public virtual void OnUpdate() { }
}
