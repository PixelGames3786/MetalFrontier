using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;

public class MissionResultManager : MonoBehaviour
{
    public MissionManager missionManager;

    public CanvasGroup resultCanvas;

    public GameObject itemInfoPrefab;

    public RectTransform missionParent,getColParent,getItemParent;

    public TextMeshProUGUI missionNameText,clearOrFailText,getColText,backDockText;

    public GameObject getColTitle, getPartsTitle, itemScrollView;

    public RectTransform content;

    private InputAction canselAct;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ResultSetUp()
    {
        InputControls inputControl = new InputControls();

        canselAct = inputControl.UI.Cancel;
        canselAct.performed += BackToDock;

        MissionCondition condition=FindObjectOfType<MissionCondition>();

        missionNameText.text = condition.missionData.missionName;

        //クリアしたかどうかで処理を変える
        if (condition.isMissionClear)
        {
            ClearSetUp(condition);
        }
        else
        {
            FailSetUp();
        }

        gameObject.SetActive(true);

        resultCanvas.DOFade(1f, 1f).OnComplete(() => 
        {
            ResultSlideIn();
        });
    }

    private void ClearSetUp(MissionCondition condition)
    {
        List<HavingItem> getItems = missionManager.getItems;

        clearOrFailText.text = "Clear";

        getColText.text = "+" + condition.missionData.clearGetCol.ToString() + "col";

        foreach (HavingItem data in getItems)
        {
            GameObject infoObj = Instantiate(itemInfoPrefab, content);

            infoObj.GetComponent<ItemSimpleInfo>().InitalizeUI(data.itemData, null);
        }
    }

    private void FailSetUp()
    {
        clearOrFailText.text = "Fail";

        getColText.gameObject.SetActive(false);
        getColTitle.gameObject.SetActive(false);

        getPartsTitle.gameObject.SetActive(false);
        itemScrollView.gameObject.SetActive(false);
    }

    //結果をDotweenでフェードインさせる
    private void ResultSlideIn()
    {
        Sequence slideInSequence=DOTween.Sequence();

        slideInSequence.Append(missionParent.DOLocalMoveX(0f, 0.5f)).Join(missionParent.GetComponent<CanvasGroup>().DOFade(1f,0.5f));
        slideInSequence.AppendInterval(0.5f);
        slideInSequence.Append(getColParent.DOLocalMoveX(0f, 0.5f)).Join(getColParent.GetComponent<CanvasGroup>().DOFade(1f, 0.5f));
        slideInSequence.AppendInterval(0.5f);
        slideInSequence.Append(getItemParent.DOLocalMoveX(0f, 0.5f)).Join(getItemParent.GetComponent<CanvasGroup>().DOFade(1f, 0.5f));
        slideInSequence.AppendInterval(0.5f);
        slideInSequence.Append(backDockText.DOFade(1f,0.5f));

        slideInSequence.OnComplete(() => { canselAct.Enable(); });

        slideInSequence.Play();
    }

    public void BackToDock(InputAction.CallbackContext context)
    {
        missionManager.CurtainTransition("DockScene");
    }

    public void OnDisable()
    {
        canselAct.performed -= BackToDock;

        canselAct.Dispose();
    }
}
