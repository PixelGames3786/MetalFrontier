using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class PartsScrollView : MonoBehaviour
{
    public CustomizeUIController controller;

    public RectTransform content;

    [SerializeField]
    private GameObject PartsInfoPrefab,weaponInfoPrefab;

    private List<BodyPartsSimpleInfo> SimpleInfoList = new List<BodyPartsSimpleInfo>();

    //現在変更している武器の部位
    public LegacySettingData.WeaponSetPosi setPosi;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeUI(List<BodyPartsData> displayItems,List<HavingItem> havingItems)
    {
        //子供を全消ししてリセット
        content.DestroyAllChilds();

        for (int i = 0; i < displayItems.Count; i++)
        {
            BodyPartsSimpleInfo info = Instantiate(PartsInfoPrefab, content).GetComponent<BodyPartsSimpleInfo>();

            info.scrollView = this;
            info.InitalizeUI(displayItems[i], havingItems[i]);
        }
    }

    public void InitializeUI(List<WeaponPartsData> displayWeapons, List<HavingItem> havingItems)
    {
        //子供を全消ししてリセット
        content.DestroyAllChilds();

        for (int i=0;i<displayWeapons.Count;i++)
        {
            WeaponPartsSimpleInfo info = Instantiate(weaponInfoPrefab, content).GetComponent<WeaponPartsSimpleInfo>();

            info.scrollView = this;
            info.InitalizeUI(displayWeapons[i], havingItems[i]);
        }
    }

    public void CloseUI()
    {
        controller.CallStateFunc("CloseScrollView");
    }

    public void BodyPartsSelect(BodyPartsData.PartsType type,int partsNum,HavingItem having)
    {
        controller.LegacyPartsChange(type,partsNum,having);
    }

    public void WeaponPartsSelect(int partsNum,HavingItem having)
    {
        //装備中でないアイテムなら普通に装備
        if (!having.equiped)
        {
            print("装備中でないためアイテムを交換");

            controller.WeaponPartsChange(setPosi, partsNum, having);
        }
        else
        {
            print("装備中であるためアイテムを外す");

            controller.WeaponPartsRemove(setPosi, partsNum, having);
        }
    }

}
