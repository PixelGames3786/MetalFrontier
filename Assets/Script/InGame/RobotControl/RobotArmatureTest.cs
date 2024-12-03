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

    [Header("�ʏ펞�ō���")]
    public float maxVelocity2;
    public float boostMaxVelocity2;

    public Vector3 maxVelocity;
    public Vector3 boostMaxVelocity;

    public float distance;
    public float dragValue;

    private float UsedBoost;

    [SerializeField]
    private bool isBoosting,isRising,isInAir;//�u�[�X�g���@�㏸���@�󒆂ɂ��邩

    private bool CanJump = true;

    public CameraSwing cameraSwing;

    private InputControls testControl;

    // 2�����͂��󂯎��z���Action
    private InputAction moveAction, LeftArmShotAct,RightArmShotAct, boostAct, JumpAct,RiseAct;

    private Vector3 moveDirection;

    //lineRenderer�֘A
    private List<Vector3> linePoints = new List<Vector3>();


    private void Awake()
    {
        testControl = new InputControls();

        // InputActions�̐ݒ���擾
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

        //Animator�ɒl������
        ArmatureAnimator.SetFloat("HorizontalMove", inputVec.x);
        ArmatureAnimator.SetFloat("VerticalMove", inputVec.y);

        // �L�����N�^�[�̌����Ă�������Ɋ�Â��Ĉړ��x�N�g�����v�Z
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

            if (linePoints.Count > 30)  // �_�̍ő吔��50�ɐ���
            {
                linePoints.RemoveAt(0);
            }

            DrawLine();

            rb.AddForce(moveDirection * BoostSpeed, ForceMode.Impulse);

            //�ő�ړ��ʂ𐧌�
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

            //�ő�ړ��ʂ𐧌�
            velocity.x = Mathf.Clamp(rb.velocity.x, maxVelocity.x * -1, maxVelocity.x);
            velocity.y = Mathf.Clamp(rb.velocity.y, maxVelocity.y * -1, maxVelocity.y);
            velocity.z = Mathf.Clamp(rb.velocity.z, maxVelocity.z * -1, maxVelocity.z);
        }

        if (isRising)
        {
            rb.AddForce(new Vector3(0, JumpForce, 0), ForceMode.Impulse);
        }

        //�������͂��Ȃ����͒�R�𑝂₵�Ă������~�߂�
        // ���͂��Ȃ��Ƃ��ɂ������~�܂邽�߂Ƀh���b�O�𒲐�
        if (moveDirection == Vector3.zero)
        {
            rb.drag = dragValue;  // �h���b�O��K�p���Č���
        }
        else
        {
            rb.drag = 0;  // ���͂�����Ƃ��̓h���b�O���I�t�ɂ���
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
        lineRenderer.SetPositions(linePoints.ToArray()); // �I�u�W�F�N�g�̈ʒu�����Z�b�g
    }

    //���蕐��ˌ��{�^���������ꂽ�Ƃ�
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

    //�E�蕐��ˌ��{�^���������ꂽ�Ƃ�
    private void RightArmShot(InputAction.CallbackContext context)
    {
        RightArmWeapon.Shot(Target.position);

        
    }

    //�u�[�X�g�{�^���������ꂽ�Ƃ�
    private void StartBoost(InputAction.CallbackContext context)
    {
        print("�u�[�X�ƊJ�n");

        isBoosting = true;

        ArmatureAnimator.SetBool("IsBoosting", true);
    }

    //�u�[�X�g�{�^���������ꂽ�Ƃ�
    private void EndBoost(InputAction.CallbackContext context)
    {
        print("�u�[�X�ƏI��");

        isBoosting = false;

        ArmatureAnimator.SetBool("IsBoosting", false);

    }

    //�W�����v�{�^���������ꂽ�Ƃ�
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

    //�W�����v�{�^���������i�㏸�j�����ꂽ�Ƃ�
    private void StartRise(InputAction.CallbackContext context)
    {
        isRising = true;
    }

    private void EndRise(InputAction.CallbackContext context)
    {
        isRising= false;
    }

    // Y����]�݂̂�K�p����LookAt�֐�
    void CustomLookAtYAxis(Transform target)
    {
        // �^�[�Q�b�g�܂ł̕������v�Z����
        Vector3 directionToTarget = target.position - transform.position;

        // Y���̉�]���l�������x�N�g�����v�Z�iX-Z���ʂ̂݁j
        directionToTarget.y = 0;

        // �����x�N�g�����L���ȏꍇ�ɂ�����]���s���i�[���x�N�g��������j
        if (directionToTarget != Vector3.zero)
        {
            // Y���̉�]�𔽉f������
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            transform.rotation = targetRotation;
        }
    }

    //�q���̃R���C�_�[�ƏՓ˂��ău���u���k�����肷��̂ŉ������
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
