using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionSimpleInfo : MonoBehaviour
{
    private MissionData missionData;

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

    public void InitializeUI(MissionData data)
    {
        missionData = data;

        nameText.text = missionData.missionName;
    }

    public void OnClick()
    {
        scrollView.SelectMission(missionData);
    }
}
