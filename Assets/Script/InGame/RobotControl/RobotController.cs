using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using DG.Tweening;

public class RobotController : MonoBehaviour
{
    private Rigidbody rb;

    public RobotSetUpController setupControl;
    public RobotStatusController statusControl;

    private LegacySpecStatus status;

    //ターゲットがいない際にロボットの視線の先に配置するオブジェクト　CinemachineCameraに入れるやつ
    private Transform target;

    public LineRenderer lineRenderer;

    public Animator ArmatureAnimator;

    //姿勢制御系
    [SerializeField]
    private ShoulderTest leftShouler, rightShoulder;

    [SerializeField]
    private ArmTest leftArm, rightArm;

    //武器系
    public WeaponBase LeftArmWeapon { get; private set; }
    public WeaponBase RightArmWeapon { get; private set; }
    public WeaponBase LShoulderWeapon { get; private set; }
    public WeaponBase RShoulderWeapon { get; private set; }

    public float dragValue,rotateSpeed;

    [SerializeField]
    private bool lookTargetSetting;

    [SerializeField]
    private bool isBoosting,isRising,isInAir;//ブースト中　上昇中　空中にいるか

    [SerializeField]
    private bool canJump = true,camReseting; //カメラリセット中か　操作可能状態か
    public bool canBoost = true,canOperate; //ブースト可能か

    private bool isAwakening; //覚醒中か

    private Vector2 moveInput;

    private Vector3 moveDirection;

    //lineRenderer関連
    private List<Vector3> linePoints = new List<Vector3>();

    private Dictionary<LegacySettingData.WeaponSetPosi, WeaponTest> weaponDic = new Dictionary<LegacySettingData.WeaponSetPosi, WeaponTest>();


    private void Awake()
    {
        IgnoreChildrenCollider();

        //cameraSwing.StartSwing();
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        setupControl = GetComponent<RobotSetUpController>();
        statusControl = GetComponent<RobotStatusController>();

        status = statusControl.nowStatus;
    }

    public void Update()
    {
        if (!canOperate) return; //操作不能状態ならば行わない

        //向いている方向に基づいて移動ベクトルを計算
        moveDirection = (transform.forward * moveInput.y) + (transform.right * moveInput.x).normalized;

        //ターゲッティング中かどうか
        if (target && lookTargetSetting)
        {
            //ターゲットの方を向く
            CustomLookAtYAxis(target);
            //transform.LookAt(Target.position);
        }
    }

    private void FixedUpdate()
    {
        if (!canOperate) return; //操作不能状態ならば行わない

        Vector3 velocity = new Vector3();

        if (isBoosting)
        {
            linePoints.Add(transform.position);

            if (linePoints.Count > 30)  // 点の最大数を50に制限
            {
                linePoints.RemoveAt(0);
            }

            if(lineRenderer) DrawLine();

            rb.AddForce(moveDirection * status.boostSpeed*Time.deltaTime, ForceMode.Impulse);

            //最大移動量を制限
            velocity.x = Mathf.Clamp(rb.velocity.x, status.boostMaxVel.x * -1, status.boostMaxVel.x);
            velocity.y = Mathf.Clamp(rb.velocity.y, status.boostMaxVel.y * -1, status.boostMaxVel.y);
            velocity.z = Mathf.Clamp(rb.velocity.z, status.boostMaxVel.z * -1, status.boostMaxVel.z);

            //ブースト使用量をステータスから計算
            float boostUsage = Time.deltaTime * statusControl.nowStatus.boostUseRate;

            //使用済みブースト量を増やす
            statusControl.BoostUse(boostUsage);
        }
        else
        {
            if (linePoints.Count > 1)
            {
                linePoints.Add(transform.position);
                linePoints.RemoveAt(0);
                linePoints.RemoveAt(1);

                if (lineRenderer) DrawLine();
            }
            rb.AddForce(moveDirection * status.moveSpeed * Time.deltaTime, ForceMode.Impulse);

            //最大移動量を制限
            velocity.x = Mathf.Clamp(rb.velocity.x, status.maxVel.x * -1, status.maxVel.x);
            velocity.y = Mathf.Clamp(rb.velocity.y, status.maxVel.y * -1, status.maxVel.y);
            velocity.z = Mathf.Clamp(rb.velocity.z, status.maxVel.z * -1, status.maxVel.z);

        }

        //上昇中ならば
        if (isRising)
        {
            rb.AddForce(new Vector3(0, status.riseForce * Time.deltaTime, 0), ForceMode.Impulse);

            //ブースト使用量をステータスから計算
            float boostUsage = Time.deltaTime * statusControl.nowStatus.boostUseRate;

            //使用済みブースト量を増やす
            statusControl.BoostUse(boostUsage);
        }

        //ブースト中かつ上昇中以外の時にブースト量を回復
        if (!isRising && !isBoosting)
        {
            float boostRecover = Time.deltaTime * statusControl.nowStatus.boostRecoverRate;

            //ブースト量を回復
            statusControl.BoostRecover(boostRecover);
        }

        //何も入力がない時は抵抗を増やしてゆっくり止める
        if (moveDirection == Vector3.zero && !isInAir)
        {
            rb.drag = dragValue;  //ドラッグを適用して減速

            ArmatureAnimator.SetBool("IsMoving", false);
        }
        else
        {
            rb.drag = 0;  //入力があるときはドラッグをオフにする

            ArmatureAnimator.SetBool("IsMoving", true);

        }

        rb.velocity = velocity;
    }

