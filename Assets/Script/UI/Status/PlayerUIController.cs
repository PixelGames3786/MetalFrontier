using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    //UI類
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

    //作戦エリア外に出た際のUI表示
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

    public void InLowFuel() //オーバーヒート状態に近づいたとき（全体量の2割を下回ったら）
    {
        Debug.Log("いつ呼ばれてる？");

        overHeatUI.ChangeText("Low Fuel");
        overHeatUI.OpenWindow();
    }

    public void CloseOverHeatUI() //残燃料が少ない状態から出たとき またはオーバーヒート状態から回復したとき
    {
        overHeatUI.CloseWindow();
    }

    public void StartOverHeat() //オーバーヒート状態に入ると
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


    //クォーラ関係
    public void QuorraBarChange(float nowQuorra)
    {
        quorraBar.ValueChange(nowQuorra);
    }

    public void ShowAwakeImage() //覚醒状態に入った際に画面の周りを明るくする
    {
        awakeImage.DOFade(1f,0.5f);
    }

    public void HideAwakeImage()
    {
        awakeImage.DOFade(0f, 0.5f);
    }

    //ハクスラ選択
    public void OpenHackSlashUI(List<ItemData> items,HackSlashSource source)
    {
        hackSlashUI.InitializeUI(items,source);
        hackSlashUI.OpenWindow();
    }

    public void CloseHackSlashUI()
    {
        hackSlashUI.CloseWindow();
    }

    //UIセットアップ
    public void UIInitlaize(RobotStatusController statusControl)
    {
        hpBar.ValueSetUp(statusControl.nowStatus.maxHP, statusControl.hp);
        boostBar.ValueSetUp(statusControl.nowStatus.boostAmount, statusControl.nowStatus.boostAmount);
        quorraBar.ValueSetUp(statusControl.nowStatus.quorraAmount, statusControl.nowStatus.quorraAmount-statusControl.usedQuorra);

        weaponInfoUI.SetUp(statusControl.GetComponent<RobotSetUpController>());


        //イベントのサブスクライブなどを行う
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
