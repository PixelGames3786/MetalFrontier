using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using DG.Tweening;

public class HackSlashUI : MonoBehaviour
{
    private InputAction upArrowAct, downArrowAct, confirmAct;

    private List<HackSlashItemSimpleInfo> simpleInfoList = new List<HackSlashItemSimpleInfo>();
    private HackSlashItemSimpleInfo forcusInfo;

    [SerializeField]
    private HackSlashItemSimpleInfo simpleInfoPrefab;

    [SerializeField]
    private RectTransform thisRect,infoParent;

    private HackSlashSource hackSource;

    private int forcusNum = 0;

    private int forcusMaxNum = 0;

    // Start is called before the first frame update
    void Start()
    {
        InputControls inputActions = new InputControls();

        upArrowAct = inputActions.UI.UpArrow;
        downArrowAct = inputActions.UI.DownArrow;
        confirmAct = inputActions.UI.Confirm;

        upArrowAct.performed += UpArrowAction;
        downArrowAct.performed+= DownArrowAction;
        confirmAct.performed += ConfirmAction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenWindow()
    {
        thisRect.DOScaleX(1f, 0.5f).OnComplete(() => 
        {
            upArrowAct.Enable();
            downArrowAct.Enable();
            confirmAct.Enable();
        });
    }

    public void CloseWindow()
    {
        upArrowAct.Disable();
        downArrowAct.Disable();
        confirmAct.Disable();

        thisRect.DOScaleX(0f, 0.5f);
    }

    public void InitializeUI(List<ItemData> displayItems,HackSlashSource source)
    {
        hackSource = source;

        //子供を全消ししてリセット
        infoParent.DestroyAllChilds();
        simpleInfoList.Clear();

        foreach (ItemData item in displayItems)
        {
            HackSlashItemSimpleInfo info = Instantiate(simpleInfoPrefab,infoParent);

            info.InitalizeUI(item);

            simpleInfoList.Add(info);
        }

        forcusInfo = simpleInfoList[0];
        forcusNum = 0;

        forcusInfo.OnForcus();
    }

    private void UpArrowAction(InputAction.CallbackContext context)
    {
        ChangeForcus(-1);
    }

    private void DownArrowAction(InputAction.CallbackContext context)
    {
        ChangeForcus(1);
    }

    private void ConfirmAction(InputAction.CallbackContext context)
    {
        //アイテム入手処理
        SaveDataManager.instance.saveData.AddItem(forcusInfo.itemData.ItemNumber);
        hackSource.HackGet();

        CloseWindow();
    }

    private void ChangeForcus(int changeNum)
    {
        forcusNum += changeNum;

        forcusNum = Mathf.Clamp(forcusNum, 0, simpleInfoList.Count - 1);

        HackSlashItemSimpleInfo newForcus = simpleInfoList[forcusNum];

        if (newForcus != forcusInfo)
        {
            forcusInfo.OffForcus();
            newForcus.OnForcus();

            forcusInfo = newForcus;
        }
    }
}
