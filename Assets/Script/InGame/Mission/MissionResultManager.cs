using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class MissionResultManager : MonoBehaviour
{
    public MissionManager missionManager;

    public CanvasGroup resultCanvas;

    public GameObject itemInfoPrefab;

    public TextMeshProUGUI missionNameText,clearOrFailText,getColText;

    public GameObject getColTitle, getPartsTitle, itemScrollView;

    public RectTransform content;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResultSetUp()
    {
        MissionCondition condition=FindObjectOfType<MissionCondition>();

        missionNameText.text = condition.missionData.missionName;

        if (condition.isMissionClear)
        {
            clearOrFailText.text = "Clear";

            getColText.text = "+" + condition.missionData.clearGetCol.ToString() + "col";

            foreach (ItemData data in condition.missionData.clearGetItems)
            {
                GameObject infoObj = Instantiate(itemInfoPrefab, content);

                infoObj.GetComponent<ItemSimpleInfo>().InitalizeUI(data, null);
            }
        }
        else
        {
            clearOrFailText.text = "Fail";

            getColText.gameObject.SetActive(false);
            getColTitle.gameObject.SetActive(false);

            getPartsTitle.gameObject.SetActive(false);
            itemScrollView.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);

        resultCanvas.DOFade(1f,1f);
    }

    public void BackToDock()
    {
        missionManager.CurtainTransition("DockScene");
    }
}
