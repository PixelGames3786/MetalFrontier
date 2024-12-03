using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class RobotArmatureTest : MonoBehaviour
{
    public Rigidbody rb;

    public Transform Target,TestTag;

    public GameObject BulletPrefab;

    public LineRenderer lineRenderer;

    public Animator ArmatureAnimator;

    public WeaponTest LeftArmWeapon,RightArmWeapon;

    public float MoveSpeed, BoostSpeed, BulletSpeed, BoostAmount, BoostRecoverRate;
    public float JumpForce;

    [Header("通常時最高速")]
    public float maxVelocity2;
    public float boostMaxVelocity2;

    public Vector3 maxVelocity;
    public Vector3 boostMaxVelocity;

    public float distance;
    public float dragValue;

    private float UsedBoost;

    [SerializeField]
    private bool isBoosting,isRising,isInAir;//ブースト中　上昇中　空中にいるか

    private bool CanJump = true;

    public CameraSwing cameraSwing;

    private InputControls testControl;

    // 2軸入力を受け取る想定のAction
    private InputAction moveAction, LeftArmShotAct,RightArmShotAct, boostAct, JumpAct,RiseAct;

    private Vector3 moveDirection;

    //lineRenderer関連
    private List<Vector3> linePoints = new List<Vector3>();


    private void Awake()
    {
        testControl = new InputControls();

        // InputActionsの設定を取得
        moveAction = testControl.Player.Move;
        LeftArmShotAct = testControl.Player.LeftArmPress;
        RightArmShotAct=testControl.Player.RightArmPress;
        boostAct = testControl.Player.Boost;
        JumpAct = testControl.Player.Jump;
        RiseAct=testControl.Player.Rise;

        LeftArmShotAct.performed += LeftArmShot;
        RightArmShotAct.performed += RightArmShot;
        boostAct.performed += StartBoost;
        boostAct.canceled += EndBoost;
        JumpAct.performed += OnJump;
        RiseAct.performed += StartRise;
        RiseAct.canceled += EndRise;

        IgnoreChildrenCollider();

        //cameraSwing.StartSwing();
    }


    public void Update()
    {
        Vector3 euler = TestTag.eulerAngles;
        euler.x += 5;

        TestTag.eulerAngles = euler;

        Vector2 inputVec = moveAction.ReadValue<Vector2>();

        //Animatorに値を入れる
        ArmatureAnimator.SetFloat("HorizontalMove", inputVec.x);
        ArmatureAnimator.SetFloat("VerticalMove", inputVec.y);

        // キャラクターの向いている方向に基づいて移動ベクトルを計算
        moveDirection = (transform.forward * inputVec.y) + (transform.right * inputVec.x).normalized;

        if (Target)
        {
            CustomLookAtYAxis(Target);
            //transform.LookAt(Target.position);
        }
        else
        {
            Vector3 screenPoint = new Vector3(Screen.width / 2, Screen.height / 2, 5000);
            Vector3 lookPosi = Camera.main.ScreenToWorldPoint(screenPoint);

            Debug.DrawRay(transform.position, lookPosi, Color.red);
            Debug.DrawRay(transform.position, transform.forward, Color.blue);

            transform.Rotate(Vector3.up, inputVec.x);

            //transform.LookAt(lookPosi);
        }

    }

    private void FixedUpdate()
    {
        Vector3 velocity = new Vector3();

        if (isBoosting)
        {
            linePoints.Add(transform.position);

            if (linePoints.Count > 30)  // 点の最大数を50に制限
            {
                linePoints.RemoveAt(0);
            }

            DrawLine();

            rb.AddForce(moveDirection * BoostSpeed, ForceMode.Impulse);

            //最大移動量を制限
            velocity.x = Mathf.Clamp(rb.velocity.x, boostMaxVelocity.x * -1, boostMaxVelocity.x);
            velocity.y = Mathf.Clamp(rb.velocity.y, boostMaxVelocity.y * -1, boostMaxVelocity.y);
            velocity.z = Mathf.Clamp(rb.velocity.z, boostMaxVelocity.z * -1, boostMaxVelocity.z );
        }
        else
        {
            if (linePoints.Count > 1)
            {
                linePoints.Add(transform.position);
                linePoints.RemoveAt(0);
                linePoints.RemoveAt(1);

                DrawLine();
            }
            rb.AddForce(moveDirection * MoveSpeed, ForceMode.Impulse);

            //最大移動量を制限
            velocity.x = Mathf.Clamp(rb.velocity.x, maxVelocity.x * -1, maxVelocity.x);
            velocity.y = Mathf.Clamp(rb.velocity.y, maxVelocity.y * -1, maxVelocity.y);
            velocity.z = Mathf.Clamp(rb.velocity.z, maxVelocity.z * -1, maxVelocity.z);
        }

        if (isRising)
        {
            rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
        }

        //何も入力がない時は抵抗を増やしてゆっくり止める
        // 入力がないときにゆっくり止まるためにドラッグを調整
        if (moveDirection == Vector3.zero)
        {
            rb.drag = dragValue;  // ドラッグを適用して減速
        }
        else
        {
            rb.drag = 0;  // 入力があるときはドラッグをオフにする
        }

        rb.velocity = velocity;
    }


    public Vector3 TargetVector()
    {
        Debug.DrawLine(transform.position, Target.position);

        return (transform.position - Target.position).normalized;
    }

    private void DrawLine()
    {
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray()); // オブジェクトの位置情報をセット
    }

    //左手武器射撃ボタンが押されたとき
    private void LeftArmShot(InputAction.CallbackContext context)
    {
        LeftArmWeapon.Shot(Target.position);

        /*
        print("OnShot!");

        Debug.DrawRay(transform.position, transform.forward, Color.red);

        Transform bulletObj = Instantiate(BulletPrefab, transform.position, transform.rotation).transform;

        Rigidbody bulletRb = bulletObj.GetChild(0).GetComponent<Rigidbody>();

        Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>(), true);

        bulletRb.AddForce(transform.forward * BulletSpeed, ForceMode.Impulse);
        */
    }

    //右手武器射撃ボタンが押されたとき
    private void RightArmShot(InputAction.CallbackContext context)
    {
        RightArmWeapon.Shot(Target.position);

        
    }

    //ブーストボタンが押されたとき
    private void StartBoost(InputAction.CallbackContext context)
    {
        print("ブースと開始");

        isBoosting = true;

        ArmatureAnimator.SetBool("IsBoosting", true);
    }

    //ブーストボタンが離されたとき
    private void EndBoost(InputAction.CallbackContext context)
    {
        print("ブースと終了");

        isBoosting = false;

        ArmatureAnimator.SetBool("IsBoosting", false);

    }

    //ジャンプボタンが押されたとき
    private void OnJump(InputAction.CallbackContext context)
    {
        if (!isInAir)
        {
            rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);

            CanJump = false;
            isInAir = true;

            ArmatureAnimator.SetBool("IsInAir",true);
        }
    }

    //ジャンプボタン長押し（上昇）がされたとき
    private void StartRise(InputAction.CallbackContext context)
    {
        isRising = true;
    }

    private void EndRise(InputAction.CallbackContext context)
    {
        isRising= false;
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

            transform.rotation = targetRotation;
        }
    }

    //子供のコライダーと衝突してブルブル震えたりするので回避する
    private void IgnoreChildrenCollider()
    {
        var thisCollider = GetComponent<Collider>();
        var childrenCollider = transform.GetComponentsInChildren<Collider>();

        foreach (var collider in childrenCollider)
        {
            Physics.IgnoreCollision(thisCollider, collider, true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag=="Ground")
        {
            CanJump = true;
            isInAir = false;

            ArmatureAnimator.SetBool("IsInAir", false);

        }
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        LeftArmShotAct.Dispose();
        RightArmShotAct.Dispose();
        JumpAct.Dispose();
        boostAct.Dispose();
        RiseAct.Dispose();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        LeftArmShotAct.Enable();
        RightArmShotAct.Enable();

        JumpAct.Enable();
        boostAct.Enable();
        RiseAct.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        LeftArmShotAct.Disable();
        RightArmShotAct.Disable();

        JumpAct.Disable();
        boostAct.Disable();
        RiseAct.Disable();
    }
}
