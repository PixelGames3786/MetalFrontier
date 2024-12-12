using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using static UnityEditor.Progress;
using static BodyPartsData;
using UnityEditor;
using System.Linq;

public class PartsScrollView : MonoBehaviour
{
    public Camera uiCamera;

    public ScrollRect scrollRect;
    public RectTransform content;

    [SerializeField]
    private GameObject PartsInfoPrefab, weaponInfoPrefab;

    private int forcusInfoNum; //���ݑI�𒆂̏��i�̔ԍ�

    private int scrollType; //0:Body 1:Weapon

    private SimpleInfoBase forcusInfo;
    private List<SimpleInfoBase> simpleInfos = new List<SimpleInfoBase>();

    //���ݕύX���Ă��镐��̕���
    public LegacySettingData.WeaponSetPosi setPosi;
    public BodyPartsData.PartsType setPartsType;

    [SerializeField]
    private CustomizeUIController uiControl;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeUI(List<BodyPartsData> displayItems, List<HavingItem> havingItems)
    {
        //�q����S�������ă��Z�b�g
        simpleInfos.Clear();
        content.DestroyAllChilds();

        for (int i = 0; i < displayItems.Count; i++)
        {
            BodyPartsSimpleInfo info = Instantiate(PartsInfoPrefab, content).GetComponent<BodyPartsSimpleInfo>();

            info.scrollView = this;
            info.InitalizeUI(displayItems[i], havingItems[i]);

            simpleInfos.Add(info);
        }

        forcusInfo = simpleInfos[0];
        forcusInfoNum = 0;

        scrollType = 0;

        forcusInfo.OnForcus();
        StatusReflesh();

    }

    public void InitializeUI(List<WeaponPartsData> displayWeapons, List<HavingItem> havingItems)
    {
        //�q����S�������ă��Z�b�g
        simpleInfos.Clear();
        content.DestroyAllChilds();

        for (int i = 0; i < displayWeapons.Count; i++)
        {
            WeaponPartsSimpleInfo info = Instantiate(weaponInfoPrefab, content).GetComponent<WeaponPartsSimpleInfo>();

            info.scrollView = this;
            info.InitalizeUI(displayWeapons[i], havingItems[i]);

            simpleInfos.Add(info);
        }

        forcusInfo = simpleInfos[0];
        forcusInfoNum = 0;

        scrollType = 1;

        forcusInfo.OnForcus();
        StatusReflesh();
    }

    public void OpenScrollView()
    {
        GetComponent<RectTransform>().DOScaleX(1f, 0.5f);
    }
    public void CloseScrollView()
    {
        GetComponent<RectTransform>().DOScaleX(0f, 0.5f);
    }

    public void ChangeForcus(int changeNum)
    {
        forcusInfoNum += changeNum;

        forcusInfoNum = Mathf.Clamp(forcusInfoNum, 0, simpleInfos.Count - 1);

        SimpleInfoBase newForcus = simpleInfos[forcusInfoNum];

        if (newForcus != forcusInfo)
        {
            forcusInfo.OffForcus();
            newForcus.OnForcus();

            forcusInfo = newForcus;

            EnsureVisible(forcusInfo.GetComponent<RectTransform>());

            StatusReflesh();
        }
    }

    public void StatusReflesh()
    {
        if (scrollType == 0)
        {
            Dictionary<PartsType, HavingItem> partsNumber = SaveDataManager.instance.saveData.settingData.PartsNumber;

            var deepCopyDict = new Dictionary<PartsType, HavingItem>(
                partsNumber.ToDictionary(entry => entry.Key, entry => entry.Value)
            );

            BodyPartsSimpleInfo simpleInfo = forcusInfo as BodyPartsSimpleInfo;
            deepCopyDict[setPartsType] = simpleInfo.haveItemData;

            uiControl.statusUI.NowStatusInitialize();
            uiControl.statusUI.NewStatusInitialize(deepCopyDict);
            uiControl.statusUI.UIInitialize();
        }
    }

    //�㉺�Ƀt�H�[�J�X���ړ������ۂɁA�X�N���[���r���[���X�N���[������
    private void EnsureVisible(RectTransform target)
    {
        RectTransform viewport = scrollRect.viewport;

        //Content���W��Viewport�̃��[�J�����W�n�ɕϊ�
        Vector3[] itemCorners = new Vector3[4];
        Vector3[] viewportCorners = new Vector3[4];

        target.GetWorldCorners(itemCorners);
        viewport.GetWorldCorners(viewportCorners);

        //���[���h���W���X�N���[�����W�ɕϊ�
        for (int i = 0; i < 4; i++)
        {
            itemCorners[i] = uiCamera.WorldToScreenPoint(itemCorners[i]);
            viewportCorners[i] = uiCamera.WorldToScreenPoint(viewportCorners[i]);
        }

        // ������iViewport�̏�[�ɑ΂��ăA�C�e������ɏo�Ă���ꍇ�j
        if (itemCorners[1].y > viewportCorners[1].y)
        {
            float deltaY = itemCorners[1].y - viewportCorners[1].y;

            float targetYPosi = scrollRect.content.anchoredPosition.y - (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi, 0.3f);
        }

        // �������iViewport�̉��[�ɑ΂��ăA�C�e�������ɏo�Ă���ꍇ�j
        if (itemCorners[0].y < viewportCorners[0].y)
        {
            float deltaY = (itemCorners[0].y - viewportCorners[0].y) * -1;

            float targetYPosi = scrollRect.content.anchoredPosition.y + (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi, 0.3f);
        }
    }

    public void SelectConfirm()
    {
        forcusInfo.OnSelect();
    }

    public void BodyPartsSelect(BodyPartsData.PartsType type, int partsNum, HavingItem having)
    {
        uiControl.LegacyPartsChange(type, partsNum, having);
    }

    public void WeaponPartsSelect(int partsNum, HavingItem having)
    {
        //�������łȂ��A�C�e���Ȃ畁�ʂɑ���
        if (!having.equiped)
        {
            print("�������łȂ����߃A�C�e��������");

            uiControl.WeaponPartsChange(setPosi, partsNum, having);
        }
        else
        {
            print("�������ł��邽�߃A�C�e�����O��");

            uiControl.WeaponPartsRemove(setPosi, partsNum, having);
        }
    }

}
