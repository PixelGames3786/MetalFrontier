using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArmTest : MonoBehaviour
{
    public Transform target,pivotObj,robotParent;

    public Vector3 upVector,factor,maxAngle,defaultAngle;

    public bool isRestrect;

    private Quaternion defaultQuaternion;
    public Vector3 defaultDir;

    public float angleToTargetX, angleToTargetY;

    // Start is called before the first frame update
    void Start()
    {
        defaultQuaternion = transform.localRotation;

        defaultDir = -transform.up.normalized;
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
            Vector3 directionToTarget = tagPosi - pivotObj.position;
            //directionToTarget = new Vector3(directionToTarget.x * factor.x, directionToTarget.y * factor.y, directionToTarget.z * factor.z);

            // Œ»İ‚Ì•ûŒü
            Vector3 currentDirection = pivotObj.forward;
            currentDirection = new Vector3(currentDirection.x * factor.x, currentDirection.y * factor.y, currentDirection.z * factor.z);

            Vector3 currentRobotDir = robotParent.forward;

            // Y²i…•½–Êj‚Ì‰ñ“]Šp“x‚ğæ“¾
            Vector3 planeToTargetY= Vector3.ProjectOnPlane(directionToTarget, robotParent.up);// Y¬•ª‚ğƒ[ƒ‚É‚µ‚ÄXZ•½–Ê‚É“Š‰e
            angleToTargetY = Vector3.SignedAngle(currentRobotDir, planeToTargetY, robotParent.up);

            // X²i‚’¼–Êj‚Ì‰ñ“]Šp“x‚ğæ“¾
            Vector3 planeToTargetX= Vector3.ProjectOnPlane(directionToTarget, robotParent.right);// Y¬•ª‚ğƒ[ƒ‚É‚µ‚ÄXZ•½–Ê‚É“Š‰e
            Vector3 directionToTargetX = new Vector3(0, directionToTarget.y, directionToTarget.z); // X¬•ª‚ğƒ[ƒ‚É‚µ‚ÄYZ•½–Ê‚É“Š‰e
            angleToTargetX = Vector3.SignedAngle(currentRobotDir, planeToTargetX, robotParent.right);

            // -45`45‹‚Ì”ÍˆÍ‚É§ŒÀ
            float clampedAngleX = Mathf.Clamp(angleToTargetX, -45f, 45f);
            float clampedAngleY = Mathf.Clamp(angleToTargetY, -45f, 45f);

            //§ŒÀ‚µ‚½’l‚ğg‚Á‚Äƒ^[ƒQƒbƒg‚ÉŒü‚©‚¤‰ñ“]‚ğŒvZ
            Quaternion targetRotation = Quaternion.AngleAxis(clampedAngleX, robotParent.right) * Quaternion.AngleAxis(clampedAngleY, robotParent.up) * Quaternion.LookRotation(currentRobotDir,upVector);

            pivotObj.rotation = targetRotation;

            //pivotObj.localEulerAngles=new Vector3(pivotObj.localEulerAngles.x,nowEuler.y,nowEuler.z);
        }
    }
}
