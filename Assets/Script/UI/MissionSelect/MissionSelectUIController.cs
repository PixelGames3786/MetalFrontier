using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;

public class MissionSelectUIController : MonoBehaviour
{
    class WaitState : MissionSelectState
    {
        //コンストラクタ　初期化
        public WaitState(MissionSelectUIController controller)
        {
            state = MissionSelectStateEnum.Wait;

            uiControl = controller;
        }
    }

    class SelectMissionState : MissionSelectState
    {
        //コンストラクタ　初期化
        public SelectMissionState(MissionSelectUIController controller)
        {
            state = MissionSelectStateEnum.SelectMission;

            uiControl = controller;

            actionDic = new Dictionary<string, Action>()
            {
                {"CloseGoodsStatus",CloseMissionStatus }
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
                {"InfoOnForcus", InfoOnForcus}
            };
        }

        public override void OnEnter()
        {
            uiControl.missionScrollView.InitializeScrollView(SaveDataManager.instance.saveData.missionDataList);

            if (uiControl.beforeState.state == MissionSelectStateEnum.Wait)
            {
                uiControl.transitionManager.onTransitionComplete += InputActionRegister;
            }
            else
            {
                InputActionRegister();
            }
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
            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed-=CanselAction;
        }

        private void InfoOnForcus(object[] args)
        {
            MissionData missions = (MissionData)args[0];
            OpenMissionStatus(missions);
        }

        private void OpenMissionStatus(MissionData selectMission)
        {
            uiControl.missionStatus.InitializeUI(selectMission);
            uiControl.missionStatus.OpenWindow();
        }

        private void CloseMissionStatus()
        {
            uiControl.missionStatus.CloseWindow();
        }

        private void UpArrowAction(InputAction.CallbackContext context)
        {
            uiControl.missionScrollView.ChangeForcus(-1);
        }

        private void DownArrowAction(InputAction.CallbackContext context)
        {
            uiControl.missionScrollView.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            uiControl.StateTranstion(MissionSelectStateEnum.ConfirmCheck);
        }

        private void CanselAction(InputAction.CallbackContext context)
        {
            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.Terminal, DockImageTransition.MenuType.Mission);

            FindObjectOfType<MainMenuUIController>().StateTranstion(MainMenuState.MainMenuStateEnum.SelectMenu);
            uiControl.StateTranstion(MissionSelectStateEnum.Wait);
        }
    }

    class ConfirmCheckState : MissionSelectState
    {
        //コンストラクタ　初期化
        public ConfirmCheckState(MissionSelectUIController controller)
        {
            state = MissionSelectStateEnum.ConfirmCheck;

            uiControl = controller;
        }

        public override void OnEnter()
        {
            uiControl.confirmCheck.InitializeMissionUI(uiControl.missionScrollView.forcusInfo.missionData);
            uiControl.confirmCheck.OpenWindow();

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
            uiControl.confirmCheck.ChangeForcus(-1);
        }

        private void RightArrowAction(InputAction.CallbackContext context)
        {
            uiControl.confirmCheck.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context)
        {
            //Yesを選択していたらミッションシーンに遷移
            if (uiControl.confirmCheck.isConfirm)
            {
                SelectMission(uiControl.missionScrollView.forcusInfo.missionData.sceneName);
            }
            else
            {
                uiControl.confirmCheck.CloseWindow();
                uiControl.StateTranstion(MissionSelectStateEnum.SelectMission);
            }
        }

        public void SelectMission(string missionSceneName)
        {
            uiControl.curtainCanvas.gameObject.SetActive(true);
            uiControl.curtainCanvas.DOFade(1f, 1f).OnComplete(() =>
            {
                uiControl.leftArrowAct.performed -= LeftArrowAction;
                uiControl.rightArrowAct.performed -= RightArrowAction;
                uiControl.confirmAct.performed -= ConfirmAction;

                SceneChangeManager.instance.StartCoroutine("SceneTransition", missionSceneName);
            });
        }
    }

    private List<MissionSelectState> States = new List<MissionSelectState>();

    private MissionSelectState nowState,beforeState;

    public RectTransform selectArrowRect;

    public MissionScrollView missionScrollView;
    public MissionStatusUI missionStatus;
    public ConfirmCheckUI confirmCheck;

    public CanvasGroup curtainCanvas;

    [SerializeField]
    private DockImageTransition transitionManager;

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
        States.Add(new SelectMissionState(this));
        States.Add(new ConfirmCheckState(this));

        nowState = States[0];

        nowState.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        nowState.OnUpdate();
    }


    //ステートの切り替え
    public void StateTranstion(MissionSelectState.MissionSelectStateEnum transitState)
    {
        nowState.OnExit();

        MissionSelectState newState = States.First(state => state.state == transitState);

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
public abstract class MissionSelectState : IState
{
    public enum MissionSelectStateEnum
    {
        Wait,
        SelectMission,
        ConfirmCheck,
    }

    public MissionSelectStateEnum state;

    protected MissionSelectUIController uiControl;

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
