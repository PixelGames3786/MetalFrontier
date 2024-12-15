using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    //UI��
    public BarUI hpBar, boostBar,quorraBar;
    public RockOnUI rockOnUI;
    public WeaponUseInfoUI weaponInfoUI;
    public OutOfAreaUI outOfAreaUI;
    public BackToBaseUI backToBaseUI;
    public HackSlashUI hackSlashUI;
    public OverHeatUI overHeatUI, hpCautionUI;

    public Image awakeImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void StartReturnCountDown()
    {
        backToBaseUI.CountDownStart();
    }

    public void ReturnCountDownChange(int value)
    {
        backToBaseUI.CountUpChange(value);
    }

    //���G���A�O�ɏo���ۂ�UI�\��
    public void StartCountDown()
    {
        outOfAreaUI.CountDownStart();
    }

    public void EndCountDown()
    {
        outOfAreaUI.CountDownEnd();
    }

    public void CountDownChange(int value)
    {
        outOfAreaUI.CountUpChange(value);
    }

    public void SetRockOnUI(List<Transform> target)
    {
        rockOnUI.RockOnChange(target);
    }

    public void HPBarChange(float nowHp)
    {
        hpBar.ValueChange(nowHp);
    }

    public void BoostBarChange(float nowBoost)
    {
        boostBar.ValueChange(nowBoost);
    }

    public void InLowFuel() //�I�[�o�[�q�[�g��Ԃɋ߂Â����Ƃ��i�S�̗ʂ�2�������������j
    {
        Debug.Log("���Ă΂�Ă�H");

        overHeatUI.ChangeText("Low Fuel");
        overHeatUI.OpenWindow();
    }

    public void CloseOverHeatUI() //�c�R�������Ȃ���Ԃ���o���Ƃ� �܂��̓I�[�o�[�q�[�g��Ԃ���񕜂����Ƃ�
    {
        overHeatUI.CloseWindow();
    }

    public void StartOverHeat() //�I�[�o�[�q�[�g��Ԃɓ����
    {
        overHeatUI.ChangeText("OverHeat");
        overHeatUI.OpenWindow();
    }


    public void InLowHP()
    {
        hpCautionUI.OpenWindow();
    }

    public void OutLowHP()
    {
        hpCautionUI.CloseWindow();
    }


    //�N�H�[���֌W
    public void QuorraBarChange(float nowQuorra)
    {
        quorraBar.ValueChange(nowQuorra);
    }

    public void ShowAwakeImage() //�o����Ԃɓ������ۂɉ�ʂ̎���𖾂邭����
    {
        awakeImage.DOFade(1f,0.5f);
    }

    public void HideAwakeImage()
    {
        awakeImage.DOFade(0f, 0.5f);
    }

    //�n�N�X���I��
    public void OpenHackSlashUI(List<ItemData> items,HackSlashSource source)
    {
        hackSlashUI.InitializeUI(items,source);
        hackSlashUI.OpenWindow();
    }

    public void CloseHackSlashUI()
    {
        hackSlashUI.CloseWindow();
    }

    //UI�Z�b�g�A�b�v
    public void UIInitlaize(RobotStatusController statusControl)
    {
        hpBar.ValueSetUp(statusControl.nowStatus.maxHP, statusControl.hp);
        boostBar.ValueSetUp(statusControl.nowStatus.boostAmount, statusControl.nowStatus.boostAmount);
        quorraBar.ValueSetUp(statusControl.nowStatus.quorraAmount, statusControl.nowStatus.quorraAmount-statusControl.usedQuorra);

        weaponInfoUI.SetUp(statusControl.GetComponent<RobotSetUpController>());


        //�C�x���g�̃T�u�X�N���C�u�Ȃǂ��s��
        statusControl.onHPChanged += HPBarChange;
        statusControl.onBoostChanged += BoostBarChange;
        statusControl.onQuorraChanged += QuorraBarChange;

        statusControl.robotControl.onAwakeStart += ShowAwakeImage;
        statusControl.robotControl.onAwakeEnd += HideAwakeImage;

        statusControl.onInLowFuel += InLowFuel;
        statusControl.onOutLowFuel += CloseOverHeatUI;
        statusControl.onInOverHeat += StartOverHeat;
        statusControl.onOutOverHeat += CloseOverHeatUI;

        statusControl.onInLowHP += InLowHP;
        statusControl.onOutLowHP += OutLowHP;

        statusControl.robotControl.onDied += OutLowHP;

        statusControl.GetComponent<RobotPlayerInput>().OutOfAreaCountDown += CountDownChange;
    }
}
