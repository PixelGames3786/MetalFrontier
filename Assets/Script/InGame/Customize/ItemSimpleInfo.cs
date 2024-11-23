using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemSimpleInfo : MonoBehaviour
{
    private HavingItem haveItemData;
    private ItemData partsData;

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

    public void InitalizeUI(ItemData itemData,HavingItem having)
    {
        partsData = itemData;
        haveItemData=having;

        PartsNameText.text = partsData.Name;
    }

    public void OnClick()
    {
        //if(!haveItemData.equiped) scrollView.BodyPartsSelect(partsData.Type,partsData.ItemNumber,haveItemData);
    }
}
