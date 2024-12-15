using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System;
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

    //�^�[�Q�b�g�����Ȃ��ۂɃ��{�b�g�̎����̐�ɔz�u����I�u�W�F�N�g�@CinemachineCamera�ɓ������
    private Transform target;

    public Animator armatureAnimator;

    //�p������n
    [SerializeField]
    private RobotShoulderControl leftShoulder, rightShoulder;

    [SerializeField]
    private RobotArmControl leftArm, rightArm;

    //����n
    public WeaponBase LeftArmWeapon { get; private set; }
    public WeaponBase RightArmWeapon { get; private set; }
    public WeaponBase LShoulderWeapon { get; private set; }
    public WeaponBase RShoulderWeapon { get; private set; }

    public float dragValue,rotateSpeed;

    [SerializeField]
    private bool lookTargetSetting;

    [SerializeField]
    private bool isBoosting,isRising,isInAir;//�u�[�X�g���@�㏸���@�󒆂ɂ��邩

    [SerializeField]
    private bool canJump = true,camReseting; //�J�������Z�b�g�����@����\��Ԃ�
    public bool canBoost = true,canOperate; //�u�[�X�g�\��

    private bool isAwakening; //�o������
    public event Action onAwakeStart,onAwakeEnd;

    public event Action onDied,onStartBoost;

    private Vector2 moveInput;
    private Vector3 moveDirection;

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
        if (!canOperate) return; //����s�\��ԂȂ�΍s��Ȃ�

        status = statusControl.nowStatus;

        //�����Ă�������Ɋ�Â��Ĉړ��x�N�g�����v�Z
        moveDirection = ((transform.forward * moveInput.y) + (transform.right * moveInput.x)).normalized;

        //�^�[�Q�b�e�B���O�����ǂ���
        if (target && lookTargetSetting)
        {
            //�^�[�Q�b�g�̕�������
            CustomLookAtYAxis(target);
            //transform.LookAt(Target.position);
        }

        //�o�����Ȃ�Ώ��X�Ɋo���ʂ����炵�Ă�
        if (isAwakening)
        {
            statusControl.QuorraUse(Time.deltaTime*status.quorraUseRate);
        }
        else
        {
            statusControl.QuorraRecover(Time.deltaTime*status.quorraRecoverRate);
        }
    }

    private void FixedUpdate()
    {
        if (!canOperate) return; //����s�\��ԂȂ�΍s��Ȃ�

        Move();
    }

    private void Move()
    {
        Vector3 velocity = new Vector3();

        if (isBoosting)
        {
            rb.AddForce(moveDirection * status.boostSpeed * Time.deltaTime*5, ForceMode.Impulse);

            //�ő�ړ��ʂ𐧌�
            velocity.x = Mathf.Clamp(rb.velocity.x, status.boostMaxVel.x * -1, status.boostMaxVel.x);
            velocity.y = Mathf.Clamp(rb.velocity.y, status.boostMaxVel.y * -1, status.boostMaxVel.y);
            velocity.z = Mathf.Clamp(rb.velocity.z, status.boostMaxVel.z * -1, status.boostMaxVel.z);

            //�u�[�X�g�g�p�ʂ��X�e�[�^�X����v�Z
            float boostUsage = Time.deltaTime * statusControl.nowStatus.boostUseRate;

            //�g�p�ς݃u�[�X�g�ʂ𑝂₷
            statusControl.BoostUse(boostUsage);
        }
        else
        {
            rb.AddForce(moveDirection * status.moveSpeed * Time.deltaTime, ForceMode.Impulse);

            //�ő�ړ��ʂ𐧌�
            velocity.x = Mathf.Clamp(rb.velocity.x, status.maxVel.x * -1, status.maxVel.x);
            velocity.y = Mathf.Clamp(rb.velocity.y, status.maxVel.y * -1, status.maxVel.y);
            velocity.z = Mathf.Clamp(rb.velocity.z, status.maxVel.z * -1, status.maxVel.z);

        }

        //�㏸���Ȃ��
        if (isRising)
        {
            rb.AddForce(new Vector3(0, status.riseForce * Time.deltaTime, 0), ForceMode.Impulse);

            //�u�[�X�g�g�p�ʂ��X�e�[�^�X����v�Z
            float boostUsage = Time.deltaTime * statusControl.nowStatus.boostUseRate;

            //�g�p�ς݃u�[�X�g�ʂ𑝂₷
            statusControl.BoostUse(boostUsage);
        }

        //�u�[�X�g�����㏸���ȊO�̎��Ƀu�[�X�g�ʂ���
        if (!isRising && !isBoosting)
        {
            float boostRecover = Time.deltaTime * statusControl.nowStatus.boostRecoverRate;

            //�u�[�X�g�ʂ���
            statusControl.BoostRecover(boostRecover);
        }

        //�������͂��Ȃ����͒�R�𑝂₵�Ă������~�߂�
        if (moveDirection == Vector3.zero && !isInAir)
        {
            rb.drag = dragValue;  //�h���b�O��K�p���Č���

            armatureAnimator.SetBool("IsMoving", false);
        }
        else
        {
            rb.drag = 0;  //���͂�����Ƃ��̓h���b�O���I�t�ɂ���

            armatureAnimator.SetBool("IsMoving", true);
        }

        rb.velocity = velocity;
    }

    //�^�[�Q�b�g�ݒ�
    public void SetTarget(Transform tag)
    {
        target = tag;

        leftShoulder.target = tag;
        rightShoulder.target = tag;
        leftArm.target = tag;
        rightArm.target = tag;
    }

    //�r�⌨���G�������悤�ɐݒ�
    public void SetArmLookAt(bool val)
    {
        leftShoulder.SetLookAt(val);
        rightShoulder.SetLookAt(val);
        leftArm.SetLookAt(val);
        rightArm.SetLookAt(val);
    }


    public Vector3 TargetVector()
    {
        return (transform.position - target.position).normalized;
    }

    public void moveDirInput(Vector2 input)
    {
        moveInput = input;

        armatureAnimator.SetFloat("Move_h", moveInput.x);
        armatureAnimator.SetFloat("Move_v", moveInput.y);
    }

    //���S������
    public void Die()
    {
        onDied?.Invoke();

        armatureAnimator.SetBool("IsDied",true);

        GetComponent<CapsuleCollider>().radius = 1.8f;

        rb.constraints = RigidbodyConstraints.None;

        rb.AddTorque(transform.forward*-1f * 500f,ForceMode.VelocityChange); //���S���Ɍ��ɓ|���
    }

    //���蕐��ˌ�
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

    //�E�蕐��ˌ�
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

    //��������g�p
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

    //�E������g�p
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

    //�o����ԕύX
    public void AwakeChange()
    {
        if (isAwakening)
        {
            AwakeEnd();
        }
        else
        {
            AwakeStart();
        }
    }

    //�o����Ԃ���o��
    public void AwakeEnd()
    {
        onAwakeEnd?.Invoke();

        isAwakening = false;
        statusControl.EndAwake();
    }

    //�o����Ԃɓ���
    public void AwakeStart()
    {
        onAwakeStart?.Invoke();

        isAwakening = true;
        statusControl.StartAwake();
    }

    //�u�[�X�g�J�n
    public void StartBoost()
    {
        if (!canBoost) return;

        print("�u�[�X�ƊJ�n");

        isBoosting = true;

        armatureAnimator.SetBool("IsBoosting", true);

        onStartBoost?.Invoke();
    }

    //�u�[�X�g�I��
    public void EndBoost()
    {
        print("�u�[�X�ƏI��");

        isBoosting = false;

        armatureAnimator.SetBool("IsBoosting", false);
    }

    //�W�����v
    public void OnJump()
    {
        if (!isInAir)
        {
            rb.AddForce(new Vector3(0, status.jumpForce, 0), ForceMode.Impulse);

            canJump = false;
            isInAir = true;

            armatureAnimator.SetBool("IsInAir",true);
        }
    }

    //�W�����v�{�^���������i�㏸�j�����ꂽ�Ƃ�
    public void StartRise()
    {
        if (!canBoost) return;

        isRising = true;
    }

    public void EndRise()
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

            // ���݂̉�]�ƃ^�[�Q�b�g�̉�]��⊮���ď��X�ɉ�]
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
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
            canJump = true;
            isInAir = false;

            armatureAnimator.SetBool("IsInAir", false);
        }
    }

}
