using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class MyCharaTest : MonoBehaviour
{
    private Rigidbody rb;

    public Transform Target;

    public GameObject BulletPrefab;

    public LineRenderer lineRenderer;

    public float MoveSpeed, BoostSpeed, BulletSpeed, BoostAmount, BoostRecoverRate;

    [Header("最高速")]
    public float maxVelocity, boostMaxVelocity;

    public float distance;

    private float UsedBoost;

    [SerializeField]
    private bool isBoosting;

    public CameraSwing cameraSwing;

    private InputControls testControl;

    // 2軸入力を受け取る想定のAction
    private InputAction moveAction, shotAction, boostAct;

    private Vector3 moveDirection;

    //lineRenderer関連
    private List<Vector3> linePoints = new List<Vector3>();


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        testControl = new InputControls();

        // InputActionsの設定を取得
        moveAction = testControl.Player.Move;
        shotAction = testControl.Player.LeftArmPress;
        boostAct = testControl.Player.Boost;

        shotAction.performed += OnShot;
        boostAct.performed += StartBoost;
        boostAct.canceled += EndBoost;
        //cameraSwing.StartSwing();
    }


    public void Update()
    {
        Vector2 inputVec = moveAction.ReadValue<Vector2>();

        // キャラクターの向いている方向に基づいて移動ベクトルを計算
        moveDirection = (transform.forward * inputVec.y) + (transform.right * inputVec.x).normalized;

        if (Target)
        {
            transform.LookAt(Target.position);
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
            velocity = new Vector3(Mathf.Clamp(rb.velocity.x, boostMaxVelocity * -1, boostMaxVelocity), Mathf.Clamp(rb.velocity.y, boostMaxVelocity * -1, boostMaxVelocity), Mathf.Clamp(rb.velocity.z, boostMaxVelocity * -1, boostMaxVelocity));
        }
        else
        {
            if (linePoints.Count>1)
            {
                linePoints.Add(transform.position);
                linePoints.RemoveAt(0);
                linePoints.RemoveAt(1);

                DrawLine();
            }
            rb.AddForce(moveDirection * MoveSpeed, ForceMode.Impulse);

            //最大移動量を制限
            velocity = new Vector3(Mathf.Clamp(rb.velocity.x, maxVelocity * -1, maxVelocity), Mathf.Clamp(rb.velocity.y, maxVelocity * -1, maxVelocity), Mathf.Clamp(rb.velocity.z, maxVelocity * -1, maxVelocity));
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

    //射撃ボタンが押されたとき
    private void OnShot(InputAction.CallbackContext context)
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red);

        Transform bulletObj = Instantiate(BulletPrefab, transform.position, transform.rotation).transform;

        Rigidbody bulletRb = bulletObj.GetChild(0).GetComponent<Rigidbody>();

        Physics.IgnoreCollision(bulletRb.gameObject.GetComponent<Collider>(), gameObject.GetComponent<Collider>(), true);

        bulletRb.AddForce(transform.forward * BulletSpeed, ForceMode.Impulse);
    }

    //ブーストボタンが押されたとき
    private void StartBoost(InputAction.CallbackContext context)
    {
        print("ブースと開始");

        isBoosting = true;
    }

    //ブーストボタンが離されたとき
    private void EndBoost(InputAction.CallbackContext context)
    {
        print("ブースと終了");

        isBoosting = false;
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        shotAction.Dispose();
        boostAct.Dispose();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        shotAction.Enable();
        boostAct.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        shotAction.Disable();
        boostAct.Disable();
    }
}
