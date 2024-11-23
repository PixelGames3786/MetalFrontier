using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PurchaseErrorUI : MonoBehaviour
{
    public ShopUIController UIcontroller;

    public TextMeshProUGUI checkText;

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
        GetComponent<RectTransform>().DOScaleX(1f,0.3f);
    }

    public void CloseWindow()
    {
        GetComponent<RectTransform>().DOScaleX(0f, 0.3f);
    }

    public void InitializeUI(ItemData goods)
    {
        //金額が足りてるかどうかをチェック
        //checkText.text= $"{goods.name}({goods.price})を購入しますか？";
    }
}
