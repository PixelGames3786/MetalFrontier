using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMissionCondition
{


    //ƒNƒŠƒAğŒ‚ª‡’v‚µ‚Ä‚¢‚é‚©‚Ç‚¤‚©
    public bool ClearConditionCheck();

    //¸”sğŒ‚ª‡’v‚µ‚Ä‚¢‚é‚©‚Ç‚¤‚©
    public bool FailConditionCheck();
}
