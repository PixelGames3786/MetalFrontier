using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class CameraSwing : MonoBehaviour
{
    //カメラがマウスの動きに合わせて動くか
    public bool IsSwing=false;

    //カメラが特定の対象を追いかけるか
    public bool IsFollowing=false;

    //プレイヤーがターゲッティングしているか
    public bool IsTargetting = false;

    public bool IsOutField = false;

    public Transform FollowTarget;

    public float SwingSpeed,Smooth,TargetDepth,AngleTest;

    public MyCharaTest chara;

    public Vector3 rotateMin, rotateMax;

    private Vector2 lastMousePosi,nowMousePosi;

    private Vector3 DefaultFollowOffset,FollowOffset;

    private InputAction resetAction;

    private float saveangle;

    private void Awake()
    {
        //InputSystemリセット
        InputControls testControl= new InputControls();

        resetAction = testControl.Player.CameraReset;

        resetAction.performed += CameraReset;
    }

    // Start is called before the first frame update
    void Start()
    {
        //オフセット設定
        if (FollowTarget)
        {
            DefaultFollowOffset= transform.position-FollowTarget.position;
            FollowOffset = DefaultFollowOffset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //カメラ回転処理
        if (IsSwing)
        {
            var pointer = Pointer.current;
            nowMousePosi = pointer.position.ReadValue();

            var diff = (nowMousePosi - lastMousePosi)*SwingSpeed;

            //transform.RotateAround()をしようしてメインカメラを回転させる
            transform.RotateAround(transform.position, Vector3.up, diff.x);
            transform.RotateAround(transform.position, transform.right, diff.y);

            //Vector3 rotation = transform.rotation.eulerAngles;

            //rotation = new Vector3(Mathf.Clamp(rotation.x,rotateMin.x,rotateMax.x), Mathf.Clamp(rotation.y, rotateMin.y, rotateMax.y), Mathf.Clamp(rotation.z, rotateMin.z, rotateMax.z));

            //transform.rotation=Quaternion.Euler(rotation);

            lastMousePosi = pointer.position.ReadValue();
        }

        //カメラ追従処理
        if (IsFollowing)
        {
            //ターゲッティング処理
            if (IsTargetting)
            {
                // 平面の法線ベクトル（上向きベクトルとする）
                var planeNormal = Vector3.up;

                // 平面に投影されたベクトルを求める
                var flattenedForwardA = Vector3.ProjectOnPlane(transform.forward, planeNormal);
                var flattenedDirectionB = Vector3.ProjectOnPlane(FollowTarget.position - transform.position, planeNormal);

                //Debug.DrawLine(transform.position, transform.position + flattenedForwardA * 5f, Color.blue);  // カメラの前方向を青で表示
                //Debug.DrawLine(transform.position, transform.position + flattenedDirectionB * 5f, Color.red); // キャラクターへの方向を赤で表示


                // objectAの前方向とobjectBへの方向ベクトル間の角度を取得
                float angle = Vector3.SignedAngle(flattenedForwardA, flattenedDirectionB, Vector3.up); // SignedAngleで正負を含む角度を取得


                Vector3 nowOffset = Vector3.one;

                if (Mathf.Abs(angle)<AngleTest)
                {
                    
                }
                else if(!IsOutField)
                {
                    IsOutField = true;

                    // 平面に投影されたベクトルを求める
                    flattenedForwardA = Vector3.ProjectOnPlane(transform.forward, planeNormal);
                    flattenedDirectionB = Vector3.ProjectOnPlane(FollowTarget.position - transform.position, planeNormal);

                    // objectAの前方向とobjectBへの方向ベクトル間の角度を取得
                    angle = Vector3.SignedAngle(flattenedForwardA, flattenedDirectionB, Vector3.up); // SignedAngleで正負を含む角度を取得

                    saveangle = (Mathf.Abs(angle)-5)*Mathf.Sign(angle);
                    //FollowOffset= transform.position - FollowTarget.position;
                }

                /*

                if (IsOutField)
                {
                    Debug.Log("X軸を無視した相対角度: " + angle);

                    // 回転しているので、現在のカメラ位置をターゲットからのオフセットで計算
                    nowOffset = FollowTarget.rotation * DefaultFollowOffset;

                    Vector3 neoposition = FollowTarget.position + nowOffset;

                    nowOffset = Quaternion.Euler(new Vector3(0f, angle, 0f)) * nowOffset;
                }
                else
                {

                    // 回転しているので、現在のカメラ位置をターゲットからのオフセットで計算
                    nowOffset = Quaternion.Euler(new Vector3(0f, angle, 0f)) * FollowOffset;
                }

                */

                if (IsOutField)
                {

                    // 回転しているので、現在のカメラ位置をターゲットからのオフセットで計算
                    nowOffset = FollowTarget.rotation * DefaultFollowOffset;

                    nowOffset = Quaternion.Euler(new Vector3(0f, saveangle, 0f)) * nowOffset;


                    Debug.DrawLine(transform.position, transform.position - nowOffset, Color.magenta);


                    Vector3 desiredPosi = FollowTarget.position +  nowOffset;


                    // カメラの位置をスムーズに補間
                    Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosi, Smooth);

                    // カメラの位置を更新
                    transform.position = desiredPosi;
                    Vector3 directionToTarget = (chara.Target.position - transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    transform.rotation = targetRotation;

                    Debug.DrawLine(transform.position, desiredPosi, Color.green);



                    // 平面に投影されたベクトルを求める
                    flattenedForwardA = Vector3.ProjectOnPlane(transform.forward, planeNormal);
                    flattenedDirectionB = Vector3.ProjectOnPlane(FollowTarget.position - transform.position, planeNormal);

                    // objectAの前方向とobjectBへの方向ベクトル間の角度を取得
                    angle = Vector3.SignedAngle(flattenedForwardA, flattenedDirectionB, Vector3.up); // SignedAngleで正負を含む角度を取得

                    IsOutField = false;

                    FollowOffset = nowOffset;


                    if (Vector3.Distance(transform.position,desiredPosi)<0.01f)
                    {
                        Debug.Log("おわったよ！！！！！！！！"+ Vector3.Distance(transform.position, desiredPosi));

                        
                        //FollowOffset = Quaternion.Euler(new Vector3(0f, angle, 0f)) * FollowOffset;
                    }
                }
                else
                {
                    Debug.DrawLine(transform.position, transform.position- FollowOffset, Color.red);

                    Debug.Log("X軸を無視した相対角度: " + angle);
                    nowOffset = Quaternion.Euler(new Vector3(0f, angle, 0f)) * FollowOffset;

                    Debug.DrawLine(transform.position, transform.position - nowOffset, Color.blue);



                    Vector3 desiredPosi = FollowTarget.position + nowOffset;

                    // カメラの位置をスムーズに補間
                    Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosi, Smooth);

                    // カメラの位置を更新
                    transform.position = desiredPosi;

                    Vector3 directionToTarget = (chara.Target.position - transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    transform.rotation = targetRotation;

                    // 平面に投影されたベクトルを求める
                    flattenedForwardA = Vector3.ProjectOnPlane(transform.forward, planeNormal);
                    flattenedDirectionB = Vector3.ProjectOnPlane(FollowTarget.position - transform.position, planeNormal);

                    // objectAの前方向とobjectBへの方向ベクトル間の角度を取得
                    angle = Vector3.SignedAngle(flattenedForwardA, flattenedDirectionB, Vector3.up); // SignedAngleで正負を含む角度を取得

                    //Debug.Log("返還後の相対角度: " + angle);

                }



                //カメラから対象物までの角度を取得
                //Debug.Log(Quaternion.LookRotation(FollowTarget.position).eulerAngles.y);

            }
            else
            {
                transform.position = FollowTarget.position + FollowOffset;
            }
        }

        
    }

    //追従対象の正面を向くようにカメラを回転
    private void CameraReset(InputAction.CallbackContext context)
    {
        if (IsTargetting) return;

        // 回転しているので、現在のカメラ位置をターゲットからのオフセットで計算
        FollowOffset = FollowTarget.rotation * DefaultFollowOffset;

        Vector3 desiredPosi = FollowTarget.position + FollowOffset;

        // カメラの位置をスムーズに補間
        Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosi, Smooth);

        // カメラの位置を更新
        transform.position = desiredPosi;
        transform.LookAt(FollowTarget.forward*TargetDepth);
    }

    bool ApproximatelyEqual(Vector3 v1, Vector3 v2, float epsilon = 0.0001f)
    {
        return Mathf.Abs(v1.x - v2.x) < epsilon &&
               Mathf.Abs(v1.y - v2.y) < epsilon &&
               Mathf.Abs(v1.z - v2.z) < epsilon;
    }

    public void StartSwing()
    {
        if (IsSwing) return;

        IsSwing = true;

        var pointer = Pointer.current;
        lastMousePosi = pointer.position.ReadValue();
    }

    private void OnDestroy()
    {
        resetAction.Dispose();
    }

    private void OnEnable()
    {
        resetAction.Enable();
    }

    private void OnDisable()
    {
        resetAction.Disable();
    }
}
