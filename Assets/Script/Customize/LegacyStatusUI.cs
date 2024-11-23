using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LegacyStatusUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI maxHpText, moveSpeedText, boostSpeedText, jumpForceText, riseForceText;

    [SerializeField]
    private TextMeshProUGUI boostAmountText, useRateText, recoverRateText;

    [SerializeField]
    private TextMeshProUGUI physicResText, beamResText, shotAccuracyText;

    private LegacySpecStatus status;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UIInitialize()
    {
        maxHpText.text = status.maxHP.ToString();
        moveSpeedText.text=status.moveSpeed.ToString();
        boostSpeedText.text=status.boostSpeed.ToString();
        jumpForceText.text=status.jumpForce.ToString();
        riseForceText.text=status.riseForce.ToString();
        boostAmountText.text=status.boostAmount.ToString();
        useRateText.text=status.boostUseRate.ToString();
        recoverRateText.text=status.boostRecoverRate.ToString();
        physicResText.text=status.physicalRes.ToString();
        beamResText.text=status.beamRes.ToString();
        shotAccuracyText.text=status.shotAccuracy.ToString();
    }

    //装備パーツからステータスを算出
    public void StatusInitialize(List<BodyPartsData> bodyParts)
    {
        status = new LegacySpecStatus();

        foreach (BodyPartsData data in bodyParts)
        {
            status.maxHP += data.hpFactor;

            status.moveSpeed += data.moveSpeed;
            status.boostSpeed += data.boostSpeed;

            status.jumpForce += data.jumpForce;
            status.riseForce += data.riseForce;

            status.boostAmount += data.boostAmout;
            status.boostUseRate += data.boostUseRate;
            status.boostRecoverRate += data.boostRecoverRate;

            status.maxVel += data.maxVel;
            status.boostMaxVel += data.boostMaxVel;

            status.physicalRes += data.physicalRes;
            status.beamRes += data.beamRes;

            status.shotAccuracy += data.shotAccuracy;
        }
    }
}
