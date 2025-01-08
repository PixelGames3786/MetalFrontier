using UnityEngine;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

public abstract class UnitBase : MonoBehaviour
{
    public abstract void UnitSetUp();

    public abstract void UnitActivate();

    public abstract void UnitDeactivate();
}