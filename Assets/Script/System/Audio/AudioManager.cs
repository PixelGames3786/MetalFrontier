using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
            }

            return _instance;
        }
    }

    public static AudioManager _instance;

    public List<AudioData> audioDataList;
    public List<AudioSource> allAudioSources;
    public GameObject audioPrefab;

    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PlayAudio(AudioData.audioName name,bool isLoop)
    {
        AudioData useData = audioDataList.First((data)=>data.name==name);
        //ヌルチェ
        if (useData == null)
        {
            Debug.Log(name+"のAudioDataが見つからなかったよ");
            return;
        }

        //使用されていないAudioSourceを検索
        AudioSource useSource = null;

        foreach (AudioSource source in allAudioSources)
        {
            if(!source.isPlaying) useSource = source;
        }

        //もしも現存するすべてのAudioSourceが使用中ならば新しいAudioSourceを作る
        if (useSource == null)
        {
            useSource = Instantiate(audioPrefab,transform).GetComponent<AudioSource>();
        }

        useSource.clip = useData.audioClip;
        useSource.loop = isLoop;

        useSource.Play();
    }
}

[Serializable]
public class AudioData
{
    public enum audioName
    {
        TitleConfirm,MenuArrowChange,MenuConfirm
    }

    public audioName name;
    public AudioClip audioClip;
}