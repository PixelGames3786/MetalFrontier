using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UIElements;
using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine.Rendering;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

public class RobotPlayerInput : MonoBehaviour,IDamageable
{
    private MissionManager missionManager;

    //�ʏ�J�����ƃ��b�N�I���J����
    [SerializeField]
    private CinemachineVirtualCamera normalCam, TargetCam;

    //���K�V�[�R���g���[���[
    public RobotController controller { get; private set; }
    private PlayerUIController uiController;

    // 2�����͂��󂯎��z���Action
    private InputAction moveAction; 
    
    //�e����ʒu�̒����������m����Action
    private Dictionary<InputAction, Action<InputAction.CallbackContext>> actionMap; //InputAction�ƑΉ�����֐������鎫�� 1��0��Action�̂�
    private Dictionary<InputAction, Action<InputAction.CallbackContext>> canselActionMap;

    //���͈͓��̓G���擾���邽�߂̔��a
    public float enemyRadius,enemyRockOffRadius,rotateFactor,camResetTime;

    //���b�N�I���͈͂ɓ����Ă���^�[�Q�b�g�ꗗ
    private List<Transform> targets=new List<Transform>();

    //�ł��߂��G
    public Transform mostNearEnemy { get; private set; }

    private Transform lookObj,target;
    private Vector3 camFrontVector;

    //���풷�����t���O
    private bool IsLArmLongPress,IsRArmLongPress,IsLShoulderLongPress,IsRShoulderLongPress;

    private bool camReseting,isFindingTarget=true;

    private bool isWorking=false;

    //�G����������ۂ̃^�O
    public string enemyTag;

    //�f���Q�[�g��
    public Action<string> OnDeathWithName;
    public Action<int> OutOfAreaCountDown;

    //�~�b�V�����͈͊O�ɏo���ۂ̃J�E���g�_�E���@10�b�O�ɂ���ƃ~�b�V�������s
    private bool IsOutOfArea;
    private int maxOutOfAreaCount=20,outOfAreaCount;
    private Coroutine countdownCoroutine; //�R���[�`���̎Q�Ƃ�ێ�

    //�������l���\�ȃ\�[�X
    public HackSlashSource hackSource { get; private set; }

    public GameObject awakeParticleParent,deathParticlePrefab;
    public ParticleSystem awakeParticlePrefab;

    private List<ParticleSystem> awakeParticles=new List<ParticleSystem>();

    public Volume postProcessVolume; //�|�X�g�v���Z�X��Volume


    // Start is called before the first frame update
    void Awake()
    {
        missionManager = FindObjectOfType<MissionManager>();

        controller = GetComponent<RobotController>();
        uiController = FindObjectOfType<PlayerUIController>();

        RegisterAction();

        InputControls testControl = new InputControls();

        controller.onStartBoost += StartBoostEffect;
    }

    private void Start()
    {
        outOfAreaCount = maxOutOfAreaCount;

        //�����_��ݒ�
        Vector3 screenPoint = new Vector3(Screen.width / 2, Screen.height / 2, 5000);
        Vector3 lookPosi = Camera.main.ScreenToWorldPoint(screenPoint);

        camFrontVector = lookPosi;

        lookObj = new GameObject("LookObject").transform;

        AwakeParticleSetUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isWorking) return;

        Vector2 inputVec = moveAction.ReadValue<Vector2>();
        controller.moveDirInput(inputVec);

        //�͈͓��̓G���`�F�b�N
        CheckEnemyInRange();
        CheckEnemyOutRange();
        GetMostNearEnemy();

        //RotateToTarget(inputVec);

        foreach (Transform trans in targets)
        {
            Debug.DrawLine(transform.position, trans.position, Color.red);
        }

        LookToCameraPoint();

