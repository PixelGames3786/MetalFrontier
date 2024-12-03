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
        //���ɂ��̃A�C�e������������Ă��Ȃ��Ȃ瑕���@��������Ă���Ȃ�O���i�������ʂ̃p�[�c�̑I�𒆂Ɍ���j
        scrollView.WeaponPartsSelect(PartsData.ItemNumber, haveItemData);
    }
}
