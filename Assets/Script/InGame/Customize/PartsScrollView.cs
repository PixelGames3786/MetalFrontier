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

    //���ݕύX���Ă��镐��̕���
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
        //�q����S�������ă��Z�b�g
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
        //�q����S�������ă��Z�b�g
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
        //�������łȂ��A�C�e���Ȃ畁�ʂɑ���
        if (!having.equiped)
        {
            print("�������łȂ����߃A�C�e��������");

            controller.WeaponPartsChange(setPosi, partsNum, having);
        }
        else
        {
            print("�������ł��邽�߃A�C�e�����O��");

            controller.WeaponPartsRemove(setPosi, partsNum, having);
        }
    }

}
