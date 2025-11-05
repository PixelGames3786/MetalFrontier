using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using DG.Tweening;
using Unity.VisualScripting.FullSerializer;

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
    public List<AudioSource> willStopSources; //DoTweenでフェードアウト中のAudioSourceリスト
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

    public void PlayAudio(AudioData.audioNameEnum name,bool isLoop,Vector3 audioPosi=default,Transform parent=null)
    {
        AudioData useData = audioDataList.Find((data)=>data.audioName==name);
        //ヌルチェ
        if (useData == null)
        {
            Debug.Log(name+"のAudioDataが見つからなかったよ");
            return;
        }

        //使用されていないAudioSourceを検索
        AudioSource useSource = GetUnUseSource();

        useSource.clip = useData.audioClip;
        useSource.loop = isLoop;
        useSource.volume = 1;

        //3DAudio設定
        if (useData.is3DAudio)
        {
            useSource.spatialBlend = 1f;
            useSource.transform.position=audioPosi;
        }
        else
        {
            useSource.spatialBlend = 0f;
        }

        //親オブジェクト設定
        if (parent)
        {
            useSource.transform.parent = parent;
            useSource.transform.localPosition = Vector3.zero;
        }
        else
        {
            useSource.transform.parent = transform;
        }

        useSource.Play();
    }

    public void PlayAudio(AudioData data, bool isLoop, Vector3 audioPosi = default, Transform parent = null)
    {
        if (!data) return;

        //使用されていないAudioSourceを検索
        AudioSource useSource = GetUnUseSource();

        useSource.clip = data.audioClip;
        useSource.loop = isLoop;
        useSource.volume = data.audioVolume;

        //3DAudio設定
        if (data.is3DAudio)
        {
            useSource.spatialBlend = 1f;
            useSource.transform.position = audioPosi;
        }
        else
        {
            useSource.spatialBlend = 0f;
        }

        //親オブジェクト設定
        if (parent)
        {
            useSource.transform.parent = parent;
        }
        else
        {
            useSource.transform.parent = transform;
        }

        useSource.Play();
    }

    public void PlayBGM(AudioData data)
    {
        if (!data) return;

        //AudioSourceを検索
        AudioSource useSource = GetUnUseSource();

        useSource.loop = true;
        useSource.clip = data.audioClip;
        useSource.volume = 0;

        useSource.spatialBlend = 0f;

        useSource.Play();
        useSource.DOFade(data.audioVolume, 0.5f);
    }

    public void PlayBGM(AudioData.audioNameEnum name)
    {
        AudioData useData = audioDataList.First((data) => data.audioName == name);
        //ヌルチェ
        if (useData == null)
        {
            Debug.Log(name + "のAudioDataが見つからなかったよ");
            return;
        }

        //AudioSourceを検索
        AudioSource useSource = GetUnUseSource();

        useSource.loop = true;
        useSource.clip = useData.audioClip;
        useSource.volume = 0;

        useSource.spatialBlend = 0f;

        useSource.Play();
        useSource.DOFade(useData.audioVolume, 0.5f);
    }

    public void StopBGM(AudioData.audioNameEnum name,bool isFade)
    {

        //TODO
        //3DAudioで設定したAudioSourceがシーン遷移時にDestoryされてしまうため応急処置
        allAudioSources = allAudioSources.Where(x => x != null).ToList();

        AudioData useData = audioDataList.Find((data) => data.audioName == name);
        //ヌルチェ
        if (useData == null)
        {
            Debug.Log(name + "のAudioDataが見つからなかったよ");
            return;
        }

        //AudioSourceを検索 今フェードアウト中のAudioSourceを除く
        AudioSource useSource = allAudioSources.Except(willStopSources).ToList().Find((source)=>source.clip==useData.audioClip);

        //ヌルチェ
        if (useSource == null)
        {
            Debug.Log(useData.audioClip.name + "のAudioSourceが見つからなかったよ");
            return;
        }

        useSource.loop = false;

        //止めるときにフェードを行うか
        if (isFade)
        {
            willStopSources.Add(useSource);

            useSource.DOFade(0f, 0.5f).OnComplete(() =>
            {
                willStopSources.Remove(useSource);
                useSource.clip = null;
                useSource.Stop();
            });
        }
        else
        {
            useSource.clip = null;
            useSource.Stop();
        }
    }

    public void StopBGM(AudioData data)
    {
        //AudioSourceを検索
        AudioSource useSource = null;

        foreach (AudioSource source in allAudioSources)
        {
            if (source.clip == data.audioClip && source.isPlaying) useSource = source;
        }

        useSource.loop = false;

        useSource.DOFade(0f, 0.5f).OnComplete(() =>
        {
            useSource.clip = null;
            useSource.Stop();
        });
    }

    private AudioSource GetUnUseSource()
    {
        AudioSource useSource = null;

        //TODO
        //3DAudioで設定したAudioSourceがシーン遷移時にDestoryされてしまうため応急処置
        allAudioSources = allAudioSources.Where(x => x != null).ToList();

        foreach (AudioSource source in allAudioSources)
        {
            //プレイ最中でないAudioSourceを検索
            if (!source.isPlaying) useSource = source;
        }

        //もしも現存するすべてのAudioSourceが使用中ならば新しいAudioSourceを作る
        if (useSource == null)
        {
            useSource = Instantiate(audioPrefab, transform).GetComponent<AudioSource>();
            allAudioSources.Add(useSource);
        }

        return useSource;
    }
}