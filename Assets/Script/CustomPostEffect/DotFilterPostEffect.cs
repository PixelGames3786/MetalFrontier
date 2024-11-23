using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
[VolumeComponentMenu("DotFilter")]
public class DotFilterPostEffect : VolumeComponent // VolumeComponent���p������
{
    public bool IsActive() => dotFactor.value>0f;

    // Volume�R���|�[�l���g�Őݒ�ł���l�ɂ�XxxParameter�N���X���g��
    public ClampedFloatParameter dotFactor = new ClampedFloatParameter(0, 0, 500);
}
