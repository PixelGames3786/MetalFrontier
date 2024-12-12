using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class OverHeatUI : MonoBehaviour
{
    public TextMeshProUGUI cautionText;

    public RectTransform thisRect;

    // Start is called before the first frame update
    void Start()
    {
        thisRect=GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenWindow()
    {
        thisRect.DOScaleX(1f,0.2f);
    }

    public void CloseWindow()
    {
        thisRect.DOScaleX(0f,0.2f);
    }

    public void ChangeText(string displayStr)
    {
        cautionText.text = displayStr;
    }
}
