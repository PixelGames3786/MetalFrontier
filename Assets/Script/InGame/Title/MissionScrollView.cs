using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionScrollView : MonoBehaviour
{
    [SerializeField]
    private Transform content;

    [SerializeField]
    private GameObject missionInfoPrefab;

    [SerializeField]
    private TitleController titleControl;

    [SerializeField]
    private CustomizeUIController uiControl;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeUI(List<MissionData> displayMissions)
    {
        //子供を全消ししてリセット
        content.DestroyAllChilds();

        for (int i = 0; i < displayMissions.Count; i++)
        {
            MissionSimpleInfo info = Instantiate(missionInfoPrefab, content).GetComponent<MissionSimpleInfo>();

            info.scrollView = this;
            info.InitializeUI(displayMissions[i]);
        }
    }

    public void SelectMission(MissionData data)
    {
        uiControl.CallStateFuncArg("SelectMission",new object[1] {data.sceneName });

        //SceneChangeManager.instance.StartCoroutine("SceneTransition", data.sceneName);

        //titleControl.StartSceneLoad(data.sceneName);
    }
}
