using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    public static SceneChangeManager instance;

    public float loadProgress;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator SceneTransition(string sceneName)
    {

        UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.89f)
        {
            loadProgress = asyncLoad.progress;

            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }

    public IEnumerator SceneTransition(int sceneNum)
    {
        UnityEngine.AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNum);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.89f)
        {
            loadProgress = asyncLoad.progress;

            yield return null;
        }

        asyncLoad.allowSceneActivation = true;
    }
}
