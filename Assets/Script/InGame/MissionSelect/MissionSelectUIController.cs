using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEditor.ShaderGraph;
using UnityEditor;
using static ShopControllerState;

public class MissionSelectUIController : MonoBehaviour
{
    class WaitState : MissionSelectState
    {
        //コンストラクタ　初期化
        public WaitState(MissionSelectUIController controller)
        {
            State = MissionSelectStateEnum.Wait;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

        }
    }

    class SelectMissionState : MissionSelectState
    {
        //コンストラクタ　初期化
        public SelectMissionState(MissionSelectUIController controller)
        {
            State = MissionSelectStateEnum.SelectMission;

            uiController = controller;

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
            uiController.upArrowAct.performed += UpArrowAction;
            uiController.downArrowAct.performed += DownArrowAction;
            uiController.confirmAct.performed += ConfirmAction;
            uiController.canselAct.performed += CanselAction;

            uiController.gameObject.SetActive(true);
            uiController.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);

            uiController.missionScrollView.InitializeScrollView(SaveDataManager.instance.saveData.missionDataList);
            uiController.missionScrollView.OpenScrollView();
        }

        public override void OnExit()
        {
            uiController.upArrowAct.performed -= UpArrowAction;
            uiController.downArrowAct.performed -= DownArrowAction;
            uiController.confirmAct.performed -= ConfirmAction;
            uiController.canselAct.performed-=CanselAction;
        }

        private void InfoOnForcus(object[] args)
        {
            MissionData missions = (MissionData)args[0];
            OpenMissionStatus(missions);
        }

        private void OpenMissionStatus(MissionData selectMission)
        {
            uiController.missionStatus.InitializeUI(selectMission);
            uiController.missionStatus.OpenWindow();
        }

        private void CloseMissionStatus()
        {
            uiController.missionStatus.CloseWindow();
        }

        private void UpArrowAction(InputAction.CallbackContext context)
        {
            uiController.missionScrollView.ChangeForcus(-1);
        }

        private void DownArrowAction(InputAction.CallbackContext context)
        {
            uiController.missionScrollView.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context) //確定
        {
            uiController.StateTranstion(MissionSelectStateEnum.ConfirmCheck);
        }

        private void CanselAction(InputAction.CallbackContext context)
        {
            uiController.GetComponent<CanvasGroup>().DOFade(0f, 0.5f);

            FindObjectOfType<MainMenuUIController>().StateTranstion(MainMenuState.MainMenuStateEnum.SelectMenu);
            uiController.StateTranstion(MissionSelectStateEnum.Wait);

            uiController.missionScrollView.CloseScrollView();
            uiController.missionStatus.CloseWindow();
        }
    }

    class ConfirmCheckState : MissionSelectState
    {
        //コンストラクタ　初期化
        public ConfirmCheckState(MissionSelectUIController controller)
        {
            State = MissionSelectStateEnum.ConfirmCheck;

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
            uiController.confirmCheck.InitializeMissionUI(uiController.missionScrollView.forcusInfo.missionData);
            uiController.confirmCheck.OpenWindow();

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
            uiController.confirmCheck.ChangeForcus(-1);
        }

        private void RightArrowAction(InputAction.CallbackContext context)
        {
            uiController.confirmCheck.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context)
        {
            //Yesを選択していたらミッションシーンに遷移
            if (uiController.confirmCheck.isConfirm)
            {
                uiController.CallStateFuncArg("SelectMission", new object[1] { uiController.missionScrollView.forcusInfo.missionData.sceneName });

            }
            else
            {
                uiController.confirmCheck.CloseWindow();
                uiController.StateTranstion(MissionSelectStateEnum.SelectMission);
            }
        }
    }

    private List<MissionSelectState> States = new List<MissionSelectState>();

    private MissionSelectState nowState;

    public RectTransform selectArrowRect;

    public MissionScrollView missionScrollView;
    public MissionStatusUI missionStatus;
    public ConfirmCheckUI confirmCheck;

    public InputAction upArrowAct, downArrowAct,leftArrowAct,rightArrowAct, confirmAct,canselAct;

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

        States.Add(new WaitState(this));
        States.Add(new SelectMissionState(this));
        States.Add(new ConfirmCheckState(this));

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
    public void StateTranstion(MissionSelectState.MissionSelectStateEnum transitState)
    {
        nowState.OnExit();

        MissionSelectState newState = States.First(state => state.State == transitState);

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
public abstract class MissionSelectState : IState
{
    public enum MissionSelectStateEnum
    {
        Wait,
        SelectMission,
        ConfirmCheck,
    }

    public MissionSelectStateEnum State;

    protected MissionSelectUIController uiController;

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
