using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITargetable
{
    //�^�[�Q�b�e�B���O�\�ȏ�Ԃ��i����ł���^�[�Q�b�e�B���O�o���Ȃ��A�Ƃ��j
    public bool CanTarget();

    //�J�����O�ɏo�Ă���Ȃ�
    public bool IsVisible();
}
