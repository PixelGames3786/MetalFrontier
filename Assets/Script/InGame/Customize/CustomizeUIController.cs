using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using DG.Tweening;
using UnityEngine.InputSystem;
using static ShopControllerState;

public class CustomizeUIController : MonoBehaviour
{
    //�X�e�[�g�ŏ�Ԃ��Ǘ�����

    //�V�[�����ǂݍ��܂ꂽ�ۂɁA�V�[���̃Z�b�g�A�b�v���s���X�e�[�g
    class SetUpState : CustomizeControlState
    {
        //�R���X�g���N�^�@������
        public SetUpState(CustomizeUIController controller)
        {
            state = CustomizeUIState.SetUp;

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
                SaveDataManager.instance.onLoadComplete += SetUp;
            }
        }

        private void SetUp()
        {
            //�����K�V�[�I�u�W�F�N�g����
            GameObject legacyObj = Instantiate(uiController.legacyCustomPrefab);
            uiController.robotControl = legacyObj.GetComponent<CustomRobotController>();

            uiController.robotControl.PartsSetReflect();

            uiController.StateTranstion(CustomizeUIState.SelectMenu);
        }
    }

    class WaitState : CustomizeControlState
    {
        //�R���X�g���N�^�@������
        public WaitState(CustomizeUIController controller)
        {
            state = CustomizeUIState.Wait;

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

        }
    }

    class SelectMenuState : CustomizeControlState
    {
        enum MenuState
        {
            Body,
            Weapon,
        }

        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 1;

        //�R���X�g���N�^�@������
        public SelectMenuState(CustomizeUIController controller)
        {
            state = CustomizeUIState.SelectMenu;

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
            uiController.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);

            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;

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
            uiController.canselAct.performed-= CanselAction;
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
            //���ݑI�����Ă����ނ̏��i�̃X�N���[���r���[��\������
            MenuState select = (MenuState)nowSelectNum;

            switch (select)
            {
                case MenuState.Body:

                    uiController.StateTranstion(CustomizeUIState.SelectBodyMenu);

                    break;

                case MenuState.Weapon:

                    uiController.StateTranstion(CustomizeUIState.SelectWeaponMenu);

                    break;
            }
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(CustomizeUIState.Wait);
            uiController.mainMenuControl.StateTranstion(MainMenuState.MainMenuStateEnum.SelectMenu);

            uiController.GetComponent<CanvasGroup>().DOFade(0f, 0.5f);
        }
    }

    class SelectBodyMenuState : CustomizeControlState
    {
        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 4;

        //�R���X�g���N�^�@������
        public SelectBodyMenuState(CustomizeUIController controller)
        {
            state = CustomizeUIState.SelectBodyMenu;

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
            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition=arrowPosi;

            uiController.bodyMenuParent.gameObject.SetActive(true);
            uiController.selectMenuParent.gameObject.SetActive(false);

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
            uiController.canselAct.performed -= CanselAction;
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
            //���ݑI�����Ă����ނ̏��i�̃X�N���[���r���[��\������
            BodyPartsData.PartsType select = (BodyPartsData.PartsType)nowSelectNum;

            OpenBodySelectScroll(select);
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(CustomizeUIState.SelectMenu);

            uiController.bodyMenuParent.gameObject.SetActive(false);
            uiController.selectMenuParent.gameObject.SetActive(true);
        }

        private void OpenBodySelectScroll(BodyPartsData.PartsType type)
        {
            //�{�^�����\���ɂ��X�N���[���r���[��\���@�����I�ɂ�DOTween�œ���������
            uiController.ButtonsParent.SetActive(false);

            //�����p�[�c����w�肵����ނ̃p�[�c�𔲂��o������ŃX�N���[���r���[���\������
            SaveData saveData = SaveDataManager.instance.saveData;

            List<HavingItem> ItemList = saveData.PartsDataList.Where(parts => parts.itemData is BodyPartsData bodyPartData && bodyPartData.Type == type).ToList();
            List<BodyPartsData> TypeDataList = ItemList.Select(parts => (BodyPartsData)parts.itemData).ToList();

            uiController.partsScroll.OpenScrollView();
            uiController.partsScroll.InitializeUI(TypeDataList, ItemList);

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
    }

    class SelectWeaponMenuState : CustomizeControlState
    {
        private float arrowDefaultYPosi;

        private int nowSelectNum = 0;
        private int maxSelectNum = 3;

        //�R���X�g���N�^�@������
        public SelectWeaponMenuState(CustomizeUIController controller)
        {
            state = CustomizeUIState.SelectWeaponMenu;

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
            Vector2 arrowPosi = uiController.selectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiController.selectArrowRect.localPosition = arrowPosi;

            uiController.weaponMenuParent.gameObject.SetActive(true);
            uiController.selectMenuParent.gameObject.SetActive(false);

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
            uiController.canselAct.performed -= CanselAction;
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
            //���ݑI�����Ă����ނ̏��i�̃X�N���[���r���[��\������
            WeaponPartsData.SetType type=WeaponPartsData.SetType.Arm;
            LegacySettingData.WeaponSetPosi selectPosi = (LegacySettingData.WeaponSetPosi)nowSelectNum;

            if (nowSelectNum > 1) type = WeaponPartsData.SetType.Shoulder;

            OpenWeaponSelectScroll(type,selectPosi);
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(CustomizeUIState.SelectMenu);

            uiController.weaponMenuParent.gameObject.SetActive(false);
            uiController.selectMenuParent.gameObject.SetActive(true);
        }

        private void OpenWeaponSelectScroll(WeaponPartsData.SetType type,LegacySettingData.WeaponSetPosi setPosi)
        {
            //�{�^�����\���ɂ��X�N���[���r���[��\���@�����I�ɂ�DOTween�œ���������
            uiController.ButtonsParent.SetActive(false);

            //�����p�[�c����w�肵����ނ̃p�[�c�𔲂��o������ŃX�N���[���r���[���\������
            SaveData saveData = SaveDataManager.instance.saveData;
            List<HavingItem> ItemList = saveData.WeaponsDataList.Where(parts => parts.itemData is WeaponPartsData weaponData && weaponData.setType == type).ToList();
            List<WeaponPartsData> TypeDataList = ItemList.Select(parts => (WeaponPartsData)parts.itemData).ToList();

            uiController.partsScroll.InitializeUI(TypeDataList, ItemList);
            uiController.partsScroll.OpenScrollView();

            uiController.partsScroll.setPosi = setPosi;

            //��ԑJ��
            uiController.StateTranstion(CustomizeUIState.OpenedScrollView);
        }
    }

    //�ҋ@�X�e�[�g
    public class WaitSelectState : CustomizeControlState
    {
        //�R���X�g���N�^�@������
        public WaitSelectState(CustomizeUIController controller)
        {
            state = CustomizeUIState.WaitButtonSelect;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
                {"OpenBodySelectScroll",OpenBodySelectScroll },
                {"OpenWeaponSelectScroll",OpenWeaponSelectScroll },
            };

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
    public class OpenedScrollViewState : CustomizeControlState
    {
        //�R���X�g���N�^�@������
        public OpenedScrollViewState(CustomizeUIController controller)
        {
            state = CustomizeUIState.OpenedScrollView;

            uiController = controller;

            actionDic = new Dictionary<string, Action>()
            {{"CloseScrollView",CloseScrollView }, };
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
            uiController.canselAct.performed -= CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            uiController.partsScroll.ChangeForcus(-1);
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            uiController.partsScroll.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context) //�m��
        {
            
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiController.StateTranstion(uiController.beforeState.state);

            uiController.partsScroll.CloseScrollView();

            //�A�E�g���C�����\����
            uiController.robotControl.SetAllOutlines(false);
        }


        public void CloseScrollView()
        {
            //�{�^����\�����X�N���[���r���[���\��
            uiController.partsScroll.gameObject.SetActive(false);

            uiController.ButtonsParent.SetActive(true);

            //�A�E�g���C�����\����
            uiController.robotControl.SetAllOutlines(false);

            //�I�[�g�Z�[�u����
            SaveDataManager.instance.SaveFileWriteAsync();

            //��ԑJ��
            uiController.StateTranstion(CustomizeUIState.WaitButtonSelect);
        }
    }

    public CanvasGroup curtainCanvas;

    public CustomRobotController robotControl;

    private List<CustomizeControlState> States = new List<CustomizeControlState>();

    private CustomizeControlState nowState,beforeState;

    public RectTransform selectArrowRect;
    public RectTransform selectMenuParent, bodyMenuParent,weaponMenuParent;

    public GameObject legacyCustomPrefab;

    public GameObject ButtonsParent;
    public PartsScrollView partsScroll;

    public LegacyStatusUI statusUI;

    public MainMenuUIController mainMenuControl;

    [NonSerialized]
    public InputAction upArrowAct, downArrowAct, leftArrowAct, rightArrowAct, confirmAct, canselAct;

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
        canselAct = testControl.UI.Cansel;

        upArrowAct.Enable();
        downArrowAct.Enable();
        leftArrowAct.Enable();
        rightArrowAct.Enable();
        confirmAct.Enable();
        canselAct.Enable();

        States.Add(new SetUpState(this));
        States.Add(new SelectMenuState(this));

        
        States.Add(new WaitState(this));
        States.Add(new WaitSelectState(this));
        States.Add(new SelectBodyMenuState(this));
        States.Add(new SelectWeaponMenuState(this));
        States.Add(new OpenedScrollViewState(this));

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
    public void StateTranstion(CustomizeControlState.CustomizeUIState transitState)
    {
        nowState.OnExit();

        CustomizeControlState newState = States.First(state => state.state == transitState);

        //�k���`�F
        if (newState == null) throw new System.Exception("�J�ڂ���X�e�[�g���Ȃ��炵����");

        beforeState = nowState;
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
}

//���X�e�[�g�̒�`
public abstract class CustomizeControlState : IState
{
    public enum CustomizeUIState
    {
        SetUp,
        Wait,
        SelectMenu,     //�{�f�B�p�[�c������p�[�c���I�����郁�j���[
        SelectBodyMenu, //����́A�ǂ̃p�[�c��ύX���邩�I�����郁�j���[
        SelectWeaponMenu, //���r��E�r�A�ǂ̃p�[�c��ύX���邩�I�����郁�j���[
        WaitButtonSelect,
        OpenedScrollView,
    }

    public CustomizeUIState state;

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
