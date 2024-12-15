using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;
using DG.Tweening;
using UnityEngine.InputSystem;
using static ShopControllerState;
using TMPro;

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

            uiControl = controller;
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
            GameObject legacyObj = Instantiate(uiControl.legacyCustomPrefab);
            uiControl.robotControl = legacyObj.GetComponent<CustomRobotController>();

            uiControl.robotControl.PartsSetReflect();

            uiControl.StateTranstion(CustomizeUIState.Wait);
        }
    }

    class WaitState : CustomizeControlState
    {
        //�R���X�g���N�^�@������
        public WaitState(CustomizeUIController controller)
        {
            state = CustomizeUIState.Wait;

            uiControl = controller;
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

            uiControl = controller;

            arrowDefaultYPosi = uiControl.genreSelectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            //UI�����炩�Ƀt�F�[�h�C������
            Vector2 arrowPosi = uiControl.genreSelectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.genreSelectArrowRect.localPosition = arrowPosi;

            /*

            uiControl.genreSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(1f,0.3f);

            RectTransform[] menuChildRect= uiControl.selectMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-700f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-813f, 0.3f).SetDelay(i * 0.1f); //���ԍ��ō��Ɉړ�����悤��
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f).SetDelay(i * 0.1f); //���ԍ��Ńt�F�[�h
            }*/

            uiControl.transitionManager.onTransitionComplete += InputActionRegister;
        }

        public override void OnExit()
        {
            //UI�����炩�Ƀt�F�[�h�A�E�g����
            /*
            uiControl.genreSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f);

            RectTransform[] menuChildRect = uiControl.selectMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-813f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-900f, 0.3f).SetDelay(i * 0.1f); //���ԍ��ō��Ɉړ�����悤��
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f).SetDelay(i * 0.1f); //���ԍ��Ńt�F�[�h
            }*/

            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed-= CanselAction;
        }

        private void InputActionRegister()
        {
            uiControl.upArrowAct.performed += UpArrowAction;
            uiControl.downArrowAct.performed += DownArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
            uiControl.cancelAct.performed += CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiControl.genreSelectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.genreSelectArrowRect.localPosition = arrowPosi;
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;

            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            Vector2 arrowPosi = uiControl.genreSelectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.genreSelectArrowRect.localPosition = arrowPosi;
        }

        private void ConfirmAction(InputAction.CallbackContext context) //�m��
        {
            //���ݑI�����Ă����ނ̏��i�̃X�N���[���r���[��\������
            MenuState select = (MenuState)nowSelectNum;

            switch (select)
            {
                case MenuState.Body:

                    uiControl.transitionManager.TransitionToRight(DockImageTransition.MenuType.CustomGenre,DockImageTransition.MenuType.CustomPartsType);
                    uiControl.StateTranstion(CustomizeUIState.SelectBodyMenu);

                    break;

                case MenuState.Weapon:

                    uiControl.transitionManager.TransitionToRight(DockImageTransition.MenuType.CustomGenre, DockImageTransition.MenuType.CustomPartsType);
                    uiControl.StateTranstion(CustomizeUIState.SelectWeaponMenu);

                    break;
            }
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.Terminal,DockImageTransition.MenuType.CustomGenre);
            uiControl.StateTranstion(CustomizeUIState.Wait);
            uiControl.mainMenuControl.StateTranstion(MainMenuState.MainMenuStateEnum.SelectMenu);
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

            uiControl = controller;

            arrowDefaultYPosi = uiControl.typeSelectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            //UI�����炩�Ƀt�F�[�h�C������
            Vector2 arrowPosi = uiControl.typeSelectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.typeSelectArrowRect.localPosition = arrowPosi;

            //uiController.typeSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f);

            uiControl.bodyMenuParent.gameObject.SetActive(true);
            uiControl.weaponMenuParent.gameObject.SetActive(false);

            /*
            RectTransform[] menuChildRect = uiControl.bodyMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-700f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-813f, 0.3f).SetDelay(i * 0.1f); //���ԍ��ō��Ɉړ�����悤��
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f).SetDelay(i * 0.1f); //���ԍ��Ńt�F�[�h
            }*/

            uiControl.transitionManager.onTransitionComplete += InputActionRegister;
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
            //UI�����炩�Ƀt�F�[�h�A�E�g����
            //uiController.typeSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f);

            /*
            RectTransform[] menuChildRect = uiControl.bodyMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-813f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-900f, 0.3f).SetDelay(i * 0.1f); //���ԍ��ō��Ɉړ�����悤��
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f).SetDelay(i * 0.1f); //���ԍ��Ńt�F�[�h
            }*/

            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed -= CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            ChangeArrowPosition();
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            ChangeArrowPosition();
        }

        private void ConfirmAction(InputAction.CallbackContext context) //�m��
        {
            //���ݑI�����Ă����ނ̏��i�̃X�N���[���r���[��\������
            BodyPartsData.PartsType select = (BodyPartsData.PartsType)nowSelectNum;

            OpenBodySelectScroll(select);
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.CustomGenre, DockImageTransition.MenuType.CustomPartsType);
            uiControl.StateTranstion(CustomizeUIState.SelectMenu);
        }

        private void ChangeArrowPosition()
        {
            Vector2 arrowPosi = uiControl.typeSelectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.typeSelectArrowRect.localPosition = arrowPosi;
        }

        private void OpenBodySelectScroll(BodyPartsData.PartsType type)
        {
            //�����p�[�c����w�肵����ނ̃p�[�c�𔲂��o������ŃX�N���[���r���[���\������
            SaveData saveData = SaveDataManager.instance.saveData;

            List<HavingItem> ItemList = saveData.PartsDataList.Where(parts => parts.itemData is BodyPartsData bodyPartData && bodyPartData.Type == type).ToList();
            List<BodyPartsData> TypeDataList = ItemList.Select(parts => (BodyPartsData)parts.itemData).ToList();

            uiControl.partsScroll.OpenScrollView();
            uiControl.partsScroll.InitializeUI(TypeDataList, ItemList);

            uiControl.partsScroll.setPartsType = type;


            //�I�𒆂̃��b�V���̃A�E�g���C����\��
            //���ݑ������Ă���I��������ނ̃f�[�^���擾
            uiControl.robotControl.SetAllOutlines(false);

            BodyPartsData partsData = (BodyPartsData)saveData.settingData.PartsNumber[type].itemData;

            foreach (KeyValuePair<string, Mesh> keyValue in partsData.ObjnameMeshPairs)
            {
                uiControl.robotControl.SetMeshOutline(keyValue.Key, true);
            }

            uiControl.statusUI.ChangeCustomize();

            //��ԑJ��
            uiControl.transitionManager.TransitionToRight(DockImageTransition.MenuType.CustomPartsType, DockImageTransition.MenuType.CustomSelectParts);
            uiControl.StateTranstion(CustomizeUIState.OpenedScrollView);
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

            uiControl = controller;

            actionDic = new Dictionary<string, Action>()
            {
            };

            actionDicWithArg = new Dictionary<string, Action<object[]>>()
            {
            };

            arrowDefaultYPosi = uiControl.typeSelectArrowRect.localPosition.y;
        }

        public override void OnEnter()
        {
            //UI�����炩�Ƀt�F�[�h�C������
            Vector2 arrowPosi = uiControl.typeSelectArrowRect.localPosition;
            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.typeSelectArrowRect.localPosition = arrowPosi;

            //uiController.typeSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f);

            uiControl.bodyMenuParent.gameObject.SetActive(false);
            uiControl.weaponMenuParent.gameObject.SetActive(true);

            /*

            RectTransform[] menuChildRect = uiControl.weaponMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-700f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-813f, 0.3f).SetDelay(i * 0.1f); //���ԍ��ō��Ɉړ�����悤��
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(1f, 0.3f).SetDelay(i * 0.1f); //���ԍ��Ńt�F�[�h
            }*/

            uiControl.transitionManager.onTransitionComplete += InputActionRegister;

        }

        public override void OnExit()
        {
            //UI�����炩�Ƀt�F�[�h�A�E�g����
            //uiController.typeSelectArrowRect.GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f);

            /*
            RectTransform[] menuChildRect = uiControl.weaponMenuParent.GetComponentsInChildren<RectTransform>();

            for (int i = 1; i < menuChildRect.Length; i++)
            {
                menuChildRect[i].localPosition = new Vector2(-813f, menuChildRect[i].localPosition.y);
                menuChildRect[i].DOLocalMoveX(-900f, 0.3f).SetDelay(i * 0.1f); //���ԍ��ō��Ɉړ�����悤��
                menuChildRect[i].GetComponent<TextMeshProUGUI>().DOFade(0f, 0.3f).SetDelay(i * 0.1f); //���ԍ��Ńt�F�[�h
            }*/

            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed -= CanselAction;
        }

        private void InputActionRegister()
        {
            uiControl.upArrowAct.performed += UpArrowAction;
            uiControl.downArrowAct.performed += DownArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
            uiControl.cancelAct.performed += CanselAction;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum--;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            ArrowPositionChange();
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            nowSelectNum++;
            nowSelectNum = Mathf.Clamp(nowSelectNum, 0, maxSelectNum);

            ArrowPositionChange();
        }

        private void ArrowPositionChange()
        {
            Vector2 arrowPosi = uiControl.typeSelectArrowRect.localPosition;

            arrowPosi.y = arrowDefaultYPosi - (nowSelectNum * 70);

            uiControl.typeSelectArrowRect.localPosition = arrowPosi;
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
            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.CustomGenre, DockImageTransition.MenuType.CustomPartsType);
            uiControl.StateTranstion(CustomizeUIState.SelectMenu);
        }

        private void OpenWeaponSelectScroll(WeaponPartsData.SetType type,LegacySettingData.WeaponSetPosi setPosi)
        {
            //�����p�[�c����w�肵����ނ̃p�[�c�𔲂��o������ŃX�N���[���r���[���\������
            SaveData saveData = SaveDataManager.instance.saveData;
            List<HavingItem> ItemList = saveData.WeaponsDataList.Where(parts => parts.itemData is WeaponPartsData weaponData && weaponData.setType == type).ToList();
            List<WeaponPartsData> TypeDataList = ItemList.Select(parts => (WeaponPartsData)parts.itemData).ToList();

            uiControl.partsScroll.InitializeUI(TypeDataList, ItemList);
            uiControl.partsScroll.OpenScrollView();

            uiControl.partsScroll.setPosi = setPosi;

            //��ԑJ��
            uiControl.transitionManager.TransitionToRight(DockImageTransition.MenuType.CustomPartsType, DockImageTransition.MenuType.CustomSelectParts);
            uiControl.StateTranstion(CustomizeUIState.OpenedScrollView);
        }
    }

    //�X�N���[���r���[�ł̑I��҂��X�e�[�g
    public class OpenedScrollViewState : CustomizeControlState
    {
        //�R���X�g���N�^�@������
        public OpenedScrollViewState(CustomizeUIController controller)
        {
            state = CustomizeUIState.OpenedScrollView;

            uiControl = controller;
        }

        public override void OnEnter()
        {
            uiControl.transitionManager.onTransitionComplete += InputActionRegister;
        }

        public override void OnExit()
        {
            uiControl.transitionManager.onTransitionComplete -= InputActionRegister;

            uiControl.upArrowAct.performed -= UpArrowAction;
            uiControl.downArrowAct.performed -= DownArrowAction;
            uiControl.confirmAct.performed -= ConfirmAction;
            uiControl.cancelAct.performed -= CanselAction;
        }

        private void InputActionRegister()
        {
            uiControl.upArrowAct.performed += UpArrowAction;
            uiControl.downArrowAct.performed += DownArrowAction;
            uiControl.confirmAct.performed += ConfirmAction;
            uiControl.cancelAct.performed += CanselAction;

            uiControl.openStatusAct.performed += StatusOpenClose;
        }

        public void UpArrowAction(InputAction.CallbackContext context)
        {
            uiControl.partsScroll.ChangeForcus(-1);
        }

        public void DownArrowAction(InputAction.CallbackContext context)
        {
            uiControl.partsScroll.ChangeForcus(1);
        }

        private void ConfirmAction(InputAction.CallbackContext context) //�m��
        {
            uiControl.partsScroll.SelectConfirm();
        }

        //�X�e�[�^�X�\��
        public void StatusOpenClose(InputAction.CallbackContext context)
        {
            uiControl.statusUI.ChangeNextState();
            uiControl.StatusUIDataReflesh();
        }

        public void CanselAction(InputAction.CallbackContext context)
        {
            //�I�[�g�Z�[�u����
            SaveDataManager.instance.SaveFileWriteAsync();

            //�A�E�g���C�����\����
            uiControl.robotControl.SetAllOutlines(false);

            if (uiControl.beforeState.state == CustomizeUIState.SelectBodyMenu)
            {
                uiControl.statusUI.ChangeCustomize();
                uiControl.StatusUIDataReflesh();
            }

            uiControl.transitionManager.TransitionToLeft(DockImageTransition.MenuType.CustomPartsType, DockImageTransition.MenuType.CustomSelectParts);
            uiControl.StateTranstion(uiControl.beforeState.state);
        }
    }

    public CanvasGroup curtainCanvas;

    public CustomRobotController robotControl;

    private List<CustomizeControlState> States = new List<CustomizeControlState>();

    private CustomizeControlState nowState,beforeState;

    public RectTransform genreSelectArrowRect,typeSelectArrowRect;
    public RectTransform selectMenuParent, bodyMenuParent,weaponMenuParent;

    public GameObject legacyCustomPrefab;

    public GameObject ButtonsParent;
    public PartsScrollView partsScroll;

    public LegacyStatusUI statusUI;

    public MainMenuUIController mainMenuControl;

    public DockImageTransition transitionManager;

    [NonSerialized]
    public InputAction upArrowAct, downArrowAct, leftArrowAct, rightArrowAct, confirmAct, cancelAct;

    private InputAction openStatusAct;

    private void Awake()
    {
        //Action�̃Z�b�g�A�b�v
        InputControls testControl = new InputControls();

        upArrowAct = testControl.UI.UpArrow;
        downArrowAct = testControl.UI.DownArrow;
        leftArrowAct = testControl.UI.LeftArrow;
        rightArrowAct = testControl.UI.RightArrow;
        confirmAct = testControl.UI.Confirm;
        cancelAct = testControl.UI.Cancel;
        openStatusAct = testControl.UI.OpenStatus;
    }

    // Start is called before the first frame update
    void Start()
    {
        //openStatusAct.performed += StatusOpenClose;

        States.Add(new SetUpState(this));
        States.Add(new WaitState(this));
        States.Add(new SelectMenuState(this));
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

    //�X�e�[�^�X�\��
    public void StatusOpenClose(InputAction.CallbackContext context)
    {
        statusUI.ChangeNextState();
        StatusUIDataReflesh();
    }

    //�X�e�[�^�XUI�f�[�^�X�V
    public void StatusUIDataReflesh()
    {
        statusUI.NowStatusInitialize();
        statusUI.UIInitialize();
    }

    private void OnDisable()
    {
        upArrowAct.Disable();
        downArrowAct.Disable();
        leftArrowAct.Disable();
        rightArrowAct.Disable();
        confirmAct.Disable();
        cancelAct.Disable();
        openStatusAct.Disable();
    }

    private void OnEnable()
    {
        upArrowAct.Enable();
        downArrowAct.Enable();
        leftArrowAct.Enable();
        rightArrowAct.Enable();
        confirmAct.Enable();
        cancelAct.Enable();
        openStatusAct.Enable();
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
        OpenedScrollView,
    }

    public CustomizeUIState state;

    protected CustomizeUIController uiControl;

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
