using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static CustomizeUIController;
using static UnityEditor.VersionControl.Asset;
using Unity.VisualScripting;
using UnityEngine.InputSystem.XR;
using System.Linq;
using static MissionState;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.InputSystem;
using static ShopControllerState;

public class MissionManager : MonoBehaviour
{
    //シーンが読み込まれた際に、シーンのセットアップを行うステート
    public class CountDownState : MissionState
    {
        //コンストラクタ　初期化
        public CountDownState(MissionManager manager)
        {
            State = MissionStateEnum.CountDown;

            missionManager = manager;

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
                MissionSetUp();
            }
            else
            {
                //終わるまで待つ
                SaveDataManager.instance.onLoadComplete += MissionSetUp;
            }
        }

        //ミッションの生成　自キャラ生成など
        private void MissionSetUp()
        {
            //Conditionが変更された際にクリア判定をするためのデリゲート
            missionManager.condition.onConditionChange += missionManager.CheckClearOrFail;

            //プレイヤーのセットアップ
            missionManager.playerInput.LegacySetUp();
            missionManager.playerInput.OnDeathWithName += missionManager.condition.AddDefeatedLegacy;

            //敵のセットアップ
            foreach (EnemyControlTest enemy in missionManager.enemys)
            {
                enemy.LegacySetUp();
                enemy.OnDeathWithName += missionManager.condition.AddDefeatedLegacy;

            }

            //セットアップ終了したなら
            //黒幕をフェードアウトして、カウントダウンに入る 一旦直接操作できるようにする
            missionManager.curtain.DOFade(0f, 0.5f).OnComplete(() =>
            {
                missionManager.curtain.gameObject.SetActive(false);

                missionManager.playerInput.SetWorking(true);
                missionManager.playerInput.SetOparete(true);

                foreach (EnemyControlTest enemy in missionManager.enemys)
                {
                    enemy.SetWorking(true);
                    enemy.SetOparete(true);
                }

                //ステートを切り替える
                missionManager.StateTranstion(MissionStateEnum.InMission);

            });
        }

    }

    public class InMissionState : MissionState
    {
        private bool isPaused = false;

        //コンストラクタ　初期化
        public InMissionState(MissionManager manager)
        {
            State = MissionStateEnum.InMission;

            missionManager = manager;

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
            missionManager.menuAction.performed += OpenMenu;
        }

        public override void OnExit()
        {
            missionManager.menuAction.performed -= OpenMenu;
        }

        private void OpenMenu(InputAction.CallbackContext context)
        {
            Time.timeScale = 0;

            Animator[] animators = FindObjectsOfType<Animator>();

            foreach (var animator in animators)
            {
                animator.enabled = false;
            }

            missionManager.menuGroup.gameObject.SetActive(true);
            missionManager.menuGroup.DOFade(1f, 0.5f).SetUpdate(true).OnComplete(() => 
            {
                missionManager.StateTranstion(MissionStateEnum.Menu);
            });

        }
    }

    public class MenuState : MissionState
    {
        private enum MenuOptions
        {
            Continue,
            Drop,
        }

        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 1;

        private InputAction upArrowAct, downArrowAct, confirmAct;

        //コンストラクタ　初期化
        public MenuState(MissionManager manager)
        {
            State = MissionStateEnum.Menu;

            missionManager = manager;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

            //Actionのセットアップ
            InputControls testControl = new InputControls();

            upArrowAct = testControl.UI.UpArrow;
            downArrowAct = testControl.UI.DownArrow;
            confirmAct = testControl.UI.Confirm;

            upArrowAct.performed += UpArrowAction;
            downArrowAct.performed += DownArrowAction;
            confirmAct.performed += ConfirmAction;

            arrowDefaultYPosi = missionManager.menuSelectArrow.localPosition.y;

        }

        //ステートに入った際に、セットアップを行う
        public override void OnEnter()
        {
            upArrowAct.Enable();
            downArrowAct.Enable();
            confirmAct.Enable();

        }

        public override void OnExit()
        {
            upArrowAct.Disable();
            downArrowAct.Disable();
            confirmAct.Disable();
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = missionManager.menuSelectArrow.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            missionManager.menuSelectArrow.localPosition = arrowPosi;
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = missionManager.menuSelectArrow.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            missionManager.menuSelectArrow.localPosition = arrowPosi;
        }

        public void ConfirmAction(InputAction.CallbackContext context)
        {
            //現在選択している種類の商品のスクロールビューを表示する
            MenuOptions option = (MenuOptions)nowSelectNum;

            switch (option)
            {
                case MenuOptions.Continue:

                    BackToMission();

                    break;

                case MenuOptions.Drop:

                    MissionFail();

                    break;
            }
        }

        private void BackToMission()
        {
            missionManager.menuGroup.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() =>
            {
                missionManager.menuGroup.gameObject.SetActive(false);
                Time.timeScale = 1;

                Animator[] animators = FindObjectsOfType<Animator>();

                foreach (var animator in animators)
                {
                    animator.enabled = true;
                }

                missionManager.StateTranstion(MissionStateEnum.InMission);
            });
        }

        private void MissionFail()
        {
            missionManager.menuGroup.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() =>
            {
                missionManager.menuGroup.gameObject.SetActive(false);
                Time.timeScale = 1;

                Animator[] animators = FindObjectsOfType<Animator>();

                foreach (var animator in animators)
                {
                    animator.enabled = true;
                }

                missionManager.MissionFail();
            });
        }

    }

    public class WaitForReturnState : MissionState
    {
        public Action<int> waitCountDown;

        private int waitReturnCount=60;

        private Coroutine waitReturnCoroutine;

        private CancellationTokenSource _cts;

        private InputAction BackAction; //Xを押して拠点に帰還するアクション

        //コンストラクタ　初期化
        public WaitForReturnState(MissionManager manager)
        {
            State = MissionStateEnum.WaitForReturn;

            missionManager = manager;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };
        }

        //ステートに入った際に、画面上にカウントダウンを出す
        public override void OnEnter()
        {
            //Actionセットアップ
            InputControls testControl = new InputControls();
            BackAction = testControl.Mission.BackToBase;
            BackAction.performed += BackButtonPress;
            BackAction.Enable();

            //BackToBaseUIをセットアップ
            PlayerUIController uiController =FindObjectOfType<PlayerUIController>();
            uiController.StartReturnCountDown();
            waitCountDown += uiController.ReturnCountDownChange;

            //キャンセルトークンを設定しカウントダウンを始める
            _cts = new CancellationTokenSource();
            StartCountdown(_cts.Token).Forget();
        }

        public override void OnUpdate()
        {
        }

        //帰還カウントダウン開始
        private async UniTaskVoid StartCountdown(CancellationToken cancellationToken)
        {
            try
            {
                while (waitReturnCount > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested(); // キャンセル要求を確認

                    waitReturnCount--;
                    waitCountDown?.Invoke(waitReturnCount); // 登録されたデリゲートを呼び出し

                    await UniTask.Delay(1000, cancellationToken: cancellationToken); // 1秒待機
                }

                Debug.Log("カウントダウン終了、帰還開始！");
                missionManager.MissionStop();
                missionManager.resultManager.ResultSetUp();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("カウントダウンが停止されました");
            }
        }

        //帰還ボタンが押された際の処理
        private void BackButtonPress(InputAction.CallbackContext context)
        {
            print("帰還ボタンが押されましたよ");

            //カウントダウンをキャンセル
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            missionManager.MissionStop();
            missionManager.resultManager.ResultSetUp();
        }


    }


    private List<MissionState> states = new List<MissionState>();

    private MissionState nowState;

    //ミッションの状態を管理するCondition
    public MissionCondition condition { get; private set; }

    //プレイヤー
    public RobotPlayerInput playerInput;

    public EnemyControlTest[] enemys;

    [SerializeField]
    private CanvasGroup curtain;

    public MissionResultManager resultManager;

    public InputAction menuAction;
    public CanvasGroup menuGroup;
    public RectTransform menuSelectArrow;


    // Start is called before the first frame update
    void Start()
    {
        condition=GetComponent<MissionCondition>();

        //InputActionを登録
        InputControls control = new InputControls();

        menuAction = control.Mission.Menu;
        menuAction.Enable();

        //ステートに追加
        states.Add(new CountDownState(this));
        states.Add(new InMissionState(this));
        states.Add(new WaitForReturnState(this));
        states.Add(new MenuState(this));

        nowState = states[0];

        nowState.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        nowState.OnUpdate();
    }

    //ミッション成功か失敗かを判定する
    public void CheckClearOrFail()
    {
        //既にミッションが終わっているなら判定しない
        if (condition.isMissionEnd) return;

        bool isClear = condition.ClearConditionCheck();
        bool isFail = condition.FailConditionCheck();

        if (isClear)
        {
            condition.MissionClear();

            OnClear();

            StateTranstion(MissionStateEnum.WaitForReturn);
        }

        if (isFail)
        {
            StateTranstion(MissionStateEnum.WaitForReturn);
        }
    }

    //敵や自キャラを操作できなくする
    public void MissionStop()
    {
        //プレイヤーと敵を全停止
        playerInput?.SetWorking(false);
        playerInput?.SetOparete(false);

        foreach (EnemyControlTest enemy in enemys)
        {
            enemy?.SetWorking(false);
            enemy?.SetOparete(false);
        }
    }

    //ミッションを失敗させる
    public void MissionFail()
    {
        MissionStop();

        resultManager.ResultSetUp();
    }

    //クリア時に、アイテムを手に入れたりする処理
    public void OnClear()
    {
        SaveData saveData = SaveDataManager.instance.saveData;

        saveData.ColChange(condition.missionData.clearGetCol);
        saveData.AddItemRange(condition.missionData.clearGetItems.Select(item=>item.ItemNumber).ToArray());
    }


    public void CurtainTransition(string transtionScene)
    {
        curtain.gameObject.SetActive(true);
        curtain.DOFade(1f, 1f).OnComplete(() =>
        {
            SceneChangeManager.instance.StartCoroutine("SceneTransition", transtionScene);
        });
    }

    //ステートの切り替え
    public void StateTranstion(MissionState.MissionStateEnum transitState)
    {
        nowState.OnExit();

        MissionState newState = states.First(state => state.State == transitState);

        //ヌルチェ
        if (newState == null) throw new System.Exception("遷移するステートがないらしいよ");

        nowState = newState;

        nowState.OnEnter();
    }
}

//基底ステートの定義
public abstract class MissionState : IState
{
    public enum MissionStateEnum
    {
        CurtainFadeOut, //シーンロード後　黒幕をフェードアウト
        CountDown, //ミッション開始までのカウントダウン
        InMission, //ミッション中
        WaitForReturn, //ミッション終了後　帰還待ち状態
        Menu,
    }

    public MissionStateEnum State;

    public MissionManager missionManager;

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