    //ターゲット設定
    public void SetTarget(Transform tag)
    {
        target = tag;

        leftShouler.target = tag;
        rightShoulder.target = tag;
        leftArm.target = tag;
        rightArm.target = tag;
    }


    public Vector3 TargetVector()
    {
        Debug.DrawLine(transform.position, target.position);

        return (transform.position - target.position).normalized;
    }

    private void DrawLine()
    {
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray()); // オブジェクトの位置情報をセット
    }

    public void moveDirInput(Vector2 input)
    {
        moveInput = input;

        ArmatureAnimator.SetFloat("Move_h", moveInput.x);
        ArmatureAnimator.SetFloat("Move_v", moveInput.y);
    }

    //左手武器射撃
    public void LeftArmShot()
    {
        if (setupControl.createdWeaponsObj[LegacySettingData.WeaponSetPosi.LeftArm]!=null)
        {
            if (LeftArmWeapon==null)
            {
                LeftArmWeapon = setupControl.createdWeaponsObj[LegacySettingData.WeaponSetPosi.LeftArm].GetComponent<WeaponBase>();
            }

            LeftArmWeapon.Use(target);
        }
    }

    //右手武器射撃
    public void RightArmShot()
    {
        if (setupControl.createdWeaponsObj[LegacySettingData.WeaponSetPosi.RightArm] != null)
        {
            if (RightArmWeapon == null)
            {
                RightArmWeapon = setupControl.createdWeaponsObj[LegacySettingData.WeaponSetPosi.RightArm].GetComponent<WeaponBase>();
            }

            RightArmWeapon.Use(target);
        }
    }

    //左肩武器使用
    public void LeftShoulderShot()
    {
        if (setupControl.createdWeaponsObj[LegacySettingData.WeaponSetPosi.LeftShoulder] != null)
        {
            if (LShoulderWeapon == null)
            {
                LShoulderWeapon = setupControl.createdWeaponsObj[LegacySettingData.WeaponSetPosi.LeftShoulder].GetComponent<WeaponBase>();
            }

            LShoulderWeapon.Use(target);
        }
    }

    //右肩武器使用
    public void RightShoulderShot()
    {
        if (setupControl.createdWeaponsObj[LegacySettingData.WeaponSetPosi.RightShoulder] != null)
        {
            if (RShoulderWeapon == null)
            {
                RShoulderWeapon = setupControl.createdWeaponsObj[LegacySettingData.WeaponSetPosi.RightShoulder].GetComponent<WeaponBase>();
            }

            RShoulderWeapon.Use(target);
        }
    }

    //覚醒状態に入る
    public void RobotStartAwake()
    {
        isAwakening = true;
    }

    //覚醒状態から出る
    public void RobotEndAwake()
    {
        isAwakening = false;
    }

    //ブースト開始
    public void StartBoost()
    {
        if (!canBoost) return;

        print("ブースと開始");

        isBoosting = true;

        ArmatureAnimator.SetBool("IsBoosting", true);
    }

    //ブースト終了
    public void EndBoost()
    {
        print("ブースと終了");

        isBoosting = false;

        ArmatureAnimator.SetBool("IsBoosting", false);
    }

    //ジャンプ
    public void OnJump()
    {
        if (!isInAir)
        {
            rb.AddForce(new Vector3(0, status.jumpForce, 0), ForceMode.Impulse);

            canJump = false;
            isInAir = true;

            ArmatureAnimator.SetBool("IsInAir",true);
        }
    }

    //ジャンプボタン長押し（上昇）がされたとき
    public void StartRise()
    {
        if (!canBoost) return;

        isRising = true;
    }

    public void EndRise()
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

            // 現在の回転とターゲットの回転を補完して徐々に回転
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
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
            canJump = true;
            isInAir = false;

            ArmatureAnimator.SetBool("IsInAir", false);
        }
    }

}
