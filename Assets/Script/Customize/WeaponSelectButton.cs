using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSelectButton : MonoBehaviour,IButton
{
    public CustomizeUIController controller;

    public LegacySettingData.WeaponSetPosi setPosi;

    public WeaponPartsData.SetType setType;

    public string FuncName;

    public void OnClick()
    {
        object[] args = {setType ,setPosi};

        controller.CallStateFuncArg(FuncName,args);


    }
}
