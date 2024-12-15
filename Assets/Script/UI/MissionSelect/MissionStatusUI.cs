using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissionStatusUI : MonoBehaviour
{
    private MissionData missionData;

    public TextMeshProUGUI nameText, descriptText, targetText;

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
        GetComponent<RectTransform>().DOScaleX(1f, 0.5f);
    }

    public void CloseWindow()
    {
        GetComponent<RectTransform>().DOScaleX(0f, 0.5f);
    }

    public void InitializeUI(MissionData data)
    {
        missionData = data;

        nameText.text = missionData.missionName;
        descriptText.text = missionData.description;
        targetText.text = missionData.target;
    }
}
