using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("DotFilter")]
public class DotFilterPostEffect : VolumeComponent // VolumeComponentを継承する
{
    public bool IsActive() => dotFactor.value>0f;

    // Volumeコンポーネントで設定できる値にはXxxParameterクラスを使う
    public ClampedFloatParameter dotFactor = new ClampedFloatParameter(0, 0, 500);
}
