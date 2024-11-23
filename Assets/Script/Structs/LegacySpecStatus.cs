using System;
using UnityEngine;

[Serializable]
public class LegacySpecStatus //�������ۂ̃X�e�[�^�X�@
{
    public float maxHP;

    public float moveSpeed;
    public float boostSpeed;

    //�W�����v���ڂ̗͂ƌp���I�ɏ㏸����ۂ̗�
    public float jumpForce;
    public float riseForce;

    //�ˌ����x
    public float shotAccuracy;

    //�u�[�X�g�g�p�\��
    public float boostAmount;

    //�u�[�X�g�g�p�{��
    public float boostUseRate;

    //�u�[�X�g�񕜔{��
    public float boostRecoverRate;

    //�����ϐ��E�r�[���ϐ�
    public float physicalRes;
    public float beamRes;

    //�ő呬�x�@�ʏ�ړ����ƃu�[�X�g��
    public Vector3 maxVel, boostMaxVel;

    //�ő�o����
    public float quorraAmount;
}