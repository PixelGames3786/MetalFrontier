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

    //デリゲート類の定義
    public event Action<float> OnHPChanged,OnBoostChanged,OnQuorraChanged;

    //変動するステータス類　実HPとか　ブースト残量など
    public float hp;
    public float usedBoost; //使用済みブースト量
    public float usedQuorra; //使用済み覚醒量

    private bool isOverHeat;

    public bool isRockOn { get; private set; }

    private Transform rockOnTarget;

    //覚醒時にどれくらい能力が変動するか とりあえず1.5倍
    private float awakeFactor = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        robotControl = GetComponent<RobotController>();
    }

    //使用済みブースト量を増やす
    public void BoostUse(float useVal)
    {
        usedBoost+=useVal;

        //最後まで使用するとオーバーヒート　強制ブースト・上昇停止
        if (usedBoost>=nowStatus.boostAmount)
        {
            isOverHeat = true;

            robotControl.canBoost = false;
            robotControl.EndBoost();
            robotControl.EndRise();
        }

        OnBoostChanged?.Invoke(nowStatus.boostAmount-usedBoost);
    }

    //使用済みブースト量を減らす
    public void BoostRecover(float recoverVal)
    {
        usedBoost-=recoverVal;

        if (usedBoost<=0)
        {
            usedBoost=0;

            //オーバーヒート状態から回復
            if (isOverHeat)
            {
                isOverHeat=false;
                robotControl.canBoost = true;
            }
        }

        OnBoostChanged?.Invoke(nowStatus.boostAmount - usedBoost);
    }

    //使用済み覚醒量を増やす
    public void QuorraUse(float useVal)
    {
        usedQuorra += useVal;

        //最後まで使用すると覚醒解除
        if (usedBoost >= nowStatus.quorraAmount)
        {
            robotControl.AwakeEnd();
        }

        OnQuorraChanged?.Invoke(nowStatus.quorraAmount - usedQuorra);
    }

    //使用済み覚醒量を減らす
    public void QuorraRecover(float recoverVal)
    {
        usedQuorra -= recoverVal;

        if (usedQuorra <= 0)
        {
            usedQuorra = 0;
        }

        OnQuorraChanged?.Invoke(nowStatus.quorraAmount - usedQuorra);
    }

    //HPを変動させる
    public void HPChange(float changeVal)
    {
        hp += changeVal;

        hp = Mathf.Clamp(hp,0,nowStatus.maxHP);

        OnHPChanged?.Invoke(hp);
    }

    //覚醒状態に入る　ステータスを切り替える
    public void StartAwake()
    {
        nowStatus = awakeStatus;
    }

    public void EndAwake()
    {
        nowStatus = normalStatus;
    }

    //ターゲット設定
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


    //装備パーツからステータスを算出
    public void StatusInitalize(List<BodyPartsData> bodyParts)
    {
        //通常時のステータスを算出
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

        //覚醒時のステータスを通常時ステータスから算出
        //JSONを使って値をコピー　参照型なので=だと参照渡しになってしまうため
        string json = JsonConvert.SerializeObject(normalStatus);
        awakeStatus = JsonConvert.DeserializeObject<LegacySpecStatus>(json);

        awakeStatus.moveSpeed=normalStatus.moveSpeed*awakeFactor;
        awakeStatus.boostSpeed=normalStatus.boostSpeed*awakeFactor;
        awakeStatus.jumpForce=normalStatus.jumpForce*awakeFactor;
        awakeStatus.riseForce=normalStatus.riseForce*awakeFactor;
        awakeStatus.maxVel=normalStatus.maxVel*awakeFactor;
        awakeStatus.boostMaxVel=normalStatus.boostMaxVel*awakeFactor;

        nowStatus = normalStatus;

        //ステータス計算後
        hp = normalStatus.maxHP;
        usedQuorra = normalStatus.quorraAmount;
    }
}


