using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class HackSlashItemSimpleInfo : MonoBehaviour
{
    public ItemData itemData { get; private set; }

    public TextMeshProUGUI itemNameText;

    [SerializeField]
    private Image forcusImage; //フォーカス（現在選択中か）

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnForcus()
    {
        forcusImage.DOFade(1f, 0.3f);
    }

    public void OffForcus()
    {
        forcusImage.DOFade(0f, 0.3f);
    }

    public void InitalizeUI(ItemData itemData)
    {
        this.itemData = itemData;

        itemNameText.text = itemData.itemName;
    }
}
