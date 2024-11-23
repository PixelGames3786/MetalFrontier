using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TitleController : MonoBehaviour
{
    [SerializeField]
    private Canvas titleCanvas, missionCanvas,curtainCanvas;

    [SerializeField]
    private MissionScrollView missionScroll;

    private bool sceneLoading;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartButtonClick()
    {
        if (!SaveDataManager.instance.isLoadComplete) return;

        curtainCanvas.gameObject.SetActive(true);

        curtainCanvas.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).OnComplete(() => 
        {
            SceneChangeManager.instance.StartCoroutine("SceneTransition", "DockScene");
        });

        /*

        missionScroll.InitializeUI(SaveDataManager.instance.saveData.missionDataList);
        missionCanvas.gameObject.SetActive(true);

        titleCanvas.GetComponent<CanvasGroup>().DOFade(0f,0.5f);
        missionCanvas.GetComponent<CanvasGroup>().DOFade(1f, 0.5f).OnComplete(() => 
        { 
            titleCanvas.gameObject.SetActive(false); 
        });

        */
    }

    public void StartSceneLoad(string sceneName)
    {
        sceneLoading = true;

        SceneChangeManager.instance.StartCoroutine("SceneTransition",sceneName);

        missionCanvas.GetComponent<CanvasGroup>().DOFade(0f, 0.5f).OnComplete(() => 
        {
            missionCanvas.gameObject.SetActive(false);
        });
    }
}
