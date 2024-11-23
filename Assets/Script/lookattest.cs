using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookattest : MonoBehaviour
{
    public Transform target; // �^�[�Q�b�g

    public Animator animator;

    void Update()
    {
        if (target != null)
        {
            CustomLookAtYAxis(target);
        }
    }

    void OnAnimatorMove()
    {
        // RootMotion�𖳌��ɂ��A�A�j���[�V�����ɂ��|�W�V�����ύX�𖳎�
        if (animator)
        {
            transform.position = animator.rootPosition;
            transform.rotation = animator.rootRotation;
        }
    }

    // Y����]�݂̂�K�p����LookAt�֐�
    void CustomLookAtYAxis(Transform target)
    {
        // �^�[�Q�b�g�܂ł̕������v�Z����
        Vector3 directionToTarget = target.position - transform.position;

        // Y���̉�]���l�������x�N�g�����v�Z�iX-Z���ʂ̂݁j
        directionToTarget.y = 0;

        // �����x�N�g�����L���ȏꍇ�ɂ�����]���s���i�[���x�N�g��������j
        if (directionToTarget != Vector3.zero)
        {
            // Y���̉�]�𔽉f������
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            Vector3 angle=targetRotation.eulerAngles;

            //angle.y += 90f;
            transform.rotation = Quaternion.Euler(angle);
        }
    }
}
