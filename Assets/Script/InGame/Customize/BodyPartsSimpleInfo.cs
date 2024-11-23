using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BodyPartsSimpleInfo : MonoBehaviour
{
    private HavingItem haveItemData;
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

        PartsNameText.text = PartsData.Name;
    }

    public void OnClick()
    {
        if(!haveItemData.equiped) scrollView.BodyPartsSelect(PartsData.Type,PartsData.ItemNumber,haveItemData);
    }
}
