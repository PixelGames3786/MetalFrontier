using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMissionCondition
{


    //�N���A���������v���Ă��邩�ǂ���
    public bool ClearConditionCheck();

    //���s���������v���Ă��邩�ǂ���
    public bool FailConditionCheck();
}
