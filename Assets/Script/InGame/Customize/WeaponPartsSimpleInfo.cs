using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WeaponPartsSimpleInfo : SimpleInfoBase
{
    private HavingItem haveItemData;
    private WeaponPartsData PartsData;

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

    public void InitalizeUI(WeaponPartsData itemData, HavingItem having)
    {
        PartsData = itemData;
        haveItemData = having;

        PartsNameText.text = PartsData.itemName;
    }

    public void OnClick()
    {
        //既にそのアイテムが装備されていないなら装備　装備されているなら外す（同じ部位のパーツの選択中に限り）
        scrollView.WeaponPartsSelect(PartsData.ItemNumber, haveItemData);
    }
}
