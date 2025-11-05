using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BarUI : MonoBehaviour
{
    public RectTransform barRect;

    public float maxValue,nowValue;

    [SerializeField]
    private TextMeshProUGUI valueText;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    public void ValueSetUp(float maxVal,float nowVal)
    {
        maxValue = maxVal;
        nowValue = nowVal;

        //比率を計算してバーに反映する
        float ratio = nowValue / maxValue;

        barRect.localScale=new Vector3(ratio, barRect.localScale.y, barRect.localScale.z);

        //テキスト反映
        if(valueText) valueText.text = $"{(int)nowValue} / {(int)maxValue}";
    }

    public void ValueChange(float val)
    {
        nowValue = val;

        //比率を計算してバーに反映する
        float ratio = nowValue / maxValue;

        barRect.localScale= new Vector3(ratio,barRect.localScale.y,barRect.localScale.z);

        //テキスト反映
        if (valueText) valueText.text = $"{(int)nowValue} / {(int)maxValue}";

    }
}
