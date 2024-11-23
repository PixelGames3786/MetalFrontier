using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShoulderTest : MonoBehaviour
{
    public Transform target,pivotObj,endObj,robotParent;

    public float maxAngle;

    public Vector3 upVector,factor;

    private Quaternion defaultQuaternion;
    public Vector3 defaultDir;

    public float angleToTargetX, angleToTargetY;

    // Start is called before the first frame update
    void Start()
    {
        defaultQuaternion=transform.localRotation;

        defaultDir = transform.forward.normalized;
    }

    // Update is called once per frame
    void Update()
    {
        if (!target)
        {
            transform.localRotation = defaultQuaternion;
        }
        else
        {
            Vector3 tagPosi = target.position;

            // ƒ^[ƒQƒbƒg‚Ì•ûŒü‚ğŒvZ
            Vector3 directionToTarget = tagPosi - endObj.position;
            //directionToTarget = new Vector3(directionToTarget.x * factor.x, directionToTarget.y * factor.y, directionToTarget.z * factor.z);

            Vector3 currentRobotDir = robotParent.forward;

            //•½–Êã‚É“Š‰e‚·‚é
            Vector3 planeToTarget =Vector3.ProjectOnPlane(directionToTarget, robotParent.right);

            // X²i‚’¼–Êj‚Ì‰ñ“]Šp“x‚ğæ“¾
            Vector3 directionToTargetX = new Vector3(0, directionToTarget.y, directionToTarget.z); // X¬•ª‚ğƒ[ƒ‚É‚µ‚ÄYZ•½–Ê‚É“Š‰e
            angleToTargetX = Vector3.SignedAngle(currentRobotDir, planeToTarget, robotParent.right);

            // -45`45‹‚Ì”ÍˆÍ‚É§ŒÀ
            float clampedAngleX = Mathf.Clamp(angleToTargetX, -30f, 30f);

            //§ŒÀ‚µ‚½’l‚ğg‚Á‚Äƒ^[ƒQƒbƒg‚ÉŒü‚©‚¤‰ñ“]‚ğŒvZ
            Quaternion targetRotation = Quaternion.AngleAxis(clampedAngleX, robotParent.right) * Quaternion.LookRotation(robotParent.forward, upVector);

            pivotObj.rotation = targetRotation;
        }
    }
}
