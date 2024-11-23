using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ShopGoodsStatusUI : MonoBehaviour
{
    private ItemData goodsData;

    public TextMeshProUGUI nameText, descriptText, priceText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenWindow()
    {
        GetComponent<RectTransform>().DOScaleX(1f,0.5f);
    }

    public void CloseWindow()
    {
        GetComponent<RectTransform>().DOScaleX(0f, 0.5f);
    }

    public void InitializeUI(ItemData data)
    {
        goodsData = data;

        nameText.text= goodsData.Name;
        descriptText.text = goodsData.Description;
        priceText.text = goodsData.price+" Col";
    }
}
