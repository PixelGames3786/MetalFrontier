using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarUI : MonoBehaviour
{
    public RectTransform barRect;

    public float maxValue,nowValue;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ValueSetUp(float maxVal,float nowVal)
    {
        maxValue = maxVal;
        nowValue = nowVal;

        //�䗦���v�Z���ăo�[�ɔ��f����
        float ratio = nowValue / maxValue;

        barRect.localScale=new Vector3(ratio, barRect.localScale.y, barRect.localScale.z);
    }

    public void ValueChange(float val)
    {
        nowValue = val;

        //�䗦���v�Z���ăo�[�ɔ��f����
        float ratio = nowValue / maxValue;

        barRect.localScale= new Vector3(ratio,barRect.localScale.y,barRect.localScale.z);
    }
}
