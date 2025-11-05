using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ShopGoodsScrollView : MonoBehaviour
{
    public enum ScrollType
    {
        Body,
        Weapon
    }

    public ScrollType scrollType;

    public ShopUIController controller;

    public Camera uiCamera;
    public ScrollRect scrollRect;
    public RectTransform content;

    [SerializeField]
    private GameObject goodsInfoPrefab;

    private int forcusInfoNum; //現在選択中の商品のインデックス

    public ShopGoodsSimpleInfo forcusInfo { get; private set; }

    private List<ShopGoodsSimpleInfo> simpleInfos = new List<ShopGoodsSimpleInfo>();

    public ShopGoodsStatusUI goodsStatus;
    public ShopWeaponStatusUI weaponStatus;
    public ShopBodyStatusUI bodyStatus;

    public void OpenScrollView()
    {
        GetComponent<RectTransform>().DOScaleX(1f, 0.5f);
    }

    public void InitializeScrollView(List<ItemData> displayGoods,ScrollType type)
    {
        scrollType = type;

        switch (scrollType)
        {
            case ScrollType.Weapon:

                weaponStatus.gameObject.SetActive(true);
                bodyStatus.gameObject.SetActive(false);

                break;

            case ScrollType.Body:

                weaponStatus.gameObject.SetActive(false);
                bodyStatus.gameObject.SetActive(true);

                break;
        }

        //子供を全消ししてリセット
        content.DestroyAllChilds();
        simpleInfos.Clear();

        for (int i = 0; i < displayGoods.Count; i++)
        {
            ShopGoodsSimpleInfo info = Instantiate(goodsInfoPrefab, content).GetComponent<ShopGoodsSimpleInfo>();

            info.scrollView = this;
            info.InitalizeUI(displayGoods[i]);

            simpleInfos.Add(info);
        }

        forcusInfo = simpleInfos[0];
        forcusInfoNum = 0;

        forcusInfo.OnForcus();
        InfoOnClick(forcusInfo.goodsData);

        scrollRect.content.anchoredPosition = new Vector2(0, 0);
    }

    public void ChangeForcus(int changeNum)
    {
        forcusInfoNum += changeNum;

        if (forcusInfoNum >= 0 && forcusInfoNum < simpleInfos.Count)
        {
            AudioManager.instance.PlayAudio(AudioData.audioNameEnum.MenuArrowChange, false);
        }
        forcusInfoNum = Mathf.Clamp(forcusInfoNum, 0, simpleInfos.Count - 1);

        ShopGoodsSimpleInfo newForcus = simpleInfos[forcusInfoNum];

        if (newForcus != forcusInfo)
        {
            forcusInfo.OffForcus();
            newForcus.OnForcus();

            forcusInfo = newForcus;

            InfoOnClick(forcusInfo.goodsData);

            EnsureVisible(forcusInfo.GetComponent<RectTransform>());
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

        float itemHeight = target.rect.height;

        // 上方向
        if (itemCorners[1].y > viewportCorners[1].y)
        {
            float deltaY = itemCorners[1].y - viewportCorners[1].y + (itemHeight / 2);

            float targetYPosi = scrollRect.content.anchoredPosition.y - (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi, 0.3f);
        }

        // 下方向
        if (itemCorners[0].y < viewportCorners[0].y)
        {
            float deltaY = viewportCorners[0].y - itemCorners[0].y + (itemHeight / 2);

            float targetYPosi = scrollRect.content.anchoredPosition.y + (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi, 0.3f);
        }
    }

    public void InfoOnClick(ItemData goods)
    {
        switch (scrollType)
        {
            case ScrollType.Body:

                OpenBodyStatus(goods);

                break;

            case ScrollType.Weapon:

                OpenWeaponStatus(goods);
                
                break;
        }
    }

    private void OpenWeaponStatus(ItemData selectGoods)
    {
        goodsStatus.InitializeUI(selectGoods);
        weaponStatus.InitializeUI(selectGoods);
    }

    private void OpenBodyStatus(ItemData selectGoods)
    {
        goodsStatus.InitializeUI(selectGoods);
        bodyStatus.InitializeUI(selectGoods);
    }
}
