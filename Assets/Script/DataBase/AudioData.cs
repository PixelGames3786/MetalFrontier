using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "DataBase/CreateAudioData")]
public class AudioData : ScriptableObject
{
    public enum audioNameEnum
    {
        TitleConfirm, MenuArrowChange, MenuConfirm, MenuCancel, Purchase, PartsChange,DockBGM,AwakeStart,BoostStart,
        Boosting,
    }

    public audioNameEnum audioName;
    public float audioVolume;
    public AudioClip audioClip;

    public bool is3DAudio; //3DAudio‚©‚Ç‚¤‚©
}