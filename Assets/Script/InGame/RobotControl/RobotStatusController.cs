using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class RobotStatusController : MonoBehaviour
{
    public RobotController robotControl { get; private set; }

    private LegacySpecStatus normalStatus = new LegacySpecStatus();
    private LegacySpecStatus awakeStatus = new LegacySpecStatus();

    public LegacySpecStatus nowStatus=new LegacySpecStatus();

    //�f���Q�[�g�ނ̒�`
    public event Action<float> OnHPChanged,OnBoostChanged,OnQuorraChanged;

    //�ϓ�����X�e�[�^�X�ށ@��HP�Ƃ��@�u�[�X�g�c�ʂȂ�
    public float hp;
    public float usedBoost; //�g�p�ς݃u�[�X�g��
    public float usedQuorra; //�g�p�ς݊o����

    private bool isOverHeat;

    public bool isRockOn { get; private set; }

    private Transform rockOnTarget;

    //�o�����ɂǂꂭ�炢�\�͂��ϓ����邩 �Ƃ肠����1.5�{
    private float awakeFactor = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        robotControl = GetComponent<RobotController>();
    }

    //�g�p�ς݃u�[�X�g�ʂ𑝂₷
    public void BoostUse(float useVal)
    {
        usedBoost+=useVal;

        //�Ō�܂Ŏg�p����ƃI�[�o�[�q�[�g�@�����u�[�X�g�E�㏸��~
        if (usedBoost>=nowStatus.boostAmount)
        {
            isOverHeat = true;

            robotControl.canBoost = false;
            robotControl.EndBoost();
            robotControl.EndRise();
        }

        OnBoostChanged?.Invoke(nowStatus.boostAmount-usedBoost);
    }

    //�g�p�ς݃u�[�X�g�ʂ����炷
    public void BoostRecover(float recoverVal)
    {
        usedBoost-=recoverVal;

        if (usedBoost<=0)
        {
            usedBoost=0;

            //�I�[�o�[�q�[�g��Ԃ����
            if (isOverHeat)
            {
                isOverHeat=false;
                robotControl.canBoost = true;
            }
        }

        OnBoostChanged?.Invoke(nowStatus.boostAmount - usedBoost);
    }

    //�g�p�ς݊o���ʂ𑝂₷
    public void QuorraUse(float useVal)
    {
        usedQuorra += useVal;

        //�Ō�܂Ŏg�p����Ɗo������
        if (usedBoost >= nowStatus.quorraAmount)
        {
            robotControl.AwakeEnd();
        }

        OnQuorraChanged?.Invoke(nowStatus.quorraAmount - usedQuorra);
    }

    //�g�p�ς݊o���ʂ����炷
    public void QuorraRecover(float recoverVal)
    {
        usedQuorra -= recoverVal;

        if (usedQuorra <= 0)
        {
            usedQuorra = 0;
        }

        OnQuorraChanged?.Invoke(nowStatus.quorraAmount - usedQuorra);
    }

    //HP��ϓ�������
    public void HPChange(float changeVal)
    {
        hp += changeVal;

        hp = Mathf.Clamp(hp,0,nowStatus.maxHP);

        OnHPChanged?.Invoke(hp);
    }

    //�o����Ԃɓ���@�X�e�[�^�X��؂�ւ���
    public void StartAwake()
    {
        nowStatus = awakeStatus;
    }

    public void EndAwake()
    {
        nowStatus = normalStatus;
    }

    //�^�[�Q�b�g�ݒ�
    public void SetTarget(Transform target)
    {
        rockOnTarget = target;

        if (rockOnTarget)
        {
            isRockOn = true;
        }
        else
        {
            isRockOn = false;
        }

    }


    //�����p�[�c����X�e�[�^�X���Z�o
    public void StatusInitalize(List<BodyPartsData> bodyParts)
    {
        //�ʏ펞�̃X�e�[�^�X���Z�o
        foreach (BodyPartsData data in bodyParts)
        {
            normalStatus.maxHP += data.hpFactor;

            normalStatus.moveSpeed += data.moveSpeed;
            normalStatus.boostSpeed += data.boostSpeed;

            normalStatus.jumpForce += data.jumpForce;
            normalStatus.riseForce += data.riseForce;

            normalStatus.boostAmount += data.boostAmout;
            normalStatus.boostUseRate+=data.boostUseRate;
            normalStatus.boostRecoverRate += data.boostRecoverRate;

            normalStatus.maxVel += data.maxVel;
            normalStatus.boostMaxVel += data.boostMaxVel;

            normalStatus.physicalRes += data.physicalRes;
            normalStatus.beamRes += data.beamRes;

            normalStatus.shotAccuracy+= data.shotAccuracy;
        }

        //�o�����̃X�e�[�^�X��ʏ펞�X�e�[�^�X����Z�o
        //JSON���g���Ēl���R�s�[�@�Q�ƌ^�Ȃ̂�=���ƎQ�Ɠn���ɂȂ��Ă��܂�����
        string json = JsonConvert.SerializeObject(normalStatus);
        awakeStatus = JsonConvert.DeserializeObject<LegacySpecStatus>(json);

        awakeStatus.moveSpeed=normalStatus.moveSpeed*awakeFactor;
        awakeStatus.boostSpeed=normalStatus.boostSpeed*awakeFactor;
        awakeStatus.jumpForce=normalStatus.jumpForce*awakeFactor;
        awakeStatus.riseForce=normalStatus.riseForce*awakeFactor;
        awakeStatus.maxVel=normalStatus.maxVel*awakeFactor;
        awakeStatus.boostMaxVel=normalStatus.boostMaxVel*awakeFactor;

        nowStatus = normalStatus;

        //�X�e�[�^�X�v�Z��
        hp = normalStatus.maxHP;
        usedQuorra = normalStatus.quorraAmount;
    }
}


