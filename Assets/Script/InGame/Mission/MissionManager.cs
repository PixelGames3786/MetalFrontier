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
    //�V�[�����ǂݍ��܂ꂽ�ۂɁA�V�[���̃Z�b�g�A�b�v���s���X�e�[�g
    public class CountDownState : MissionState
    {
        //�R���X�g���N�^�@������
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

        //�X�e�[�g�ɓ������ۂɁA�Z�b�g�A�b�v���s��
        public override void OnEnter()
        {
            //�������Z�[�u�f�[�^�̃��[�h���ς�ł���̂Ȃ�
            if (SaveDataManager.instance.isLoadComplete)
            {
                MissionSetUp();
            }
            else
            {
                //�I���܂ő҂�
                SaveDataManager.instance.OnLoadComplete += MissionSetUp;
            }
        }

        //�~�b�V�����̐����@���L���������Ȃ�
        private void MissionSetUp()
        {
            //Condition���ύX���ꂽ�ۂɃN���A��������邽�߂̃f���Q�[�g
            missionManager.condition.onConditionChange += missionManager.CheckClearOrFail;

            //�v���C���[�̃Z�b�g�A�b�v
            missionManager.playerInput.LegacySetUp();
            missionManager.playerInput.OnDeathWithName += missionManager.condition.AddDefeatedLegacy;

            //�G�̃Z�b�g�A�b�v
            foreach (EnemyControlTest enemy in missionManager.enemys)
            {
                enemy.LegacySetUp();
                enemy.OnDeathWithName += missionManager.condition.AddDefeatedLegacy;

            }

            //�Z�b�g�A�b�v�I�������Ȃ�
            //�������t�F�[�h�A�E�g���āA�J�E���g�_�E���ɓ��� ��U���ڑ���ł���悤�ɂ���
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

                //�X�e�[�g��؂�ւ���
                missionManager.StateTranstion(MissionStateEnum.InMission);

            });
        }

    }

    public class InMissionState : MissionState
    {
        //�R���X�g���N�^�@������
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

        //�X�e�[�g�ɓ������ۂɁA�Z�b�g�A�b�v���s��
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

        private InputAction BackAction; //X�������ċ��_�ɋA�҂���A�N�V����

        //�R���X�g���N�^�@������
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

        //�X�e�[�g�ɓ������ۂɁA��ʏ�ɃJ�E���g�_�E�����o��
        public override void OnEnter()
        {
            //Action�Z�b�g�A�b�v
            TestControls testControl = new TestControls();
            BackAction = testControl.Mission.BackToBase;
            BackAction.performed += BackButtonPress;
            BackAction.Enable();

            //BackToBaseUI���Z�b�g�A�b�v
            PlayerUIController uiController =FindObjectOfType<PlayerUIController>();
            uiController.StartReturnCountDown();
            waitCountDown += uiController.ReturnCountDownChange;

            //�L�����Z���g�[�N����ݒ肵�J�E���g�_�E�����n�߂�
            _cts = new CancellationTokenSource();
            StartCountdown(_cts.Token).Forget();
        }

        public override void OnUpdate()
        {
        }

        //�A�҃J�E���g�_�E���J�n
        private async UniTaskVoid StartCountdown(CancellationToken cancellationToken)
        {
            try
            {
                while (waitReturnCount > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested(); // �L�����Z���v�����m�F

                    waitReturnCount--;
                    waitCountDown?.Invoke(waitReturnCount); // �o�^���ꂽ�f���Q�[�g���Ăяo��

                    await UniTask.Delay(1000, cancellationToken: cancellationToken); // 1�b�ҋ@
                }

                Debug.Log("�J�E���g�_�E���I���A�A�ҊJ�n�I");
                missionManager.MissionStop();
                missionManager.resultManager.ResultSetUp();
            }
            catch (OperationCanceledException)
            {
                Debug.Log("�J�E���g�_�E������~����܂���");
            }
        }

        //�A�҃{�^���������ꂽ�ۂ̏���
        private void BackButtonPress(InputAction.CallbackContext context)
        {
            print("�A�҃{�^����������܂�����");

            //�J�E���g�_�E�����L�����Z��
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            missionManager.MissionStop();
            missionManager.resultManager.ResultSetUp();
        }


    }


    private List<MissionState> states = new List<MissionState>();

    private MissionState nowState;

    //�~�b�V�����̏�Ԃ��Ǘ�����Condition
    public MissionCondition condition { get; private set; }

    //�v���C���[
    public RobotPlayerInput playerInput;

    public EnemyControlTest[] enemys;

    [SerializeField]
    private CanvasGroup curtain;

    public MissionResultManager resultManager;


    // Start is called before the first frame update
    void Start()
    {
        condition=GetComponent<MissionCondition>();

        //�X�e�[�g�ɒǉ�
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

    //�~�b�V�������������s���𔻒肷��
    public void CheckClearOrFail()
    {
        //���Ƀ~�b�V�������I����Ă���Ȃ画�肵�Ȃ�
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

    //�G�⎩�L�����𑀍�ł��Ȃ�����
    public void MissionStop()
    {
        //�v���C���[�ƓG��S��~
        playerInput?.SetWorking(false);
        playerInput?.SetOparete(false);

        foreach (EnemyControlTest enemy in enemys)
        {
            enemy?.SetWorking(false);
            enemy?.SetOparete(false);
        }
    }

    //�~�b�V���������s������
    public void MissionFail()
    {
        MissionStop();

        resultManager.ResultSetUp();
    }

    //�N���A���ɁA�A�C�e������ɓ��ꂽ�肷�鏈��
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

    //�X�e�[�g�̐؂�ւ�
    public void StateTranstion(MissionState.MissionStateEnum transitState)
    {
        nowState.OnExit();

        MissionState newState = states.First(state => state.State == transitState);

        //�k���`�F
        if (newState == null) throw new System.Exception("�J�ڂ���X�e�[�g���Ȃ��炵����");

        nowState = newState;

        nowState.OnEnter();
    }
}

//���X�e�[�g�̒�`
public abstract class MissionState : IState
{
    public enum MissionStateEnum
    {
        CurtainFadeOut, //�V�[�����[�h��@�������t�F�[�h�A�E�g
        CountDown, //�~�b�V�����J�n�܂ł̃J�E���g�_�E��
        InMission, //�~�b�V������
        WaitForReturn, //�~�b�V�����I����@�A�ґ҂����
        Clear, //�~�b�V�����N���A
        Fail, //�~�b�V�������s
    }

    public MissionStateEnum State;

    public MissionManager missionManager;

    //�Ăׂ�֐����܂Ƃ߂�Dictionary
    protected Dictionary<string, Action> actionDic;

    //��������̊֐����܂Ƃ߂��
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
            throw new System.Exception("�ĂԊ֐����Ȃ����I");
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
