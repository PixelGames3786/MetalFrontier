using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static BodyPartsData;

public class LegacyStatusUI : MonoBehaviour
{
    public enum StatusUIEnum
    {
        Hide,
        Normal,
    }

    [SerializeField]
    private TextMeshProUGUI maxHpText, moveSpeedText, boostSpeedText, jumpForceText, riseForceText;

    [SerializeField]
    private TextMeshProUGUI boostAmountText, useRateText, recoverRateText;

    [SerializeField]
    private TextMeshProUGUI physicResText, beamResText, shotAccuracyText;

    [SerializeField]
    private List<TextMeshProUGUI> oldStatusTexts,nowStatusTexts;

    private LegacySpecStatus nowStatus,newStatus;

    private bool isCustomize;

    private StatusUIEnum nowState;

    [SerializeField]
    private GameObject oldStatusParent, arrowParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeNextState()
    {
        nowState=nowState.Next();

        switch (nowState)
        {
            case StatusUIEnum.Hide:

                gameObject.SetActive(false);

                break;

            case StatusUIEnum.Normal:

                gameObject.SetActive(true);
                NowStatusInitialize();

                break;
        }
    }

    public void ChangeCustomize()
    {
        isCustomize = !isCustomize;

        if (isCustomize)
        {
            oldStatusParent.SetActive(true);
            arrowParent.SetActive(true);
        }
        else
        {
            oldStatusParent.SetActive(false);
            arrowParent.SetActive(false);
        }
    }

    public void UIInitialize()
    {
        if (isCustomize)
        {
            maxHpText.text = newStatus.maxHP.ToString();
            moveSpeedText.text = newStatus.moveSpeed.ToString();
            boostSpeedText.text = newStatus.boostSpeed.ToString();
            jumpForceText.text = newStatus.jumpForce.ToString();
            riseForceText.text = newStatus.riseForce.ToString();
            boostAmountText.text = newStatus.boostAmount.ToString();
            useRateText.text = newStatus.boostUseRate.ToString();
            recoverRateText.text = newStatus.boostRecoverRate.ToString();
            physicResText.text = newStatus.physicalRes.ToString();
            beamResText.text = newStatus.beamRes.ToString();
            shotAccuracyText.text = newStatus.shotAccuracy.ToString();

            oldStatusTexts[0].text = nowStatus.maxHP.ToString();
            oldStatusTexts[1].text = nowStatus.moveSpeed.ToString();
            oldStatusTexts[2].text = nowStatus.boostSpeed.ToString();
            oldStatusTexts[3].text = nowStatus.jumpForce.ToString();
            oldStatusTexts[4].text = nowStatus.riseForce.ToString();
            oldStatusTexts[5].text = nowStatus.boostAmount.ToString();
            oldStatusTexts[6].text = nowStatus.boostUseRate.ToString();
            oldStatusTexts[7].text = nowStatus.boostRecoverRate.ToString();
            oldStatusTexts[8].text = nowStatus.physicalRes.ToString();
            oldStatusTexts[9].text = nowStatus.beamRes.ToString();
            oldStatusTexts[10].text = nowStatus.shotAccuracy.ToString();
        }
        else 
        {
            maxHpText.text = nowStatus.maxHP.ToString();
            moveSpeedText.text = nowStatus.moveSpeed.ToString();
            boostSpeedText.text = nowStatus.boostSpeed.ToString();
            jumpForceText.text = nowStatus.jumpForce.ToString();
            riseForceText.text = nowStatus.riseForce.ToString();
            boostAmountText.text = nowStatus.boostAmount.ToString();
            useRateText.text = nowStatus.boostUseRate.ToString();
            recoverRateText.text = nowStatus.boostRecoverRate.ToString();
            physicResText.text = nowStatus.physicalRes.ToString();
            beamResText.text = nowStatus.beamRes.ToString();
            shotAccuracyText.text = nowStatus.shotAccuracy.ToString();
        }
    }

    //装備パーツからステータスを算出
    public void NowStatusInitialize()
    {
        //データ取得
        LegacySettingData settingData = SaveDataManager.instance.saveData.settingData;

        //ボディパーツ反映
        List<BodyPartsData> bodyParts = new List<BodyPartsData>();
        Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData> weaponParts = new Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData>();

        //全パーツのデータ取得
        foreach (KeyValuePair<BodyPartsData.PartsType, HavingItem> keyValue in settingData.PartsNumber)
        {
            BodyPartsData Data = (BodyPartsData)keyValue.Value.itemData;
            bodyParts.Add(Data);
        }

        nowStatus = new LegacySpecStatus();

        foreach (BodyPartsData data in bodyParts)
        {
            nowStatus.maxHP += data.hpFactor;

            nowStatus.moveSpeed += data.moveSpeed;
            nowStatus.boostSpeed += data.boostSpeed;

            nowStatus.jumpForce += data.jumpForce;
            nowStatus.riseForce += data.riseForce;

            nowStatus.boostAmount += data.boostAmout;
            nowStatus.boostUseRate += data.boostUseRate;
            nowStatus.boostRecoverRate += data.boostRecoverRate;

            nowStatus.maxVel += data.maxVel;
            nowStatus.boostMaxVel += data.boostMaxVel;

            nowStatus.physicalRes += data.physicalRes;
            nowStatus.beamRes += data.beamRes;

            nowStatus.shotAccuracy += data.shotAccuracy;
        }
    }

    //装備パーツからステータスを算出
    public void NewStatusInitialize(Dictionary<PartsType, HavingItem> partsDic)
    {
        //ボディパーツ反映
        List<BodyPartsData> bodyParts=new List<BodyPartsData>();

        //全パーツのデータ取得
        foreach (KeyValuePair<PartsType, HavingItem> keyValue in partsDic)
        {
            BodyPartsData Data = (BodyPartsData)keyValue.Value.itemData;
            bodyParts.Add(Data);
        }

        newStatus = new LegacySpecStatus();

        foreach (BodyPartsData data in bodyParts)
        {
            newStatus.maxHP += data.hpFactor;

            newStatus.moveSpeed += data.moveSpeed;
            newStatus.boostSpeed += data.boostSpeed;

            newStatus.jumpForce += data.jumpForce;
            newStatus.riseForce += data.riseForce;

            newStatus.boostAmount += data.boostAmout;
            newStatus.boostUseRate += data.boostUseRate;
            newStatus.boostRecoverRate += data.boostRecoverRate;

            newStatus.maxVel += data.maxVel;
            newStatus.boostMaxVel += data.boostMaxVel;

            newStatus.physicalRes += data.physicalRes;
            newStatus.beamRes += data.beamRes;

            newStatus.shotAccuracy += data.shotAccuracy;
        }
    }
}
