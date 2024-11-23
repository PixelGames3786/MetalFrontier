using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using DG.Tweening;
using static UnityEditor.Progress;

public class CustomizeUIController : MonoBehaviour
{
    //�X�e�[�g�ŏ�Ԃ��Ǘ�����

    //�V�[�����ǂݍ��܂ꂽ�ۂɁA�V�[���̃Z�b�g�A�b�v���s���X�e�[�g
    public class SetUpState : UIControllerState
    {
        //�R���X�g���N�^�@������
        public SetUpState(CustomizeUIController controller)
        {
            State = CustomizeUIState.SetUp;

            uiController = controller;

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
                SetUp();
            }
            else
            {
                //�I���܂ő҂�
                SaveDataManager.instance.OnLoadComplete += SetUp;
            }
        }

        private void SetUp()
        {
            //�����K�V�[�I�u�W�F�N�g����
            GameObject legacyObj = Instantiate(uiController.legacyCustomPrefab);
            uiController.robotControl = legacyObj.GetComponent<CustomRobotController>();

            uiController.robotControl.PartsSetReflect();

            uiController.curtainCanvas.DOFade(0f, 0.5f).OnComplete(() => 
            { 
                uiController.curtainCanvas.gameObject.SetActive(false);
                uiController.StateTranstion(CustomizeUIState.WaitButtonSelect);
            });
        }

    }

    //�ҋ@�X�e�[�g
    public class WaitSelectState : UIControllerState
    {
        //�R���X�g���N�^�@������
        public WaitSelectState(CustomizeUIController controller)
        {
            State = CustomizeUIState.WaitButtonSelect;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
                {"OpenMissionScroll",OpenMissionScroll }
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
                {"OpenBodySelectScroll",OpenBodySelectScroll },
                {"OpenWeaponSelectScroll",OpenWeaponSelectScroll },
            };

        }

        public void OpenMissionScroll()
        {
            //�{�^�����\���ɂ��X�N���[���r���[��\���@�����I�ɂ�DOTween�œ���������
            uiController.ButtonsParent.SetActive(false);

            uiController.missionScroll.InitializeUI(SaveDataManager.instance.saveData.missionDataList);

            uiController.missionCanvas.gameObject.SetActive(true);
            uiController.missionCanvas.DOFade(1f, 0.5f).OnComplete(() => 
            {
                //��ԑJ��
                uiController.StateTranstion(CustomizeUIState.MissionScrollView);
            });
        }

        public void OpenBodySelectScroll(object[] args)
        {
            //object�^�œn���ꂽ������ϊ�����
            BodyPartsData.PartsType type = (BodyPartsData.PartsType)args[0];


            //�{�^�����\���ɂ��X�N���[���r���[��\���@�����I�ɂ�DOTween�œ���������
            uiController.ButtonsParent.SetActive(false);

            //�����p�[�c����w�肵����ނ̃p�[�c�𔲂��o������ŃX�N���[���r���[���\������
            SaveData saveData = SaveDataManager.instance.saveData;

            List<HavingItem> ItemList = saveData.PartsDataList.Where(parts => parts.itemData is BodyPartsData bodyPartData && bodyPartData.Type == type).ToList();
            List<BodyPartsData> TypeDataList = ItemList.Select(parts => (BodyPartsData)parts.itemData).ToList();

            uiController.partsScroll.InitializeUI(TypeDataList, ItemList);
            uiController.partsScroll.gameObject.SetActive(true);


            //�I�𒆂̃��b�V���̃A�E�g���C����\��
            //���ݑ������Ă���I��������ނ̃f�[�^���擾
            uiController.robotControl.SetAllOutlines(false);

            BodyPartsData partsData = (BodyPartsData)saveData.settingData.PartsNumber[type].itemData;

            foreach (KeyValuePair<string, Mesh> keyValue in partsData.ObjnameMeshPairs)
            {
                uiController.robotControl.SetMeshOutline(keyValue.Key, true);
            }

            //��ԑJ��
            uiController.StateTranstion(CustomizeUIState.OpenedScrollView);
        }

        public void OpenWeaponSelectScroll(object[] args)
        {
            //object�^�œn���ꂽ������ϊ�����
            WeaponPartsData.SetType type = (WeaponPartsData.SetType)args[0];
            LegacySettingData.WeaponSetPosi setPosi = (LegacySettingData.WeaponSetPosi)args[1];


            //�{�^�����\���ɂ��X�N���[���r���[��\���@�����I�ɂ�DOTween�œ���������
            uiController.ButtonsParent.SetActive(false);

            //�����p�[�c����w�肵����ނ̃p�[�c�𔲂��o������ŃX�N���[���r���[���\������
            SaveData saveData = SaveDataManager.instance.saveData;
            List<HavingItem> ItemList = saveData.WeaponsDataList.Where(parts => parts.itemData is WeaponPartsData weaponData && weaponData.setType == type).ToList();
            List<WeaponPartsData> TypeDataList = ItemList.Select(parts => (WeaponPartsData)parts.itemData).ToList();

            uiController.partsScroll.InitializeUI(TypeDataList, ItemList);
            uiController.partsScroll.gameObject.SetActive(true);

            uiController.partsScroll.setPosi = setPosi;


            //�I�𒆂̃��b�V���̃A�E�g���C����\��
            //���ݑ������Ă���I��������ނ̃f�[�^���擾
            /*
            uiController.robotControl.SetAllOutlines(false);

            BodyPartsData partsData = (BodyPartsData)ItemDataBaseController.instance.itemDataBase.GetItem(saveData.settingData.PartsNumber[type]);

            foreach (KeyValuePair<string, Mesh> keyValue in partsData.ObjnameMeshPairs)
            {
                uiController.robotControl.SetMeshOutline(keyValue.Key, true);
            }

            */

            //��ԑJ��
            uiController.StateTranstion(CustomizeUIState.OpenedScrollView);
        }

    }

    //�X�N���[���r���[�ł̑I��҂��X�e�[�g
    public class OpenedScrollViewState : UIControllerState
    {
        //�R���X�g���N�^�@������
        public OpenedScrollViewState(CustomizeUIController controller)
        {
            State = CustomizeUIState.OpenedScrollView;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {{"CloseScrollView",CloseScrollView }, };
        }

        public void CloseScrollView()
        {
            //�{�^����\�����X�N���[���r���[���\��
            uiController.partsScroll.gameObject.SetActive(false);

            uiController.ButtonsParent.SetActive(true);

            //�A�E�g���C�����\����
            uiController.robotControl.SetAllOutlines(false);

            //��ԑJ��
            uiController.StateTranstion(CustomizeUIState.WaitButtonSelect);
        }
    }

    //�~�b�V�����X�N���[���r���[�ł̑I��҂��X�e�[�g
    public class MissionScrollViewState : UIControllerState
    {
        //�R���X�g���N�^�@������
        public MissionScrollViewState(CustomizeUIController controller)
        {
            State = CustomizeUIState.MissionScrollView;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
                {"BackButton",BackButton }
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
                { "SelectMission",SelectMission},
            };
        }

        //�~�b�V�������I�����ꂽ�Ƃ�
        public void SelectMission(object[] args)
        {
            string sceneName = (string)args[0];

            uiController.curtainCanvas.gameObject.SetActive(true);
            uiController.curtainCanvas.DOFade(1f, 1f).OnComplete(() => 
            {
                SceneChangeManager.instance.StartCoroutine("SceneTransition", sceneName);
            });
        }

        //�p�[�c�I���ɖ߂�
        public void BackButton()
        {
            //�{�^�����\���ɂ��X�N���[���r���[��\���@�����I�ɂ�DOTween�œ���������
            uiController.ButtonsParent.SetActive(true);

            uiController.missionCanvas.DOFade(0f, 0.5f).OnComplete(() =>
            {
                uiController.missionCanvas.gameObject.SetActive(false);

                //��ԑJ��
                uiController.StateTranstion(CustomizeUIState.WaitButtonSelect);
            });
        }
    }

    public CanvasGroup curtainCanvas;

    public CustomRobotController robotControl;

    private List<UIControllerState> States = new List<UIControllerState>();

    private UIControllerState nowState;

    public GameObject legacyCustomPrefab;

    public GameObject ButtonsParent;
    public PartsScrollView partsScroll;
    public MissionScrollView missionScroll;
    public CanvasGroup missionCanvas;

    public LegacyStatusUI statusUI;

    public ShopUIController shopUIController;

    // Start is called before the first frame update
    void Start()
    {
        States.Add(new SetUpState(this));
        States.Add(new WaitSelectState(this));
        States.Add(new OpenedScrollViewState(this));
        States.Add(new MissionScrollViewState(this));

        nowState = States[0];

        nowState.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        nowState.OnUpdate();
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

    //���K�V�[�p�[�c�̕ύX
    public void LegacyPartsChange(BodyPartsData.PartsType type, int partsNum, HavingItem having)
    {
        SaveDataManager.instance.saveData.LegacyPartsChange(type, having);

        robotControl.PartsSetReflect();

        //�X�e�[�^�X�ɔ��f
        StatusUIDataReflesh();
    }

    //����p�[�c�̕ύX
    public void WeaponPartsChange(LegacySettingData.WeaponSetPosi posi, int partsNum, HavingItem having)
    {
        SaveDataManager.instance.saveData.WeaponPartsChange(posi, having);

        robotControl.PartsSetReflect();
    }

    //����p�[�c�̎��O��
    public void WeaponPartsRemove(LegacySettingData.WeaponSetPosi posi, int partsNum, HavingItem having)
    {
        SaveDataManager.instance.saveData.WeaponPartsRemove(posi, having);

        robotControl.PartsSetReflect();
    }

    //�X�e�[�g�̐؂�ւ�
    public void StateTranstion(UIControllerState.CustomizeUIState transitState)
    {
        nowState.OnExit();

        UIControllerState newState = States.First(state => state.State == transitState);

        //�k���`�F
        if (newState == null) throw new System.Exception("�J�ڂ���X�e�[�g���Ȃ��炵����");

        nowState = newState;

        nowState.OnEnter();
    }

    //�ۑ�
    public void SaveButtonOnClick()
    {
        SaveDataManager.instance.SaveFileWrite();
    }

    //�X�e�[�^�X�\��
    public void StatusButtonOnClick()
    {
        if (statusUI.gameObject.activeSelf)
        {
            statusUI.gameObject.SetActive(false);
            
        }
        else
        {
            StatusUIDataReflesh();

            statusUI.gameObject.SetActive(true);
        }
    }

    //�X�e�[�^�XUI�f�[�^�X�V
    public void StatusUIDataReflesh()
    {
        //�f�[�^�擾
        LegacySettingData settingData = SaveDataManager.instance.saveData.settingData;

        //�{�f�B�p�[�c���f
        List<BodyPartsData> bodyParts = new List<BodyPartsData>();
        Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData> weaponParts = new Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData>();

        //�S�p�[�c�̃f�[�^�擾
        foreach (KeyValuePair<BodyPartsData.PartsType, HavingItem> keyValue in settingData.PartsNumber)
        {
            BodyPartsData Data = (BodyPartsData)keyValue.Value.itemData;
            bodyParts.Add(Data);
        }

        statusUI.StatusInitialize(bodyParts);
        statusUI.UIInitialize();
    }

    //�V���b�v�{�^��
    public void ShopButtonOnClick()
    {
        shopUIController.StateTranstion(ShopControllerState.ShopState.SelectGenre);
    }
}

//���X�e�[�g�̒�`
public abstract class UIControllerState : IState
{
    public enum CustomizeUIState
    {
        SetUp,
        WaitButtonSelect,
        OpenedScrollView,
        MissionScrollView,
    }

    public CustomizeUIState State;

    protected CustomizeUIController uiController;

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
