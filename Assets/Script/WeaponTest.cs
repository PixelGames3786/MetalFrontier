using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WeaponTest : MonoBehaviour
{
    public GameObject BulletPrefab;

    public Transform ShotPosi;

    public Transform pivotObj;

    public float maxAngle,BulletSpeed;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shot(Vector3 tagPosi)
    {
        // ターゲットの方向を計算
        Vector3 directionToTarget = tagPosi - pivotObj.position;
        directionToTarget.y = 0; // 水平方向のみの回転を考慮したい場合はY軸を無視する

        // ターゲットに向かう回転を計算
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        pivotObj.rotation = targetRotation;

        // 回転角度を制限（上下左右の制限）
        Vector3 eulerAngles = pivotObj.localEulerAngles;

        print("前"+eulerAngles);

        /*
        // 上下の制限（X軸）
        if (eulerAngles.x > maxAngle && eulerAngles.x < 180f)
        {
            eulerAngles.x = maxAngle;
        }
        else if (eulerAngles.x < 360f - maxAngle && eulerAngles.x > 180f)
        {
            eulerAngles.x = 360f - maxAngle;
        }

        // 左右の制限（Y軸）
        if (eulerAngles.y > maxAngle && eulerAngles.y < 180f)
        {
            eulerAngles.y = maxAngle;
        }
        else if (eulerAngles.y < 360f - maxAngle && eulerAngles.y > 180f)
        {
            eulerAngles.y = 360f - maxAngle;
        }
        */

        if (eulerAngles.x>=180) eulerAngles.x -= 360;
        if (eulerAngles.y >= 180) eulerAngles.y -= 360; 
        if (eulerAngles.z >= 180) eulerAngles.z -= 360; 

        // 角度を適用
        eulerAngles.x = Mathf.Clamp(eulerAngles.x, maxAngle * -1, maxAngle);
        eulerAngles.y = Mathf.Clamp(eulerAngles.y, maxAngle * -1, maxAngle);
        eulerAngles.z = Mathf.Clamp(eulerAngles.z, maxAngle * -1, maxAngle);

        pivotObj.localEulerAngles = eulerAngles;

        print("後" + eulerAngles);




        print("OnShot!");

        Debug.DrawRay(pivotObj.position, pivotObj.forward, Color.red);

        Transform bulletObj = Instantiate(BulletPrefab, ShotPosi.position, pivotObj.rotation).transform;

        Rigidbody bulletRb = bulletObj.GetChild(0).GetComponent<Rigidbody>();

        Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>(), true);

        bulletRb.AddForce(pivotObj.forward * BulletSpeed, ForceMode.Impulse);
    }
}
