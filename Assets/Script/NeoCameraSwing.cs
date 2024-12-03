using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class CameraSwing : MonoBehaviour
{
    //�J�������}�E�X�̓����ɍ��킹�ē�����
    public bool IsSwing=false;

    //�J����������̑Ώۂ�ǂ������邩
    public bool IsFollowing=false;

    //�v���C���[���^�[�Q�b�e�B���O���Ă��邩
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
        //InputSystem���Z�b�g
        InputControls testControl= new InputControls();

        resetAction = testControl.Player.CameraReset;

        resetAction.performed += CameraReset;
    }

    // Start is called before the first frame update
    void Start()
    {
        //�I�t�Z�b�g�ݒ�
        if (FollowTarget)
        {
            DefaultFollowOffset= transform.position-FollowTarget.position;
            FollowOffset = DefaultFollowOffset;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //�J������]����
        if (IsSwing)
        {
            var pointer = Pointer.current;
            nowMousePosi = pointer.position.ReadValue();

            var diff = (nowMousePosi - lastMousePosi)*SwingSpeed;

            //transform.RotateAround()�����悤���ă��C���J��������]������
            transform.RotateAround(transform.position, Vector3.up, diff.x);
            transform.RotateAround(transform.position, transform.right, diff.y);

            //Vector3 rotation = transform.rotation.eulerAngles;

            //rotation = new Vector3(Mathf.Clamp(rotation.x,rotateMin.x,rotateMax.x), Mathf.Clamp(rotation.y, rotateMin.y, rotateMax.y), Mathf.Clamp(rotation.z, rotateMin.z, rotateMax.z));

            //transform.rotation=Quaternion.Euler(rotation);

            lastMousePosi = pointer.position.ReadValue();
        }

        //�J�����Ǐ]����
        if (IsFollowing)
        {
            //�^�[�Q�b�e�B���O����
            if (IsTargetting)
            {
                // ���ʂ̖@���x�N�g���i������x�N�g���Ƃ���j
                var planeNormal = Vector3.up;

                // ���ʂɓ��e���ꂽ�x�N�g�������߂�
                var flattenedForwardA = Vector3.ProjectOnPlane(transform.forward, planeNormal);
                var flattenedDirectionB = Vector3.ProjectOnPlane(FollowTarget.position - transform.position, planeNormal);

                //Debug.DrawLine(transform.position, transform.position + flattenedForwardA * 5f, Color.blue);  // �J�����̑O������ŕ\��
                //Debug.DrawLine(transform.position, transform.position + flattenedDirectionB * 5f, Color.red); // �L�����N�^�[�ւ̕�����Ԃŕ\��


                // objectA�̑O������objectB�ւ̕����x�N�g���Ԃ̊p�x���擾
                float angle = Vector3.SignedAngle(flattenedForwardA, flattenedDirectionB, Vector3.up); // SignedAngle�Ő������܂ފp�x���擾


                Vector3 nowOffset = Vector3.one;

                if (Mathf.Abs(angle)<AngleTest)
                {
                    
                }
                else if(!IsOutField)
                {
                    IsOutField = true;

                    // ���ʂɓ��e���ꂽ�x�N�g�������߂�
                    flattenedForwardA = Vector3.ProjectOnPlane(transform.forward, planeNormal);
                    flattenedDirectionB = Vector3.ProjectOnPlane(FollowTarget.position - transform.position, planeNormal);

                    // objectA�̑O������objectB�ւ̕����x�N�g���Ԃ̊p�x���擾
                    angle = Vector3.SignedAngle(flattenedForwardA, flattenedDirectionB, Vector3.up); // SignedAngle�Ő������܂ފp�x���擾

                    saveangle = (Mathf.Abs(angle)-5)*Mathf.Sign(angle);
                    //FollowOffset= transform.position - FollowTarget.position;
                }

                /*

                if (IsOutField)
                {
                    Debug.Log("X���𖳎��������Ίp�x: " + angle);

                    // ��]���Ă���̂ŁA���݂̃J�����ʒu���^�[�Q�b�g����̃I�t�Z�b�g�Ōv�Z
                    nowOffset = FollowTarget.rotation * DefaultFollowOffset;

                    Vector3 neoposition = FollowTarget.position + nowOffset;

                    nowOffset = Quaternion.Euler(new Vector3(0f, angle, 0f)) * nowOffset;
                }
                else
                {

                    // ��]���Ă���̂ŁA���݂̃J�����ʒu���^�[�Q�b�g����̃I�t�Z�b�g�Ōv�Z
                    nowOffset = Quaternion.Euler(new Vector3(0f, angle, 0f)) * FollowOffset;
                }

                */

                if (IsOutField)
                {

                    // ��]���Ă���̂ŁA���݂̃J�����ʒu���^�[�Q�b�g����̃I�t�Z�b�g�Ōv�Z
                    nowOffset = FollowTarget.rotation * DefaultFollowOffset;

                    nowOffset = Quaternion.Euler(new Vector3(0f, saveangle, 0f)) * nowOffset;


                    Debug.DrawLine(transform.position, transform.position - nowOffset, Color.magenta);


                    Vector3 desiredPosi = FollowTarget.position +  nowOffset;


                    // �J�����̈ʒu���X���[�Y�ɕ��
                    Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosi, Smooth);

                    // �J�����̈ʒu���X�V
                    transform.position = desiredPosi;
                    Vector3 directionToTarget = (chara.Target.position - transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    transform.rotation = targetRotation;

                    Debug.DrawLine(transform.position, desiredPosi, Color.green);



                    // ���ʂɓ��e���ꂽ�x�N�g�������߂�
                    flattenedForwardA = Vector3.ProjectOnPlane(transform.forward, planeNormal);
                    flattenedDirectionB = Vector3.ProjectOnPlane(FollowTarget.position - transform.position, planeNormal);

                    // objectA�̑O������objectB�ւ̕����x�N�g���Ԃ̊p�x���擾
                    angle = Vector3.SignedAngle(flattenedForwardA, flattenedDirectionB, Vector3.up); // SignedAngle�Ő������܂ފp�x���擾

                    IsOutField = false;

                    FollowOffset = nowOffset;


                    if (Vector3.Distance(transform.position,desiredPosi)<0.01f)
                    {
                        Debug.Log("���������I�I�I�I�I�I�I�I"+ Vector3.Distance(transform.position, desiredPosi));

                        
                        //FollowOffset = Quaternion.Euler(new Vector3(0f, angle, 0f)) * FollowOffset;
                    }
                }
                else
                {
                    Debug.DrawLine(transform.position, transform.position- FollowOffset, Color.red);

                    Debug.Log("X���𖳎��������Ίp�x: " + angle);
                    nowOffset = Quaternion.Euler(new Vector3(0f, angle, 0f)) * FollowOffset;

                    Debug.DrawLine(transform.position, transform.position - nowOffset, Color.blue);



                    Vector3 desiredPosi = FollowTarget.position + nowOffset;

                    // �J�����̈ʒu���X���[�Y�ɕ��
                    Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosi, Smooth);

                    // �J�����̈ʒu���X�V
                    transform.position = desiredPosi;

                    Vector3 directionToTarget = (chara.Target.position - transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                    transform.rotation = targetRotation;

                    // ���ʂɓ��e���ꂽ�x�N�g�������߂�
                    flattenedForwardA = Vector3.ProjectOnPlane(transform.forward, planeNormal);
                    flattenedDirectionB = Vector3.ProjectOnPlane(FollowTarget.position - transform.position, planeNormal);

                    // objectA�̑O������objectB�ւ̕����x�N�g���Ԃ̊p�x���擾
                    angle = Vector3.SignedAngle(flattenedForwardA, flattenedDirectionB, Vector3.up); // SignedAngle�Ő������܂ފp�x���擾

                    //Debug.Log("�ԊҌ�̑��Ίp�x: " + angle);

                }



                //�J��������Ώە��܂ł̊p�x���擾
                //Debug.Log(Quaternion.LookRotation(FollowTarget.position).eulerAngles.y);

            }
            else
            {
                transform.position = FollowTarget.position + FollowOffset;
            }
        }

        
    }

    //�Ǐ]�Ώۂ̐��ʂ������悤�ɃJ��������]
    private void CameraReset(InputAction.CallbackContext context)
    {
        if (IsTargetting) return;

        // ��]���Ă���̂ŁA���݂̃J�����ʒu���^�[�Q�b�g����̃I�t�Z�b�g�Ōv�Z
        FollowOffset = FollowTarget.rotation * DefaultFollowOffset;

        Vector3 desiredPosi = FollowTarget.position + FollowOffset;

        // �J�����̈ʒu���X���[�Y�ɕ��
        Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosi, Smooth);

        // �J�����̈ʒu���X�V
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
