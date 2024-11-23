using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PurchaseCheckUI : MonoBehaviour
{
    public ShopUIController UIcontroller;

    public TextMeshProUGUI checkText;

    public RectTransform selectArrow;

    private int forcusNum=1; //0 No 1 Yes

    //�w�����邩�ǂ���
    public bool isPurchase
    {
        get { return forcusNum == 1; }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenWindow()
    {
        GetComponent<RectTransform>().DOScaleX(1f,0.3f);
    }

    public void CloseWindow()
    {
        GetComponent<RectTransform>().DOScaleX(0f, 0.3f);
    }

    public void InitializeUI(ItemData goods)
    {
        //���z������Ă邩�ǂ������`�F�b�N
        checkText.text= $"{goods.name}({goods.price}Col)���w�����܂����H";
    }

    public void ChangeForcus(int changeNum)
    {
        forcusNum += changeNum;

        forcusNum = Mathf.Clamp(forcusNum,0,1);

        Vector2 arrowPosi = selectArrow.localPosition;
        arrowPosi.x = -110 + (270 * forcusNum);

        selectArrow.localPosition = arrowPosi;
    }

}
