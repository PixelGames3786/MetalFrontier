using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ShopBodyStatusUI : ShopGoodsStatusUI
{
    private BodyPartsData partsData;

    public List<TextMeshProUGUI> valueTexts;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void InitializeUI(ItemData data)
    {
        partsData = data as BodyPartsData;

        valueTexts[0].text = partsData.hpFactor.ToString();
        valueTexts[1].text = partsData.moveSpeed.ToString();
        valueTexts[2].text = partsData.boostSpeed.ToString();
        valueTexts[3].text = partsData.jumpForce.ToString();
        valueTexts[4].text = partsData.riseForce.ToString();
        valueTexts[5].text = partsData.boostAmout.ToString();
        valueTexts[6].text = partsData.boostUseRate.ToString();
        valueTexts[7].text = partsData.boostRecoverRate.ToString();
        valueTexts[8].text = partsData.physicalRes.ToString();
        valueTexts[9].text = partsData.beamRes.ToString();
        valueTexts[10].text = partsData.shotAccuracy.ToString();
    }
}