        //����𒷉������Ȃ�
        if (IsLArmLongPress) controller.LeftArmShot();
        if (IsRArmLongPress) controller.RightArmShot();
        if (IsLShoulderLongPress) controller.LeftShoulderShot();
        if (IsRShoulderLongPress) controller.RightShoulderShot();

    }

    //���͈͓��ɓG�����Ȃ����`�F�b�N���ă��b�N�I������
    public void CheckEnemyInRange()
    {
        //�������^�[�Q�b�e�B���O���Ă�����s��Ȃ�
        RaycastHit[] hits = Physics.SphereCastAll(transform.position,enemyRadius,Vector3.up,0);
        List<Transform> enemiesInRange = new List<Transform>();

        foreach (var hit in hits)
        {
            //�G�^�O�����Ă��邩�m�F
            if (hit.collider.CompareTag(enemyTag))
            {
                ITargetable targetable = hit.collider.GetComponent<ITargetable>();

                if (targetable==null)
                {
                    continue;
                }

                //���Ƀ^�[�Q�b�e�B���O���Ă��Ȃ������݃^�[�Q�b�e�B���O�\�Ȃ�΃^�[�Q�b�e�B���O����
                if (!targets.Contains(hit.transform) && targetable.CanTarget() && !CheckObstacle(hit.transform))
                {
                    targets.Add(hit.transform);
                    uiController.SetRockOnUI(targets);
                }
            }
        }

        //TargetCam.LookAt = mostNearEnemy.transform;
        //TargetCam.Priority = 11;
    }

    //�^�[�Q�b�e�B���O�\���ǂ����`�F�b�N����@�ǂȂǂɎՂ��Ă�����^�[�Q�b�e�B���O�ł��Ȃ��悤�ɂ���
    private bool CheckObstacle(Transform tag)
    {
        LayerMask obstacleLayer = LayerMask.GetMask("Field");
        bool isObstacle = Physics.Linecast(transform.position, tag.position, out RaycastHit hit, obstacleLayer);

        if (isObstacle)
        {
            //print(hit.collider.name);
        }

        return isObstacle;
    }

    //���b�N�I�������G���͈͊O�ɏo���烍�b�N�I�����O��
    private void CheckEnemyOutRange()
    {
        List<Transform> removeList = new List<Transform>();

        foreach (Transform enemy in targets)
        {
            //�����J�����O�ɏo�Ă���Ȃ�ⓚ���p�Ń^�[�Q�b�g����O��
            if (!enemy.GetComponent<ITargetable>().IsVisible())
            {
                removeList.Add(enemy);

                continue;
            }

            //�^�[�Q�b�g�s�\�ɂȂ��Ă�����O��
            if (!enemy.GetComponent<ITargetable>().CanTarget())
            {
                removeList.Add(enemy);
                continue;
            }

            //�Ԃɏ�Q��������ΊO��
            if (CheckObstacle(enemy.transform))
            {
                removeList.Add(enemy);
                continue;
            }

            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance>enemyRockOffRadius)
            {
                removeList.Add(enemy);
            }
        }

        if (removeList.Count == 0) return;

        foreach (Transform remove in removeList)
        {
            targets.Remove(remove);
        }

        //�^�[�Q�b�g�ꗗ�ɉ��炩�̕ύX���������Ȃ�^�[�Q�b�gUI�̍X�V���s��
        uiController.SetRockOnUI(targets);
    }

    //���b�N�I�����Ă���G�̒��ōł��߂��G���擾
    private void GetMostNearEnemy()
    {
        if (targets.Count == 0)
        {
            mostNearEnemy = null;
            controller.SetTarget(null);
            return;
        }

        Transform mostNear = targets[0];
        //�͈͓��ōł��߂��G�����
        float distance = Vector3.Distance(transform.position, mostNear.transform.position);

        foreach (Transform enemy in targets)
        {
            float dis = Vector3.Distance(transform.position, enemy.transform.position);

            if (dis < distance)
            {
                mostNear = enemy;
                distance = dis;
            }
        }

        //�������ł��߂��G���ς���Ă�����f���Q�[�g���Ă�
        if (mostNear!=mostNearEnemy)
        {
            mostNearEnemy = mostNear;

            controller.SetTarget(mostNearEnemy);
        }
    }

    void OnDrawGizmosSelected()
    {
        // �V�[���r���[�Ŕ͈͂�����
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyRadius);
    } //���G�͈͂�UI��ŕ\������

    //�^�[�Q�b�g�̕���������
    private void RotateToTarget(Vector2 input)
    {
        if (target)
        {
            if (camReseting) return;

            lookObj.position = camFrontVector;
            normalCam.LookAt = lookObj;
        }
        else
        {
            transform.Rotate(Vector3.up, input.x * rotateFactor);
        }
    }

    //���K�V�[�̃f�[�^�Ȃǂ��Z�b�g�A�b�v����
    public void LegacySetUp()
    {
        //�f�[�^�擾
        LegacySettingData settingData = SaveDataManager.instance.saveData.settingData;

        //�{�f�B�p�[�c���f
        List<BodyPartsData> bodyParts = new List<BodyPartsData>();
        Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData> weaponParts = new Dictionary<LegacySettingData.WeaponSetPosi, WeaponPartsData>();

        //�S�p�[�c�̃f�[�^�擾
        foreach (KeyValuePair<BodyPartsData.PartsType, HavingItem> keyValue in settingData.PartsNumber)
        {
            BodyPartsData Data = (BodyPartsData)keyValue.Value.itemData;
            bodyParts.Add(Data);
        }

        //����p�[�c���f
        //�p�[�c�f�[�^�擾
        foreach (KeyValuePair<LegacySettingData.WeaponSetPosi, HavingItem> keyValue in settingData.WeaponsNumber)
        {
            if (keyValue.Value == null) continue;

            WeaponPartsData data = (WeaponPartsData)keyValue.Value.itemData;
            weaponParts[keyValue.Key]=data;
        }

        controller.setupControl.LegacySetUp(bodyParts,weaponParts);
        controller.statusControl.StatusInitalize(bodyParts);

        uiController.UIInitlaize(controller.statusControl);
    }

    //�o���p�[�e�B�N���̃Z�b�g�A�b�v
    private void AwakeParticleSetUp()
    {
        MeshRenderer[] meshRenderers = transform.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer renderer in meshRenderers)
        {
            ParticleSystem particle = Instantiate(awakeParticlePrefab, awakeParticleParent.transform);
            awakeParticles.Add(particle);

            particle.Stop();

            var shapeModule = particle.shape;

            shapeModule.meshRenderer = renderer;
        }

        controller.onAwakeStart += () =>
        {
            foreach (var particle in awakeParticles)
            {
                particle.Play();
            }
        };
        controller.onAwakeEnd += () => 
        {
            foreach (var particle in awakeParticles)
            {
                particle.Stop();
            }
        };
    }

    //�����Ԃ�ύX
    public void SetWorking(bool val)
    {
        isWorking = val;
    }

    //���K�V�[�̑���\�t���O��ύX
    public void SetOparete(bool val)
    {
        controller.canOperate = val;
    }

    //InputAction�̓o�^���s��
    public void RegisterAction()
    {
        InputControls testControl = new InputControls();

        moveAction = testControl.Player.Move;

        actionMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>
        {
            { testControl.Player.LeftArmPress, LeftArmShot },
            { testControl.Player.RightArmPress, RightArmShot },
            { testControl.Player.LeftShoulderPress, LeftShoulderShot },
            { testControl.Player.RightShoulderPress, RightShoulderShot },
            { testControl.Player.LeftArmLongPress,StartLeftArmLongPress},
            { testControl.Player.RightArmLongPress,StartRightArmLongPress},
            { testControl.Player.LeftShoulderLongPress,StartLeftShoulderLongPress},
            { testControl.Player.RightShoulderLongPress,StartRightShoulderLongPress},
            { testControl.Player.Boost, StartBoost },
            { testControl.Player.Jump, OnJump },
            { testControl.Player.Rise, StartRise },
            { testControl.Player.Awake,ChangeAwake},
            { testControl.Player.CameraReset,CameraReset},
        };

        canselActionMap = new Dictionary<InputAction, Action<InputAction.CallbackContext>>
        {
            { testControl.Player.LeftArmLongPress,EndLeftArmLongPress},
            { testControl.Player.RightArmLongPress,EndRightArmLongPress},
            { testControl.Player.LeftShoulderLongPress,EndLeftShoulderLongPress},
            { testControl.Player.RightShoulderLongPress,EndRightShoulderLongPress},
            { testControl.Player.Boost,EndBoost},
            { testControl.Player.Rise,EndRise},
        };

        //���ׂẴA�N�V�������ꊇ�o�^(performed)
        foreach (var entry in actionMap)
        {
            entry.Key.performed += entry.Value;
        }

        //�S�ẴA�N�V�������ꊇ�o�^(canseled)
        foreach (var entry in canselActionMap)
        {
            entry.Key.canceled += entry.Value;
        }
    }

    //�_���[�W���󂯂�
    public void Damage(AttackData attack)
    {
        //�U���^�C�v�Ƒϐ����l�����ă_���[�W������
        float damage = attack.damage;

        switch (attack.type)
        {
            case WeaponPartsData.AttackType.Physicial:

                damage = damage / (1+controller.statusControl.nowStatus.physicalRes);

                break;

            case WeaponPartsData.AttackType.Beam:

                damage = damage / (1 + controller.statusControl.nowStatus.beamRes);

                break;
        }

        controller.statusControl.HPChange(-damage);

        //���S����
        if (controller.statusControl.hp <= 0)
        {
            print("���񂾂��I");
            Die();
        }
    }

    //�u�[�X�g���n�߂�ۂɃ|�X�g�G�t�F�N�g��������
    private void StartBoostEffect()
    {
        ChromaticAberration chroma;

        //Volume����Chromatic���擾
        if (postProcessVolume.profile.TryGet(out chroma))
        {
            Sequence sequence = DOTween.Sequence();

            //DOTween��Bloom��intensity��ύX
            Tween startTween= DOTween.To(() => chroma.intensity.value,
                       x => chroma.intensity.value = x,
                       0.7f,
                       0.3f);

            Tween endTween = DOTween.To(() => chroma.intensity.value,
                       x => chroma.intensity.value = x,
                       0f,
                       0.3f);

            sequence.Append(startTween).Append(endTween);

            sequence.Play();
        }
    }

    //���S����
    private void Die()
    {
        SetWorking(false);
        SetOparete(false);
        controller.SetArmLookAt(false);

        //�f���Q�[�g�̌Ăяo��
        OnDeathWithName?.Invoke(gameObject.name);

        //���S���p�[�e�B�N��
        GameObject particle = Instantiate(deathParticlePrefab);
        particle.transform.position = transform.position;

        controller.Die();

        //���b��ɔj��
        //Destroy(gameObject, 5f);
    }

    IEnumerator CountdownCoroutine()
    {
        while (outOfAreaCount > 0)
        {
            outOfAreaCount--;
            OutOfAreaCountDown?.Invoke(outOfAreaCount); //�o�^�����f���Q�[�g���Ăяo��

            yield return new WaitForSeconds(1f); // 1�b�ҋ@
        }

        //�C�����s
        FindObjectOfType<MissionManager>().MissionFail();
    }

    private void OnTriggerExit(Collider other)
    {
        //���G���A�O�ɏo���Ȃ�
        if (other.tag=="OperationArea")
        {
            IsOutOfArea = true;

            uiController.StartCountDown();

            if (countdownCoroutine==null)
            {
                countdownCoroutine = StartCoroutine(CountdownCoroutine());
            }
        }

        //���ő�G���A�O�ɏo���Ȃ瑦���Ƀ~�b�V�������s
        if (other.tag=="OperationMaxArea")
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            //�C�����s
            FindObjectOfType<MissionManager>().MissionFail();
        }

        //�������l���\�ȃG���A���o���Ȃ�l��UI���\���ɂ���
        if (other.tag == "HackSlashSource")
        {
            hackSource = null;

            uiController.CloseHackSlashUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //���G���A���ɓ������Ȃ�
        if (other.tag == "OperationArea")
        {
            IsOutOfArea = false;

            uiController.EndCountDown();
            if (countdownCoroutine != null) 
            { 
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            outOfAreaCount = maxOutOfAreaCount;
        }

        //�������l���\�ȃG���A�ɓ������Ȃ�
        if (other.tag=="HackSlashSource")
        {
            hackSource = other.GetComponent<HackSlashSource>();

            if (!hackSource.isGeted)
            {
                uiController.OpenHackSlashUI(hackSource.selectItemList,hackSource);
            }
            else
            {
                hackSource = null;
            }
        }
    }

    //���蕐��ˌ��{�^���������ꂽ�Ƃ�
    private void LeftArmShot(InputAction.CallbackContext context)=>controller.LeftArmShot();

    //�E�蕐��ˌ��{�^���������ꂽ�Ƃ�
    private void RightArmShot(InputAction.CallbackContext context)=>controller.RightArmShot();

    //��������g�p�{�^���������ꂽ�Ƃ�
    private void LeftShoulderShot(InputAction.CallbackContext context)=> controller.LeftShoulderShot();

    //�E������g�p�{�^���������ꂽ�Ƃ�
    private void RightShoulderShot(InputAction.CallbackContext context)=> controller.RightShoulderShot();

    //���r���풷�����J�n/�I��
    private void StartLeftArmLongPress(InputAction.CallbackContext context)=> IsLArmLongPress = true;
    private void EndLeftArmLongPress(InputAction.CallbackContext context)=> IsLArmLongPress = false;

    //�E�r���풷�����J�n/�I��
    private void StartRightArmLongPress(InputAction.CallbackContext context) => IsRArmLongPress = true;
    private void EndRightArmLongPress(InputAction.CallbackContext context) => IsRArmLongPress = false;

    //�������풷�����J�n/�I��
    private void StartLeftShoulderLongPress(InputAction.CallbackContext context) => IsLShoulderLongPress = true;
    private void EndLeftShoulderLongPress(InputAction.CallbackContext context) => IsLShoulderLongPress = false;

    //�E�����풷�����J�n/�I��
    private void StartRightShoulderLongPress(InputAction.CallbackContext context) => IsRShoulderLongPress = true;
    private void EndRightShoulderLongPress(InputAction.CallbackContext context) => IsRShoulderLongPress = false;

    //�u�[�X�g�{�^���������J�n/�I��
    private void StartBoost(InputAction.CallbackContext context)=> controller.StartBoost();
    private void EndBoost(InputAction.CallbackContext context)=> controller.EndBoost();

    //�W�����v�{�^���������ꂽ�Ƃ�
    private void OnJump(InputAction.CallbackContext context)
    {
        controller.OnJump();
    }

    //�W�����v�{�^���������i�㏸�j�����ꂽ�Ƃ�
    private void StartRise(InputAction.CallbackContext context)
    {
        controller.StartRise();
    }

    //�W�����v�{�^�����������I�������
    private void EndRise(InputAction.CallbackContext context)
    {
        controller.EndRise();
    }

    //�o���{�^���������ꂽ��
    private void ChangeAwake(InputAction.CallbackContext context)=> controller.AwakeChange();

    private void CameraReset(InputAction.CallbackContext context)
    {
        CameraReset();
    }

    private void LookToCameraPoint()
    {
        //�����_��ݒ�
        Vector3 screenPoint = new Vector3(Screen.width / 2, Screen.height / 2, 5000);
        Vector3 lookPosi = Camera.main.ScreenToWorldPoint(screenPoint);

        // Y���𖳎����邽�߂ɒ����_��Y���W�����݂̃I�u�W�F�N�g��Y���W�ɌŒ�
        lookPosi.y = transform.position.y;

        transform.LookAt(lookPosi);
    }

    //�J�����������Z�b�g
    public async void CameraReset()
    {
        if (!target)
        {
            isFindingTarget = true;

            camReseting = true;

            Vector3 forwardNormal = transform.forward.normalized;
            camFrontVector = new Vector3(forwardNormal.x * 5000, camFrontVector.y, forwardNormal.z * 5000);

            //lookObj.position = camFrontVector;
            //normalCam.LookAt = lookObj;

            Debug.DrawRay(transform.position, camFrontVector, Color.red);
            Debug.DrawRay(transform.position, transform.forward, Color.blue);

            await UniTask.Delay(Mathf.CeilToInt(camResetTime * 1000));

            camReseting = false;
        }
        else
        {
            print("�J�������Z�b�g");

            isFindingTarget = false;

            target = null;

            //���b�N�I������
            controller.statusControl.SetTarget(null);
            controller.SetTarget(null);

            uiController.SetRockOnUI(null);

            // �ʏ�J�����Ƀ��b�N�I���J�����Ɠ����ʒu�ƌ������Z�b�g
            CinemachinePOV pov = normalCam.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
            {
                // lockOnCamera�̌��݂̌��������Ƃ�POV�̒l��ݒ�
                pov.m_HorizontalAxis.Value = transform.eulerAngles.y;
                pov.m_VerticalAxis.Value = 20;
            }

            TargetCam.LookAt = null;
            TargetCam.Priority = 9;

        }

    }


    //InputAction�̗L�����Ȃ�
    private void OnDestroy()
    {
        moveAction.Dispose();

        foreach (var entry in actionMap)
        {
            entry.Key.Dispose();
        }
        foreach (var entry in canselActionMap)
        {
            entry.Key.Dispose();
        }
    }

    private void OnEnable()
    {
        moveAction.Enable();

        foreach (var entry in actionMap)
        {
            entry.Key.Enable();
        }
        foreach (var entry in canselActionMap)
        {
            entry.Key.Enable();
        }
    }

    private void OnDisable()
    {
        moveAction.Disable();

        foreach (var entry in actionMap)
        {
            entry.Key.Disable();
        }
        foreach (var entry in canselActionMap)
        {
            entry.Key.Disable();
        }
    }

}
