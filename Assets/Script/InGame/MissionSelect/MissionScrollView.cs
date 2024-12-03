using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem.XR;

public class MissionScrollView : MonoBehaviour
{
    public Camera uiCamera;
    public ScrollRect scrollRect;

    private int forcusInfoNum; //現在選択中のミッションの番号

    [SerializeField]
    private RectTransform content;

    [SerializeField]
    private GameObject missionInfoPrefab;

    public MissionSimpleInfo forcusInfo { get; private set; }

    private List<MissionSimpleInfo> SimpleInfoList = new List<MissionSimpleInfo>();

    [SerializeField]
    private TitleController titleControl;

    [SerializeField]
    private MissionSelectUIController controller;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenScrollView()
    {
        GetComponent<RectTransform>().DOScaleX(1f, 1f);
    }

    public void CloseScrollView()
    {
        GetComponent<RectTransform>().DOScaleX(0f, 1f);
    }

    public void InitializeScrollView(List<MissionData> displayMissions)
    {
        //子供を全消ししてリセット
        content.DestroyAllChilds();
        SimpleInfoList.Clear();

        for (int i = 0; i < displayMissions.Count; i++)
        {
            MissionSimpleInfo info = Instantiate(missionInfoPrefab, content).GetComponent<MissionSimpleInfo>();

            info.scrollView = this;
            info.InitializeUI(displayMissions[i]);

            SimpleInfoList.Add(info);
        }

        forcusInfo = SimpleInfoList[0];
        forcusInfoNum = 0;

        forcusInfo.OnForcus();
        InfoOnForcus(forcusInfo.missionData);
    }

    public void ChangeForcus(int changeNum)
    {
        forcusInfoNum += changeNum;

        forcusInfoNum = Mathf.Clamp(forcusInfoNum, 0, SimpleInfoList.Count - 1);

        MissionSimpleInfo newForcus = SimpleInfoList[forcusInfoNum];

        if (newForcus != forcusInfo)
        {
            forcusInfo.OffForcus();
            newForcus.OnForcus();

            forcusInfo = newForcus;

            InfoOnForcus(forcusInfo.missionData);

            EnsureVisible(forcusInfo.GetComponent<RectTransform>());
        }
    }

    private void EnsureVisible(RectTransform target)
    {
        RectTransform viewport = scrollRect.viewport;

        //Content座標をViewportのローカル座標系に変換
        Vector3[] itemCorners = new Vector3[4];
        Vector3[] viewportCorners = new Vector3[4];

        target.GetWorldCorners(itemCorners);
        viewport.GetWorldCorners(viewportCorners);

        //ワールド座標をスクリーン座標に変換
        for (int i = 0; i < 4; i++)
        {
            itemCorners[i] = uiCamera.WorldToScreenPoint(itemCorners[i]);
            viewportCorners[i] = uiCamera.WorldToScreenPoint(viewportCorners[i]);
        }

        // 上方向（Viewportの上端に対してアイテムが上に出ている場合）
        if (itemCorners[1].y > viewportCorners[1].y)
        {
            float deltaY = itemCorners[1].y - viewportCorners[1].y;

            float targetYPosi = scrollRect.content.anchoredPosition.y - (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi, 0.3f);
        }

        // 下方向（Viewportの下端に対してアイテムが下に出ている場合）
        if (itemCorners[0].y < viewportCorners[0].y)
        {
            float deltaY = (itemCorners[0].y - viewportCorners[0].y) * -1;

            float targetYPosi = scrollRect.content.anchoredPosition.y + (deltaY / uiCamera.pixelHeight * content.rect.height);
            scrollRect.content.DOAnchorPosY(targetYPosi, 0.3f);
        }
    }

    public void InfoOnForcus(MissionData data)
    {
        object[] objects = new object[1] { data };

        controller.CallStateFuncArg("InfoOnForcus", objects);

        //controller.CallStateFuncArg("SelectMission",new object[1] {data.sceneName });
    }
}
