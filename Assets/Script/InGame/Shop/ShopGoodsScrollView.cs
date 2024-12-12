using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ShopGoodsScrollView : MonoBehaviour
{
    public ShopUIController controller;

    public Camera uiCamera;
    public ScrollRect scrollRect;
    public RectTransform content;

    [SerializeField]
    private GameObject goodsInfoPrefab;

    private int forcusInfoNum; //現在選択中の商品の番号

    public ShopGoodsSimpleInfo forcusInfo { get; private set; }

    private List<ShopGoodsSimpleInfo> SimpleInfoList = new List<ShopGoodsSimpleInfo>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenScrollView()
    {
        GetComponent<RectTransform>().DOScaleX(1f, 0.5f);
    }

    public void CloseScrollView()
    {
        GetComponent<RectTransform>().DOScaleX(0f, 0.5f);
    }

    public void InitializeScrollView(List<ItemData> displayGoods)
    {
        //子供を全消ししてリセット
        content.DestroyAllChilds();
        SimpleInfoList.Clear();

        for (int i = 0; i < displayGoods.Count; i++)
        {
            ShopGoodsSimpleInfo info = Instantiate(goodsInfoPrefab, content).GetComponent<ShopGoodsSimpleInfo>();

            info.scrollView = this;
            info.InitalizeUI(displayGoods[i]);

            SimpleInfoList.Add(info);
        }

        forcusInfo = SimpleInfoList[0];
        forcusInfoNum = 0;

        forcusInfo.OnForcus();
        InfoOnClick(forcusInfo.goodsData);

    }

    public void ChangeForcus(int changeNum)
    {
        forcusInfoNum += changeNum;

        forcusInfoNum = Mathf.Clamp(forcusInfoNum, 0, SimpleInfoList.Count - 1);

        ShopGoodsSimpleInfo newForcus = SimpleInfoList[forcusInfoNum];

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

        // 上方向（Viewportの上端に対してアイテムが上に出ている場合）
        if (itemCorners[1].y > viewportCorners[1].y)
        {
            float deltaY = itemCorners[1].y - viewportCorners[1].y;

            float targetYPosi= scrollRect.content.anchoredPosition.y - (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi,0.3f);
        }

        // 下方向（Viewportの下端に対してアイテムが下に出ている場合）
        if (itemCorners[0].y < viewportCorners[0].y)
        {
            float deltaY = (itemCorners[0].y - viewportCorners[0].y)*-1;

            float targetYPosi = scrollRect.content.anchoredPosition.y + (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi, 0.3f);
        }
    }

    public void InfoOnClick(ItemData goods)
    {
        object[] objects = new object[1] { goods };

        controller.CallStateFuncArg("InfoOnClick", objects);
    }
}
