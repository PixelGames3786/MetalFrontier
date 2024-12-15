using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.XR;

public class WeaponUseInfoUI : MonoBehaviour
{
    public Dictionary<LegacySettingData.WeaponSetPosi, WeaponBase> weaponDic=new Dictionary<LegacySettingData.WeaponSetPosi, WeaponBase>();

    public SerializableDictionary<LegacySettingData.WeaponSetPosi, RectTransform> gaugeDic;

    public SerializableDictionary<LegacySettingData.WeaponSetPosi, TextMeshProUGUI> nameTextDic;

    public SerializableDictionary<LegacySettingData.WeaponSetPosi, TextMeshProUGUI> leftBulletTextDic;


    private Dictionary<LegacySettingData.WeaponSetPosi, bool> isIntervalDic=new Dictionary<LegacySettingData.WeaponSetPosi, bool>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //UI更新
        foreach (var pair in isIntervalDic)
        {
            if (pair.Value)
            {
                WeaponBase weapon = weaponDic[pair.Key];

                float ratio = weapon.intervalElapseTime / weapon.useInterval;
                gaugeDic[pair.Key].localScale = new Vector3(ratio, 1, 1);
            }
        }
    }

    //武器使用後、インターバルに入るとき
    public void IntervalStart(LegacySettingData.WeaponSetPosi posi)
    {
        isIntervalDic[posi] = true;

        gaugeDic[posi].localScale = new Vector3(0, 1, 1);
    }

    //武器使用後、インターバル待ちが終わった時
    public void IntervalEnd(LegacySettingData.WeaponSetPosi posi)
    {
        isIntervalDic[posi] = false;

        gaugeDic[posi].localScale = new Vector3(1, 1, 1);
    }

    public void LeftBulletChange(LegacySettingData.WeaponSetPosi posi)
    {
        WeaponBase weapon = weaponDic[posi];

        leftBulletTextDic[posi].text = $"{weapon.leftBulletNum}/{weapon.maxBulletNum}";
    }

    //初期設定
    public void SetUp(RobotSetUpController controller)
    {
        foreach(var pair in controller.createdWeaponsObj)
        {
            if (pair.Value != null)
            {
                WeaponBase weapon = pair.Value.GetComponent<WeaponBase>();

                weaponDic[pair.Key] = weapon;
                gaugeDic[pair.Key].localScale = new Vector3(1, 1, 1);
                nameTextDic[pair.Key].text = weapon.weaponData.modelName;
                isIntervalDic[pair.Key] = false;

                leftBulletTextDic[pair.Key].text = $"{weapon.leftBulletNum}/{weapon.maxBulletNum}";

                weaponDic[pair.Key].onLeftBulletChange += LeftBulletChange;
                weaponDic[pair.Key].OnStartInterval += IntervalStart;
                weaponDic[pair.Key].OnEndInterval += IntervalEnd;

            }else
            {
                weaponDic[pair.Key] = null;
                gaugeDic[pair.Key].localScale = new Vector3(0,1,1);
                nameTextDic[pair.Key].text = "";
                leftBulletTextDic[pair.Key].text = "";
            }
        }
    }
}
