using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NeoCameraSwing : MonoBehaviour
{
    //カメラがマウスの動きに合わせて動くか
    public bool IsSwing=false;

    //カメラが特定の対象を追いかけるか
    public bool IsFollowing=false;

    //プレイヤーがターゲッティングしているか
    public bool IsTargetting = false;

    public Transform FollowTarget;

    public float SwingSpeed,Smooth,TargetDepth;

    public MyCharaTest chara;

    public Vector3 rotateMin, rotateMax;

    private Vector2 lastMousePosi,nowMousePosi;

    private Vector3 DefaultFollowOffset,FollowOffset;

    private InputAction resetAction;

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
                // 回転しているので、現在のカメラ位置をターゲットからのオフセットで計算
                Vector3 nowOffset = FollowTarget.rotation * FollowOffset;

                Vector3 desiredPosi = FollowTarget.position+nowOffset;

                // カメラの位置をスムーズに補間
                Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosi, Smooth);

                // カメラの位置を更新
                transform.position = smoothedPosition;

                //transform.position = FollowTarget.position + (tagOffset);
                transform.LookAt(chara.Target);
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
