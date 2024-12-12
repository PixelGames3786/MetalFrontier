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

    private int forcusInfoNum; //現在選択中の商品の番号

    private int scrollType; //0:Body 1:Weapon

    private SimpleInfoBase forcusInfo;
    private List<SimpleInfoBase> simpleInfos = new List<SimpleInfoBase>();

    //現在変更している武器の部位
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
        //子供を全消ししてリセット
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
        //子供を全消ししてリセット
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

    //上下にフォーカスを移動した際に、スクロールビューをスクロールする
    private void EnsureVisible(RectTransform target)
    {
        RectTransform viewport = scrollRect.viewport;

        //Content座標をViewportのローカル座標系に変換
        Vector3[] itemCorners = new Vector3[4];
        Vector3[] viewportCorners = new Vector3[4];

        target.GetWorldCorners(itemCorners);
        viewport.GetWorldCorners(viewportCorners);

        //ワールド座標をスクリーン座標に変換
        for (int i = 0; i < 4; i++)
        {
            itemCorners[i] = uiCamera.WorldToScreenPoint(itemCorners[i]);
            viewportCorners[i] = uiCamera.WorldToScreenPoint(viewportCorners[i]);
        }

        // 上方向（Viewportの上端に対してアイテムが上に出ている場合）
        if (itemCorners[1].y > viewportCorners[1].y)
        {
            float deltaY = itemCorners[1].y - viewportCorners[1].y;

            float targetYPosi = scrollRect.content.anchoredPosition.y - (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi, 0.3f);
        }

        // 下方向（Viewportの下端に対してアイテムが下に出ている場合）
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
        //装備中でないアイテムなら普通に装備
        if (!having.equiped)
        {
            print("装備中でないためアイテムを交換");

            uiControl.WeaponPartsChange(setPosi, partsNum, having);
        }
        else
        {
            print("装備中であるためアイテムを外す");

            uiControl.WeaponPartsRemove(setPosi, partsNum, having);
        }
    }

}
