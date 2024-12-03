using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "ModuleData", menuName = "Data/CreateModule")]
public class ModuleData : ScriptableObject
{
    //’Ê‚µ”Ô†
    public int moduleNumber;

    public int price; //”Ì”„‰¿Ši

    public string moduleName;

    [TextArea]
    public string description;
}
