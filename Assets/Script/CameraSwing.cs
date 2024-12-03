using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NeoCameraSwing : MonoBehaviour
{
    //�J�������}�E�X�̓����ɍ��킹�ē�����
    public bool IsSwing=false;

    //�J����������̑Ώۂ�ǂ������邩
    public bool IsFollowing=false;

    //�v���C���[���^�[�Q�b�e�B���O���Ă��邩
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
                // ��]���Ă���̂ŁA���݂̃J�����ʒu���^�[�Q�b�g����̃I�t�Z�b�g�Ōv�Z
                Vector3 nowOffset = FollowTarget.rotation * FollowOffset;

                Vector3 desiredPosi = FollowTarget.position+nowOffset;

                // �J�����̈ʒu���X���[�Y�ɕ��
                Vector3 smoothedPosition = Vector3.Slerp(transform.position, desiredPosi, Smooth);

                // �J�����̈ʒu���X�V
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
