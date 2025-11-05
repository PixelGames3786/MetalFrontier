using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class ShopWeaponStatusUI : ShopGoodsStatusUI
{
    private WeaponPartsData weaponData;

    public List<TextMeshProUGUI> valueTexts;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void InitializeUI(ItemData data)
    {
        weaponData = data as WeaponPartsData;

        valueTexts[0].text = weaponData.bulletSpeed.ToString();
        valueTexts[1].text = weaponData.damage.ToString();
        valueTexts[2].text = weaponData.useInterval.ToString();
        valueTexts[3].text = weaponData.defaultBullet.ToString();
        valueTexts[4].text = weaponData.attackType.ToString();
        valueTexts[5].text = weaponData.weaponType.ToString();
    }
}
