using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RobotShoulderControl : MonoBehaviour
{
    public Transform target,pivotObj,endObj,robotParent;

    public float maxAngle;

    public Vector3 upVector,factor;

    private Quaternion defaultQuaternion;
    public Vector3 defaultDir;

    public float angleToTargetX, angleToTargetY;

    private bool isLookAt=true;

    // Start is called before the first frame update
    void Start()
    {
        defaultQuaternion=transform.localRotation;

        defaultDir = transform.forward.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        //ターゲットを注視中っでなければ
        if (!isLookAt || !target)
        {
            transform.localRotation = defaultQuaternion;
            return;
        }

        Vector3 tagPosi = target.position;

        // ターゲットの方向を計算
        Vector3 directionToTarget = tagPosi - endObj.position;
        //directionToTarget = new Vector3(directionToTarget.x * factor.x, directionToTarget.y * factor.y, directionToTarget.z * factor.z);

        Vector3 currentRobotDir = robotParent.forward;

        //平面上に投影する
        Vector3 planeToTarget = Vector3.ProjectOnPlane(directionToTarget, robotParent.right);

        // X軸（垂直面）の回転角度を取得
        Vector3 directionToTargetX = new Vector3(0, directionToTarget.y, directionToTarget.z); // X成分をゼロにしてYZ平面に投影
        angleToTargetX = Vector3.SignedAngle(currentRobotDir, planeToTarget, robotParent.right);

        // -45～45°の範囲に制限
        float clampedAngleX = Mathf.Clamp(angleToTargetX, -30f, 30f);

        //制限した値を使ってターゲットに向かう回転を計算
        Quaternion targetRotation = Quaternion.AngleAxis(clampedAngleX, robotParent.right) * Quaternion.LookRotation(robotParent.forward, upVector);

        pivotObj.rotation = targetRotation;
    }

    public void SetLookAt(bool val)
    {
        isLookAt = val;
    }
}
