using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MissionSimpleInfo : MonoBehaviour
{
    public MissionData missionData { get; private set; }

    [SerializeField]
    private Image forcusImage; //フォーカス（現在選択中かの画像）

    [SerializeField]
    private TextMeshProUGUI nameText;

    public MissionScrollView scrollView;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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

    public void InitializeUI(MissionData data)
    {
        missionData = data;

        nameText.text = missionData.missionName;
    }


    public void OnClick()
    {
        scrollView.InfoOnForcus(missionData);
    }
}
