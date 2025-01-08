using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class GiantAnimationEventHandler : MonoBehaviour
{
    public GiantEnemyControl control;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //animationEvent‚Íˆø”‚ªint string object‚µ‚©ó‚¯•t‚¯‚È‚¢‚Ì‚Åstring‚©‚çbool‚É•ÏŠ·‚·‚é
    public void ChangeIsAttack(string val)
    {
        bool boolVal = val.ToLower() == "true";

        control.isAttack = boolVal;
    }

    public void ShowAttackCollider(string colName)
    {
        control.ShowAttackCollider(colName);
    }

    public void HideAttackCollider(string colName)
    {
        control.HideAttackCollider(colName);
    }

    public void CallStateFunc(string funcName)
    {
        control.CallStateFunc(funcName);
    }
}
