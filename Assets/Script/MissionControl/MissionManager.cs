using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using MissionManageState;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.InputSystem;

public class MissionManager : MonoBehaviour
{
    private List<MissionState> states = new List<MissionState>();

    //ミッション中に入手したアイテム　クリアした際に入手する
    public List<HavingItem> getItems { get; private set; } = new List<HavingItem>();

    private MissionState nowState;

    //ミッションの状態を管理するCondition
    public MissionCondition condition { get; private set; }

    //全ユニット（プレイヤー・敵含め）
    public List<UnitBase> allUnit;

    //死亡ハンドラー（全てのユニットの）
    public List<DeathHandler> deathHandlers;

    public CanvasGroup curtain;

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

            StateTranstion(MissionState.MissionStateEnum.WaitForReturn);
        }

        if (isFail)
        {
            StateTranstion(MissionState.MissionStateEnum.WaitForReturn);
        }
    }

    //敵や自キャラを操作できなくする
    public void MissionStop()
    {
        //プレイヤーと敵を全停止
        foreach (UnitBase unit in allUnit)
        {
            unit?.UnitDeactivate();
        }
    }

    //ミッションを失敗させる
    public void MissionFail()
    {
        condition.MissionEnd();
        MissionStop();
        resultManager.ResultSetUp();
    }

    //ミッションから帰還を選択したとき
    public void MissionDrop()
    {
        condition.MissionEnd();
        MissionStop();
        GetReward();
        resultManager.ResultSetUp();
    }

    //クリア時に、一時保存していたアイテムを手に入れたりする処理
    public void GetReward()
    {
        if (!condition.isMissionClear) return; //クリアしていなかったら手に入らない

        SaveData saveData = SaveDataManager.instance.saveData;

        foreach (ItemData data in condition.missionData.clearGetItems)
        {
            getItems.Add(new HavingItem(data.ItemNumber));
        }

        //報酬入手
        saveData.ColChange(condition.missionData.clearGetCol);
        saveData.AddItemRange(getItems.ToArray());

        //ミッション解放
        foreach (MissionData mission in condition.missionData.clearOpenMission)
        {
            saveData.OpenMission(mission.missionNumber);
        }

        //非同期でセーブデータ保存
        SaveDataManager.instance.SaveFileWriteAsync();
    }

    //ミッション中に入手したアイテムを一時保存　入手したアイテムはクリア時に実際にプレイヤーに渡される
    public void GetItem(int itemNum)
    {
        getItems.Add(new HavingItem(itemNum));
    }

    //ミッション中に新たにユニットが作られた際に登録する
    public void AddUnit(UnitBase newUnit)
    {
        allUnit.Add(newUnit);
    }

    //ミッション中に新たなユニットが作られた際の死亡ハンドラーを登録
    public void AddDeathHandler(DeathHandler newHandler)
    {
        deathHandlers.Add(newHandler);
        newHandler.OnDeathWithName += condition.AddDefeatedUnit;
    }

    //黒幕をフェードインしてシーンを変える
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

namespace MissionManageState
{
    //シーンが読み込まれた際に、シーンのセットアップを行うステート
    public class CountDownState : MissionState
    {
        private MissionCondition condition;

        //コンストラクタ　初期化
        public CountDownState(MissionManager manager)
        {
            State = MissionStateEnum.CountDown;

            missionManager = manager;
            condition = manager.condition;
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
            condition.onConditionChange += missionManager.CheckClearOrFail;

            condition.MissionSetUp();

            //敵・プレイヤーのセットアップを行う
            foreach (UnitBase unit in missionManager.allUnit)
            {
                unit?.UnitSetUp();
            }

            //死亡ハンドラーのデリゲート登録
            foreach (DeathHandler handler in missionManager.deathHandlers)
            {
                handler.OnDeathWithName+= missionManager.condition.AddDefeatedUnit;
            }

            //セットアップ終了したなら
            //黒幕をフェードアウトして、カウントダウンに入る 一旦直接操作できるようにする
            missionManager.curtain.DOFade(0f, 0.5f).OnComplete(() =>
            {
                missionManager.condition.MissionStart();
                missionManager.curtain.gameObject.SetActive(false);

                foreach (UnitBase unit in missionManager.allUnit)
                {
                    unit?.UnitActivate();
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

            Animator[] animators = GameObject.FindObjectsOfType<Animator>();

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
            if (nowSelectNum != 0) AudioManager.instance.PlayAudio(AudioData.audioNameEnum.MenuArrowChange, false);

            nowSelectNum--;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            UpdateArrowPosition();
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            if (nowSelectNum != maxSelectNum) AudioManager.instance.PlayAudio(AudioData.audioNameEnum.MenuArrowChange, false);

            nowSelectNum++;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            UpdateArrowPosition();
        }

        private void UpdateArrowPosition()
        {
            Vector2 arrowPosi = missionManager.menuSelectArrow.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            missionManager.menuSelectArrow.localPosition = arrowPosi;
        }

        public void ConfirmAction(InputAction.CallbackContext context)
        {
            AudioManager.instance.PlayAudio(AudioData.audioNameEnum.MenuConfirm, false);

            //現在選択している種類の商品のスクロールビューを表示する
            MenuOptions option = (MenuOptions)nowSelectNum;

            switch (option)
            {
                case MenuOptions.Continue:

                    BackToMission();

                    break;

                case MenuOptions.Drop:

                    MissionDrop();

                    break;
            }
        }

        private void BackToMission()
        {
            missionManager.menuGroup.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() =>
            {
                missionManager.menuGroup.gameObject.SetActive(false);

                WorldResume();

                missionManager.StateTranstion(MissionStateEnum.InMission);
            });
        }

        private void MissionDrop()
        {
            missionManager.menuGroup.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() =>
            {
                missionManager.menuGroup.gameObject.SetActive(false);

                WorldResume();

                missionManager.MissionDrop();
            });

            OnExit(); //InputActionを解除
        }

        //停止していた時間を再度動かす
        private void WorldResume()
        {
            Time.timeScale = 1;

            Animator[] animators = GameObject.FindObjectsOfType<Animator>();

            foreach (var animator in animators)
            {
                animator.enabled = true;
            }
        }

    }

    public class WaitForReturnState : MissionState
    {
        public Action<int> waitCountDown;

        private int waitReturnCount = 60;

        private Coroutine waitReturnCoroutine;

        private CancellationTokenSource _cts;

        private InputAction BackAction; //Xを押して拠点に帰還するアクション

        //コンストラクタ　初期化
        public WaitForReturnState(MissionManager manager)
        {
            State = MissionStateEnum.WaitForReturn;

            missionManager = manager;
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
            PlayerUIController uiController = GameObject.FindObjectOfType<PlayerUIController>();
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
                missionManager.GetReward();
                missionManager.MissionStop();
                missionManager.condition.MissionEnd();
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
            //カウントダウンをキャンセル
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            missionManager.GetReward();
            missionManager.MissionStop();
            missionManager.condition.MissionEnd();
            missionManager.resultManager.ResultSetUp();
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
}


