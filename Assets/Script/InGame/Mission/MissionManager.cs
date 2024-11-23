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
                SaveDataManager.instance.OnLoadComplete += MissionSetUp;
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
            TestControls testControl = new TestControls();
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


    // Start is called before the first frame update
    void Start()
    {
        condition=GetComponent<MissionCondition>();

        //ステートに追加
        states.Add(new CountDownState(this));
        states.Add(new InMissionState(this));
        states.Add(new WaitForReturnState(this));

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
        Clear, //ミッションクリア
        Fail, //ミッション失敗
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

