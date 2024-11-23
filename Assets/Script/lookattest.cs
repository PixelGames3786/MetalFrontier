using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookattest : MonoBehaviour
{
    public Transform target; // ターゲット

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
        // RootMotionを無効にし、アニメーションによるポジション変更を無視
        if (animator)
        {
            transform.position = animator.rootPosition;
            transform.rotation = animator.rootRotation;
        }
    }

    // Y軸回転のみを適用するLookAt関数
    void CustomLookAtYAxis(Transform target)
    {
        // ターゲットまでの方向を計算する
        Vector3 directionToTarget = target.position - transform.position;

        // Y軸の回転を考慮したベクトルを計算（X-Z平面のみ）
        directionToTarget.y = 0;

        // 方向ベクトルが有効な場合にだけ回転を行う（ゼロベクトルを回避）
        if (directionToTarget != Vector3.zero)
        {
            // Y軸の回転を反映させる
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            Vector3 angle=targetRotation.eulerAngles;

            //angle.y += 90f;
            transform.rotation = Quaternion.Euler(angle);
        }
    }
}
