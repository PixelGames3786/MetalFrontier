using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BodyPartsSimpleInfo : SimpleInfoBase
{
    public HavingItem haveItemData {  get; private set; }
    private BodyPartsData PartsData;

    public TextMeshProUGUI PartsNameText;

    public PartsScrollView scrollView;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitalizeUI(BodyPartsData itemData,HavingItem having)
    {
        PartsData = itemData;
        haveItemData=having;

        PartsNameText.text = PartsData.itemName;
    }

    public override void OnSelect()
    {
        if (!haveItemData.equiped) scrollView.BodyPartsSelect(PartsData.Type, PartsData.ItemNumber, haveItemData);
    }
}
