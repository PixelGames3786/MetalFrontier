using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShopGoodsSimpleInfo : MonoBehaviour
{
    public ItemData goodsData {  get; private set; }

    [SerializeField]
    private Image forcusImage; //フォーカス（現在選択中か）

    public TextMeshProUGUI goodsNameText,goodsPriceText;

    public ShopGoodsScrollView scrollView;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnForcus()
    {
        forcusImage.DOFade(1f,0.3f);
    }

    public void OffForcus()
    {
        forcusImage.DOFade(0f,0.3f);
    }

    public void InitalizeUI(ItemData itemData)
    {
        goodsData = itemData;

        goodsNameText.text = itemData.Name;
        goodsPriceText.text = itemData.price+" col";
    }

    public void OnClick()
    {
        scrollView.InfoOnClick(goodsData);
    }
}