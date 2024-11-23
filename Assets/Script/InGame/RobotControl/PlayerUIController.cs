using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIController : MonoBehaviour
{
    //UI類
    public BarUI hpBar, boostBar;
    public RockOnUI rockOnUI;
    public WeaponUseInfoUI weaponInfoUI;
    public OutOfAreaUI outOfAreaUI;
    public BackToBaseUI backToBaseUI;

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

    //UIセットアップ
    public void UIInitlaize(RobotStatusController statusControl)
    {
        hpBar.ValueSetUp(statusControl.nowStatus.maxHP, statusControl.hp);
        boostBar.ValueSetUp(statusControl.nowStatus.boostAmount, statusControl.nowStatus.boostAmount);
        weaponInfoUI.SetUp(statusControl.GetComponent<RobotSetUpController>());


        //イベントのサブスクライブなどを行う
        statusControl.OnHPChanged += HPBarChange;
        statusControl.OnBoostChanged += BoostBarChange;
        statusControl.GetComponent<RobotPlayerInput>().OutOfAreaCountDown += CountDownChange;
    }
}
