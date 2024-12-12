using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "BodyPartsData", menuName = "PartsData/CreateBodyParts")]
public class BodyPartsData : ItemData
{
    public enum PartsType
    {
        Head,
        Body,
        LeftArm,
        RightArm,
        Leg,
    }

    public PartsType Type;

    public int maxModuleSlot;
    public int minModuleSlot;


    //1�̃p�[�c�ɕ����̃��b�V�����܂܂��ꍇ�����邽��
    public SerializableDictionary<string,Mesh> ObjnameMeshPairs=new SerializableDictionary<string, Mesh>();

    //�p�����[�^
    public float hpFactor;

    public float moveSpeed;
    public float boostSpeed;

    //�W�����v���ڂ̗͂ƌp���I�ɏ㏸����ۂ̗�
    public float jumpForce;
    public float riseForce;

    //�ˌ����x
    public float shotAccuracy;

    //�u�[�X�g�g�p�\��
    public float boostAmout;


    public float boostUseRate; //�u�[�X�g�g�p�{��
    //�u�[�X�g�񕜔{��
    public float boostRecoverRate;

    //�����ϐ��E�r�[���ϐ�
    public float physicalRes;
    public float beamRes;

    //�ő呬�x�@�ʏ�ړ����ƃu�[�X�g��
    public Vector3 maxVel, boostMaxVel;

}
