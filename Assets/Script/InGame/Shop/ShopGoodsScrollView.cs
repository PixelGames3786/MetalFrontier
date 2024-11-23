using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShopGoodsScrollView : MonoBehaviour
{
    public ShopUIController controller;

    public RectTransform content;

    [SerializeField]
    private GameObject goodsInfoPrefab;

    private int forcusInfoNum; //現在選択中の商品の番号

    public ShopGoodsSimpleInfo forcusInfo {  get; private set; }

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
        GetComponent<RectTransform>().DOScaleX(1f,1f);
    }

    public void CloseScrollView()
    {
        GetComponent<RectTransform>().DOScaleX(0f, 1f);
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
    }

    public void ChangeForcus(int changeNum)
    {
        forcusInfoNum += changeNum;

        forcusInfoNum = Mathf.Clamp(forcusInfoNum,0,SimpleInfoList.Count-1);

        ShopGoodsSimpleInfo newForcus = SimpleInfoList[forcusInfoNum];

        if (newForcus!=forcusInfo)
        {
            forcusInfo.OffForcus();
            newForcus.OnForcus();

            forcusInfo=newForcus;

            InfoOnClick(forcusInfo.goodsData);
        }
    }

    public void InfoOnClick(ItemData goods)
    {
        object[] objects = new object[1] { goods };

        controller.CallStateFuncArg("InfoOnClick", objects);
    }
}
